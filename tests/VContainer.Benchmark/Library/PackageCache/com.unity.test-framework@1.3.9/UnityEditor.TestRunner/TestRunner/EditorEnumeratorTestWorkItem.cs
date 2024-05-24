using System;
using System.Collections;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditorEnumeratorTestWorkItem : UnityWorkItem
    {
        private TestCommand m_Command;
        public EditorEnumeratorTestWorkItem(TestMethod test, ITestFilter filter)
            : base(test, null)
        {
            m_Command = TestCommandBuilder.BuildTestCommand(test, filter);
        }

        private static IEnumerableTestMethodCommand FindFirstIEnumerableTestMethodCommand(TestCommand command)
        {
            if (command == null)
            {
                return null;
            }

            if (command is IEnumerableTestMethodCommand)
            {
                return (IEnumerableTestMethodCommand)command;
            }

            if (command is DelegatingTestCommand)
            {
                var delegatingTestCommand = (DelegatingTestCommand)command;
                return FindFirstIEnumerableTestMethodCommand(delegatingTestCommand.GetInnerCommand());
            }
            return null;
        }

        protected override IEnumerable PerformWork()
        {
            if (IsCancelledRun())
            {
                yield break;
            }

            if (m_DontRunRestoringResult)
            {
                if (EditModeTestCallbacks.RestoringTestContext == null)
                {
                    throw new NullReferenceException("RestoringTestContext is not set");
                }
                EditModeTestCallbacks.RestoringTestContext();
                Result = Context.CurrentResult;
                yield break;
            }

            try
            {
                if (IsCancelledRun())
                {
                    yield break;
                }

                if (m_Command is SkipCommand)
                {
                    m_Command.Execute(Context);
                    Result = Context.CurrentResult;
                    yield break;
                }

                //Check if we can execute this test
                var firstEnumerableCommand = FindFirstIEnumerableTestMethodCommand(m_Command);
                if (firstEnumerableCommand == null)
                {
                    Context.CurrentResult.SetResult(ResultState.Error, "Returning IEnumerator but not using test attribute supporting this");
                    yield break;
                }
                if (Context.TestCaseTimeout == 0)
                {
                    Context.TestCaseTimeout = k_DefaultTimeout;
                }
                if (m_Command.Test.Method.ReturnType.IsType(typeof(IEnumerator)))
                {
                    if (m_Command is ApplyChangesToContextCommand)
                    {
                        var applyChangesToContextCommand = ((ApplyChangesToContextCommand)m_Command);
                        applyChangesToContextCommand.ApplyChanges(Context);
                        m_Command = applyChangesToContextCommand.GetInnerCommand();
                    }

                    var innerCommand = m_Command as IEnumerableTestMethodCommand;
                    if (innerCommand == null)
                    {
                        Debug.Log("failed getting innerCommand");
                        throw new Exception("Tests returning IEnumerator can only use test attributes handling those");
                    }

                    foreach (var workItemStep in innerCommand.ExecuteEnumerable(Context))
                    {
                        if (IsCancelledRun())
                        {
                            yield break;
                        }

                        if (workItemStep is TestEnumerator)
                        {
                            if (EnumeratorStepHelper.UpdateEnumeratorPcIfNeeded(TestEnumerator.Enumerator))
                            {
                                yield return new RestoreTestContextAfterDomainReload();
                            }
                            continue;
                        }

                        if (workItemStep is AsyncOperation)
                        {
                            var asyncOperation = (AsyncOperation)workItemStep;
                            while (!asyncOperation.isDone)
                            {
                                if (IsCancelledRun())
                                {
                                    yield break;
                                }

                                yield return null;
                            }
                            continue;
                        }

                        ResultedInDomainReload = false;

                        if (workItemStep is IEditModeTestYieldInstruction)
                        {
                            var editModeTestYieldInstruction = (IEditModeTestYieldInstruction)workItemStep;
                            yield return editModeTestYieldInstruction;
                            var enumerator = editModeTestYieldInstruction.Perform();
                            while (true)
                            {
                                bool moveNext;
                                try
                                {
                                    moveNext = enumerator.MoveNext();
                                }
                                catch (Exception e)
                                {
                                    Context.CurrentResult.RecordException(e);
                                    break;
                                }

                                if (!moveNext)
                                {
                                    break;
                                }
                                yield return null;
                            }
                        }
                        else
                        {
                            yield return workItemStep;
                        }
                    }

                    Result = Context.CurrentResult;
                    EditorApplication.isPlaying = false;
                    yield return null;
                }
            }
            finally
            {
                WorkItemComplete();
            }
        }

        private bool IsCancelledRun()
        {
            return Context.ExecutionStatus == TestExecutionStatus.AbortRequested || Context.ExecutionStatus == TestExecutionStatus.StopRequested;
        }
    }
}
