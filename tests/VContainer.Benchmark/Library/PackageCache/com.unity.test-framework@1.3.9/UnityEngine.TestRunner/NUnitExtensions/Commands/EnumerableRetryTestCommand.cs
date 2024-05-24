using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    internal class EnumerableRetryTestCommand : DelegatingTestCommand, IEnumerableTestMethodCommand
    {
        private int retryCount;

        public EnumerableRetryTestCommand(RetryAttribute.RetryCommand commandToReplace) : base(commandToReplace.GetInnerCommand())
        {
            retryCount = (int)typeof(RetryAttribute.RetryCommand)
                .GetField("_retryCount", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(commandToReplace);
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new NotImplementedException("Use ExecuteEnumerable");
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            var unityContext = (UnityTestExecutionContext)context;
            int count = unityContext.EnumerableTestState.Retry;
            var firstCycleAfterResume = count > 0;

            while (count < retryCount || (firstCycleAfterResume && count <= retryCount))
            {
                if (!firstCycleAfterResume)
                {
                    count++;
                }

                firstCycleAfterResume = false;

                unityContext.EnumerableTestState.Retry = count;

                if (innerCommand is IEnumerableTestMethodCommand)
                {
                    var executeEnumerable = ((IEnumerableTestMethodCommand)innerCommand).ExecuteEnumerable(context);
                    foreach (var iterator in executeEnumerable)
                    {
                        yield return iterator;
                    }
                }
                else
                {
                    context.CurrentResult = innerCommand.Execute(context);
                }

                if (context.CurrentResult.ResultState != ResultState.Failure)
                {
                    break;
                }
            }

            unityContext.EnumerableTestState.Retry = 0;
        }
    }
}
