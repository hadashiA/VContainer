using System;
using NUnit.Framework.Interfaces;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal interface ICallbacksDelegator
    {
        void RunStarted(ITest testsToRun);
        void RunStartedRemotely(byte[] testsToRunData);
        void RunFinished(ITestResult testResults);
        void RunFinishedRemotely(byte[] testResultsData);
        void RunFailed(string failureMessage);
        void TestStarted(ITest test);
        void TestStartedRemotely(byte[] testStartedData);
        void TestFinished(ITestResult result);
        void TestFinishedRemotely(byte[] testResultsData);
        void TestTreeRebuild(ITest test);
    }
}
