using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    internal class RepeatCommand : DelegatingTestCommand, IEnumerableTestMethodCommand
    {
        public RepeatCommand(TestCommand innerCommand)
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

            while(unityContext.RetryRepeatState.Repeat < unityContext.RepeatCount + 1)
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

                if (context.CurrentResult.ResultState != ResultState.Success)
                {
                    unityContext.RetryRepeatState.Repeat++;
                    break;
                }

                if (unityContext.RetryRepeatState.Repeat < unityContext.RepeatCount)
                {
                    ReportTestFinishStartPair(unityContext);
                }
                unityContext.RetryRepeatState.Repeat++;
            }
           
            SetIterationProperty(unityContext, unityContext.RetryRepeatState.Repeat-1);
            unityContext.RetryRepeatState.Repeat = 0;
        }

        private static void ReportTestFinishStartPair(UnityTestExecutionContext unityContext)
        {
            unityContext.CurrentResult.StartTime = unityContext.StartTime;
            unityContext.CurrentResult.EndTime = DateTime.UtcNow;
            long tickCount = Stopwatch.GetTimestamp() - unityContext.StartTicks;
            double seconds = (double) tickCount / Stopwatch.Frequency;
            unityContext.CurrentResult.Duration = seconds;
            SetIterationProperty(unityContext, unityContext.RetryRepeatState.Repeat);
            unityContext.Listener.TestFinished(unityContext.CurrentResult);

            // Start new test iteration
            unityContext.CurrentResult = unityContext.CurrentTest.MakeTestResult();
            unityContext.StartTime = DateTime.UtcNow;
            unityContext.StartTicks = Stopwatch.GetTimestamp();
            unityContext.Listener.TestStarted(unityContext.CurrentTest);
        }

        private static void SetIterationProperty(UnityTestExecutionContext unityContext, int iteration)
        {
            unityContext.CurrentResult.Test.Properties.Set("repeatIteration", iteration);
        }
    }
}
