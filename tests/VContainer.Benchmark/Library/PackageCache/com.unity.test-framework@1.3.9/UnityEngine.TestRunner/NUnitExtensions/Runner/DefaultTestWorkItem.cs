using System;
using System.Collections;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestTools;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal class EditModeTestCallbacks
    {
        public static Action RestoringTestContext { get; set; }
    }

    internal class DefaultTestWorkItem : UnityWorkItem
    {
        private TestCommand _command;
        public DefaultTestWorkItem(TestMethod test, ITestFilter filter)
            : base(test, null)
        {
            _command = TestCommandBuilder.BuildTestCommand(test, filter);
        }

        protected override IEnumerable PerformWork()
        {
            if (m_DontRunRestoringResult && EditModeTestCallbacks.RestoringTestContext != null)
            {
                EditModeTestCallbacks.RestoringTestContext();
                Result = Context.CurrentResult;
                yield break;
            }

            try
            {
                if (_command is SkipCommand || _command is FailCommand)
                {
                    Result = _command.Execute(Context);
                    yield break;
                }

                if (!(_command is IEnumerableTestMethodCommand))
                {
                    Debug.LogError("Cannot perform work on " + _command.GetType().Name);
                    yield break;
                }
                if (Context.TestCaseTimeout == 0)
                {
                    Context.TestCaseTimeout = k_DefaultTimeout;
                }
                foreach (var workItemStep in ((IEnumerableTestMethodCommand)_command).ExecuteEnumerable(Context))
                {
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
            }
            finally
            {
                WorkItemComplete();
            }
        }
    }
}
