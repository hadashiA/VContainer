using System;
using System.Linq;
using UnityEditor.TestTools.TestRunner.CommandLineTest;
using UnityEditor.TestTools.TestRunner.TestRun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestRunner.TestLaunchers;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// The TestRunnerApi retrieves and runs tests programmatically from code inside the project, or inside other packages. TestRunnerApi is a [ScriptableObject](https://docs.unity3d.com/ScriptReference/ScriptableObject.html).
    /// You can initialize the API like this:
    /// <code>
    /// var testRunnerApi = ScriptableObject.CreateInstance&lt;TestRunnerApi&gt;();
    /// </code>
    /// Note: You can subscribe and receive test results in one instance of the API, even if the run starts from another instance.
    /// The TestRunnerApi supports the following workflows:
    /// - [How to run tests programmatically](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/extension-run-tests.html)
    /// - [How to get test results](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/extension-get-test-results.html)
    /// - [How to retrieve the list of tests](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/extension-retrieve-test-list.html)
    /// </summary>
    public class TestRunnerApi : ScriptableObject, ITestRunnerApi
    {
        internal static ICallbacksHolder callbacksHolder;
        private static ICallbacksHolder CallbacksHolder
        {
            get
            {
                if (callbacksHolder == null)
                {
                    callbacksHolder = Api.CallbacksHolder.instance;
                }

                return callbacksHolder;
            }
        }

        internal static ITestJobDataHolder testJobDataHolder;

        private static ITestJobDataHolder m_testJobDataHolder
        {
            get { return testJobDataHolder ?? (testJobDataHolder = TestJobDataHolder.instance); }
        }

        internal Func<ExecutionSettings,string> ScheduleJob = executionSettings =>
        {
            var runner = new TestJobRunner();
            return runner.RunJob(new TestJobData(executionSettings));
        };
        /// <summary>
        /// Starts a test run with a given set of executionSettings.
        /// </summary>
        /// <param name="executionSettings">Set of <see cref="ExecutionSettings"/></param>
        /// <returns>A GUID that identifies the TestJobData.</returns>
        public string Execute(ExecutionSettings executionSettings)
        {
            if (executionSettings == null)
            {
                throw new ArgumentNullException(nameof(executionSettings));
            }

            if ((executionSettings.filters == null || executionSettings.filters.Length == 0) && executionSettings.filter != null)
            {
                // Map filter (singular) to filters (plural), for backwards compatibility.
                executionSettings.filters = new[] { executionSettings.filter };
            }

            if (executionSettings.targetPlatform == null && executionSettings.filters != null &&
                executionSettings.filters.Length > 0)
            {
                executionSettings.targetPlatform = executionSettings.filters[0].targetPlatform;
            }

            if (executionSettings.featureFlags == null)
            {
                executionSettings.featureFlags = new FeatureFlags();
            }

            return ScheduleJob(executionSettings);
        }

        /// <summary>
        /// Sets up a given instance of <see cref="ICallbacks"/> to be invoked on test runs.
        /// </summary>
        /// <typeparam name="T">
        /// Generic representing a type of callback.
        /// </typeparam>
        /// <param name="testCallbacks">
        /// The test callbacks to be invoked.
        /// </param>
        /// <param name="priority">
        /// Sets the order in which the callbacks are invoked, starting with the highest value first.
        /// </param>
        public void RegisterCallbacks<T>(T testCallbacks, int priority = 0) where T : ICallbacks
        {
            RegisterTestCallback(testCallbacks, priority);
        }

        /// <summary>
        /// Sets up a given instance of <see cref="ICallbacks"/> to be invoked on test runs.
        /// </summary>
        /// <typeparam name="T">
        /// Generic representing a type of callback.
        /// </typeparam>
        /// <param name="testCallbacks">The test callbacks to be invoked</param>
        /// <param name="priority">
        /// Sets the order in which the callbacks are invoked, starting with the highest value first.
        /// </param>
        public static void RegisterTestCallback<T>(T testCallbacks, int priority = 0) where T : ICallbacks
        {
            if (testCallbacks == null)
            {
                throw new ArgumentNullException(nameof(testCallbacks));
            }

            CallbacksHolder.Add(testCallbacks, priority);
        }

        /// <summary>
        /// Unregister an instance of <see cref="ICallbacks"/> to no longer receive callbacks from test runs.
        /// </summary>
        /// <typeparam name="T">
        /// Generic representing a type of callback.
        /// </typeparam>
        /// <param name="testCallbacks">The test callbacks to unregister.</param>
        public void UnregisterCallbacks<T>(T testCallbacks) where T : ICallbacks
        {
            UnregisterTestCallback(testCallbacks);
        }

        /// <summary>
        /// Unregister an instance of <see cref="ICallbacks"/> to no longer receive callbacks from test runs.
        /// </summary>
        /// <typeparam name="T">
        /// Generic representing a type of callback.
        /// </typeparam>
        /// <param name="testCallbacks">The test callbacks to unregister.</param>
        public static void UnregisterTestCallback<T>(T testCallbacks) where T : ICallbacks
        {
            if (testCallbacks == null)
            {
                throw new ArgumentNullException(nameof(testCallbacks));
            }

            CallbacksHolder.Remove(testCallbacks);
        }

        internal void RetrieveTestList(ExecutionSettings executionSettings, Action<ITestAdaptor> callback)
        {
            if (executionSettings == null)
            {
                throw new ArgumentNullException(nameof(executionSettings));
            }

            var firstFilter = executionSettings.filters?.FirstOrDefault() ?? executionSettings.filter;
            RetrieveTestList(firstFilter.testMode, callback);
        }
        /// <summary>
        /// Retrieve the full test tree as ITestAdaptor for a given test mode. This is obsolete. Use TestRunnerApi.RetrieveTestTree instead.
        /// </summary>
        /// <param name="testMode"></param>
        /// <param name="callback"></param>
        public void RetrieveTestList(TestMode testMode, Action<ITestAdaptor> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var platform = ParseTestMode(testMode);
            var testAssemblyProvider = new EditorLoadedTestAssemblyProvider(new EditorCompilationInterfaceProxy(), new EditorAssembliesProxy());
            var testAdaptorFactory = new TestAdaptorFactory();
            var testListCache = new TestListCache(testAdaptorFactory, new RemoteTestResultDataFactory(), TestListCacheData.instance);
            var testListProvider = new TestListProvider(testAssemblyProvider, new UnityTestAssemblyBuilder(null, 0));
            var cachedTestListProvider = new CachingTestListProvider(testListProvider, testListCache, testAdaptorFactory);

            var job = new TestListJob(cachedTestListProvider, platform, testRoot =>
            {
                callback(testRoot);
            });
            job.Start();
        }

        /// <summary>
        /// Cancel the test run with a given guid. The guid can be retrieved when executing the test run. Currently only supports EditMode tests.
        /// </summary>
        /// <param name="guid">Test run GUID.</param>
        /// <returns></returns>
        internal static bool CancelTestRun(string guid)
        {
            var runner = m_testJobDataHolder.GetRunner(guid);
            if (runner == null || !runner.IsRunningJob())
            {
                return false;
            }

            return runner.CancelRun();
        }

        internal static bool IsRunActive()
        {
            return m_testJobDataHolder.GetAllRunners().Any(r => r.GetData().isRunning);
        }

        private static TestPlatform ParseTestMode(TestMode testMode)
        {
            return (((testMode & TestMode.EditMode) == TestMode.EditMode) ? TestPlatform.EditMode : 0) | (((testMode & TestMode.PlayMode) == TestMode.PlayMode) ? TestPlatform.PlayMode : 0);
        }

        internal class RunProgressChangedEvent : UnityEvent<TestRunProgress> {}
        internal static RunProgressChangedEvent runProgressChanged = new RunProgressChangedEvent();

    }
}
