using System;
using System.Collections;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using Unity.Profiling;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestTools
{
    internal class EnumerableTestMethodCommand : TestCommand, IEnumerableTestMethodCommand
    {
        private readonly TestMethod testMethod;

        public EnumerableTestMethodCommand(TestMethod testMethod)
            : base(testMethod)
        {
            this.testMethod = testMethod;
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            var unityContext = (UnityTestExecutionContext) context;
            yield return null;

            IEnumerator currentExecutingTestEnumerator;
            try
            {
                currentExecutingTestEnumerator = new TestEnumeratorWrapper(testMethod).GetEnumerator(context);
            }
            catch (Exception ex)
            {
                context.CurrentResult.RecordException(ex);
                yield break;
            }

            if (currentExecutingTestEnumerator != null)
            {
                var testEnumeraterYieldInstruction = new TestEnumerator(context, currentExecutingTestEnumerator);

                yield return testEnumeraterYieldInstruction;

                var enumerator = testEnumeraterYieldInstruction.Execute();

                var executingEnumerator = ExecuteEnumerableAndRecordExceptions(enumerator, new EnumeratorContext(context), unityContext);
                while (AdvanceEnumerator(executingEnumerator))
                {
                    yield return executingEnumerator.Current;
                }
            }
            else
            {
                if (context.CurrentResult.ResultState != ResultState.Ignored)
                {
                    context.CurrentResult.SetResult(ResultState.Success);
                }
            }
        }

        private bool AdvanceEnumerator(IEnumerator enumerator)
        {
            using (new ProfilerMarker(testMethod.MethodName).Auto())
                return enumerator.MoveNext();
        }

        private IEnumerator ExecuteEnumerableAndRecordExceptions(IEnumerator enumerator, EnumeratorContext context, UnityTestExecutionContext unityContext)
        {
            while (true)
            {
                if (context.ExceptionWasRecorded)
                {
                    break;
                }

                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (unityContext.TestMode == TestPlatform.PlayMode && enumerator.Current is IEditModeTestYieldInstruction)
                    {
                        throw new Exception($"PlayMode test are not allowed to yield {enumerator.Current.GetType().Name}");
                    }
                }
                catch (Exception ex)
                {
                    context.RecordExceptionWithHint(ex);
                    break;
                }

                if (enumerator.Current is IEnumerator nestedEnumerator)
                {
                    yield return ExecuteEnumerableAndRecordExceptions(nestedEnumerator, context, unityContext);
                }
                else
                {
                    yield return enumerator.Current;
                }
            }
        }

        private class EnumeratorContext
        {
            private readonly ITestExecutionContext m_Context;

            public EnumeratorContext(ITestExecutionContext context)
            {
                m_Context = context;
            }

            public bool ExceptionWasRecorded
            {
                get;
                private set;
            }

            public void RecordExceptionWithHint(Exception ex)
            {
                if (ExceptionWasRecorded)
                {
                    return;
                }
                m_Context.CurrentResult.RecordException(ex);
                ExceptionWasRecorded = true;
            }
        }


        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new NotImplementedException("Use ExecuteEnumerable");
        }
    }
}
