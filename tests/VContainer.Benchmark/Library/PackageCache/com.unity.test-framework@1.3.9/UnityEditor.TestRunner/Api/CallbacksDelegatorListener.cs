using System;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal class CallbacksDelegatorListener : ScriptableObject, ITestRunnerListener
    {
        public void RunStarted(ITest testsToRun)
        {
            CallbacksDelegator.instance.RunStarted(testsToRun);
        }

        public void RunFinished(ITestResult testResults)
        {
            CallbacksDelegator.instance.RunFinished(testResults);
        }

        public void TestStarted(ITest test)
        {
            CallbacksDelegator.instance.TestStarted(test);
        }

        public void TestFinished(ITestResult result)
        {
            CallbacksDelegator.instance.TestFinished(result);
        }
    }
}
