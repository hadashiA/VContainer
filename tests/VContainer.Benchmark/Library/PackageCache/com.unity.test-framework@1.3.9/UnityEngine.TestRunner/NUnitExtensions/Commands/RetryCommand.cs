using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestRunner.TestProtocol;

namespace UnityEngine.TestTools
{
    internal class RetryCommand  : DelegatingTestCommand, IEnumerableTestMethodCommand
    {
        public RetryCommand(TestCommand innerCommand)
            : base(innerCommand)
        {
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new NotImplementedException("Use ExecuteEnumerable");
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            var unityContext = (UnityTestExecutionContext)context;
            if (unityContext.RetryRepeatState?.GetHashCode() == null)
            {
                unityContext.RetryRepeatState = new EnumerableTestState();
            }

            while(unityContext.RetryRepeatState.Retry < unityContext.RetryCount + 1)
            {
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

                if (context.CurrentResult.ResultState != ResultState.Failure || context.CurrentResult.ResultState == ResultState.Error)
                {
                    unityContext.RetryRepeatState.Retry++;
                    break;
                }

                if (unityContext.Automated && unityContext.RetryRepeatState.Retry < unityContext.RetryCount)
                {
                    ReportTestFinishStartPair(unityContext);
                }
                unityContext.RetryRepeatState.Retry++;
            }
            
            SetIterationProperty(unityContext, unityContext.RetryRepeatState.Retry - 1);
            unityContext.RetryRepeatState.Retry = 0;
        }

        private static void ReportTestFinishStartPair(UnityTestExecutionContext unityContext)
        {
            unityContext.CurrentResult.StartTime = unityContext.StartTime;
            unityContext.CurrentResult.EndTime = DateTime.UtcNow;
            long tickCount = Stopwatch.GetTimestamp() - unityContext.StartTicks;
            double seconds = (double) tickCount / Stopwatch.Frequency;
            unityContext.CurrentResult.Duration = seconds;
            SetIterationProperty(unityContext, unityContext.RetryRepeatState.Retry);
            unityContext.Listener.TestFinished(unityContext.CurrentResult);

            // Start new test iteration
            unityContext.CurrentResult = unityContext.CurrentTest.MakeTestResult();
            unityContext.StartTime = DateTime.UtcNow;
            unityContext.StartTicks = Stopwatch.GetTimestamp();
            unityContext.Listener.TestStarted(unityContext.CurrentTest);
        }

        private static void SetIterationProperty(UnityTestExecutionContext unityContext, int iteration)
        {
            unityContext.CurrentResult.Test.Properties.Set("retryIteration", iteration);
        }
    }
}
