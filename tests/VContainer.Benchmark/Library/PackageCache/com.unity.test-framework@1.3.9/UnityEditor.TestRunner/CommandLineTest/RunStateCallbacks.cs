using System;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal class RunStateCallbacks : IErrorCallbacks
    {
        internal IRunData runData = RunData.instance;
        internal static bool preventExit;

        public void RunFinished(ITestResultAdaptor testResults)
        {
            if (preventExit)
            {
                return;
            }

            if (runData.RunState == TestRunState.NoCallbacksReceived)
            {
                runData.RunState = TestRunState.CompletedJobWithoutAnyTestsExecuted;
            }
        }

        public void TestStarted(ITestAdaptor test)
        {
            if (!test.IsSuite && runData.RunState == TestRunState.NoCallbacksReceived)
            {
                runData.RunState = TestRunState.OneOrMoreTestsExecutedWithNoFailures;
            }
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (!result.Test.IsSuite && (result.TestStatus == TestStatus.Failed || result.TestStatus == TestStatus.Inconclusive))
            {
                runData.RunState = TestRunState.OneOrMoreTestsExecutedWithOneOrMoreFailed;
            }
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void OnError(string message)
        {
            runData.RunState = TestRunState.RunError;
            runData.RunErrorMessage = message;
        }
    }
}
