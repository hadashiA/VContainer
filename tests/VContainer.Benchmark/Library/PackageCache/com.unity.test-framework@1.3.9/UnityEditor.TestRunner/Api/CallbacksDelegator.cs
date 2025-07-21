using System;
using System.Linq;
using System.Text;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestRunner.TestLaunchers;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal class CallbacksDelegator : ICallbacksDelegator
    {
        private static CallbacksDelegator s_instance;
        public static CallbacksDelegator instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new CallbacksDelegator(CallbacksHolder.instance.GetAll, new TestAdaptorFactory());
                }
                return s_instance;
            }
        }

        private readonly Func<ICallbacks[]> m_CallbacksProvider;
        private readonly ITestAdaptorFactory m_AdaptorFactory;

        public CallbacksDelegator(Func<ICallbacks[]> callbacksProvider, ITestAdaptorFactory adaptorFactory)
        {
            m_CallbacksProvider = callbacksProvider;
            m_AdaptorFactory = adaptorFactory;
        }

        public void RunStarted(ITest testsToRun)
        {
            m_AdaptorFactory.ClearResultsCache();
            var testRunnerTestsToRun = m_AdaptorFactory.Create(testsToRun);
            TryInvokeAllCallbacks(callbacks => callbacks.RunStarted(testRunnerTestsToRun));
        }

        public void RunStartedRemotely(byte[] testsToRunData)
        {
            var testData = Deserialize<RemoteTestResultDataWithTestData>(testsToRunData);
            var testsToRun = m_AdaptorFactory.BuildTree(testData);
            TryInvokeAllCallbacks(callbacks => callbacks.RunStarted(testsToRun));
        }

        public void RunFinished(ITestResult testResults)
        {
            var testResult = m_AdaptorFactory.Create(testResults);
            TryInvokeAllCallbacks(callbacks => callbacks.RunFinished(testResult));
        }

        public void RunFinishedRemotely(byte[] testResultsData)
        {
            var remoteTestResult = Deserialize<RemoteTestResultDataWithTestData>(testResultsData);
            var testResult = m_AdaptorFactory.Create(remoteTestResult.results.First(), remoteTestResult);
            TryInvokeAllCallbacks(callbacks => callbacks.RunFinished(testResult));
        }

        public void RunFailed(string failureMessage)
        {
            Debug.LogError(failureMessage);
            TryInvokeAllCallbacks(callbacks =>
            {
                var errorCallback = callbacks as IErrorCallbacks;
                if (errorCallback != null)
                {
                    errorCallback.OnError(failureMessage);
                }
            });
        }

        public void TestStarted(ITest test)
        {
            var testRunnerTest = m_AdaptorFactory.Create(test);
            TryInvokeAllCallbacks(callbacks => callbacks.TestStarted(testRunnerTest));
        }

        public void TestStartedRemotely(byte[] testStartedData)
        {
            var testData = Deserialize<RemoteTestResultDataWithTestData>(testStartedData);
            var testsToRun = m_AdaptorFactory.BuildTree(testData);

            TryInvokeAllCallbacks(callbacks => callbacks.TestStarted(testsToRun));
        }

        public void TestFinished(ITestResult result)
        {
            var testResult = m_AdaptorFactory.Create(result);
            TryInvokeAllCallbacks(callbacks => callbacks.TestFinished(testResult));
        }

        public void TestFinishedRemotely(byte[] testResultsData)
        {
            var remoteTestResult = Deserialize<RemoteTestResultDataWithTestData>(testResultsData);
            var testResult = m_AdaptorFactory.Create(remoteTestResult.results.First(), remoteTestResult);
            TryInvokeAllCallbacks(callbacks => callbacks.TestFinished(testResult));
        }

        public void TestTreeRebuild(ITest test)
        {
            m_AdaptorFactory.ClearTestsCache();
            var testAdaptor = m_AdaptorFactory.Create(test);
            TryInvokeAllCallbacks(callbacks =>
            {
                var rebuildCallbacks = callbacks as ITestTreeRebuildCallbacks;
                if (rebuildCallbacks != null)
                {
                    rebuildCallbacks.TestTreeRebuild(testAdaptor);
                }
            });
        }

        private void TryInvokeAllCallbacks(Action<ICallbacks> callbackAction)
        {
            foreach (var testRunnerApiCallback in m_CallbacksProvider())
            {
                try
                {
                    callbackAction(testRunnerApiCallback);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static T Deserialize<T>(byte[] data)
        {
            return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(data));
        }
    }
}
