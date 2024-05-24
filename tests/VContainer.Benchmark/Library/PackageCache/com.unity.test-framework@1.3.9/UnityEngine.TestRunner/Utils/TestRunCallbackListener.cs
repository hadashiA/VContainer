using System;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestRunner.Utils
{
    internal class TestRunCallbackListener : ScriptableObject, ITestRunnerListener
    {
        private ITestRunCallback[] m_Callbacks;
        public void RunStarted(ITest testsToRun)
        {
            InvokeAllCallbacks(callback => callback.RunStarted(testsToRun));
        }

        private static ITestRunCallback[] GetAllCallbacks()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            allAssemblies = allAssemblies.Where(x => x.GetReferencedAssemblies().Any(z => z.Name == "UnityEngine.TestRunner")).ToArray();
            var attributes = allAssemblies.SelectMany(assembly => assembly.GetCustomAttributes(typeof(TestRunCallbackAttribute), true).OfType<TestRunCallbackAttribute>()).ToArray();
            return attributes.Select(attribute => attribute.ConstructCallback()).ToArray();
        }

        private void InvokeAllCallbacks(Action<ITestRunCallback> invoker)
        {
            if (m_Callbacks == null)
            {
                m_Callbacks = GetAllCallbacks();
            }

            foreach (var testRunCallback in m_Callbacks)
            {
                try
                {
                    invoker(testRunCallback);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
        }

        public void RunFinished(ITestResult testResults)
        {
            InvokeAllCallbacks(callback => callback.RunFinished(testResults));
        }

        public void TestStarted(ITest test)
        {
            InvokeAllCallbacks(callback => callback.TestStarted(test));
        }

        public void TestFinished(ITestResult result)
        {
            InvokeAllCallbacks(callback => callback.TestFinished(result));
        }
    }
}
