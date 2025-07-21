using System;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// Callbacks in the <see cref="TestRunnerApi"/> for the test stages when running tests.
    /// </summary>
    public interface ICallbacks
    {
        /// <summary>
        /// A callback invoked when a test run is started.
        /// </summary>
        /// <param name="testsToRun">The full loaded test tree.</param>
        void RunStarted(ITestAdaptor testsToRun);
        /// <summary>
        /// A callback invoked when a test run is finished.
        /// </summary>
        /// <param name="result">The result of the test run.</param>
        void RunFinished(ITestResultAdaptor result);
        /// <summary>
        /// A callback invoked when each individual node of the test tree has started executing.
        /// </summary>
        /// <param name="test">The test node currently executed.</param>
        void TestStarted(ITestAdaptor test);
        /// <summary>
        /// A callback invoked when each individual node of the test tree has finished executing.
        /// </summary>
        /// <param name="result">The result of the test tree node after it had been executed.</param>
        void TestFinished(ITestResultAdaptor result);
    }
}
