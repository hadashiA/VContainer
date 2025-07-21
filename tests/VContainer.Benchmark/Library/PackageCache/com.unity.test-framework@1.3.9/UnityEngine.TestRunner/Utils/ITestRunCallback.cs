using System;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestRunner
{
    /// <summary>
    /// Interface for getting callsbacks on test progress directly from NUnit. This is available both in the editor and directly in the runtime. It is registered by using <see cref="TestRunCallbackAttribute"/>.
    /// </summary>
    public interface ITestRunCallback
    {
        /// <summary>
        /// A callback invoked when a test run is started.
        /// </summary>
        /// <param name="testsToRun">The full loaded test tree.</param>
        void RunStarted(ITest testsToRun);
        /// <summary>
        /// A callback invoked when a test run is finished.
        /// </summary>
        /// <param name="testResults">The result of the test run.</param>
        void RunFinished(ITestResult testResults);
        /// <summary>
        /// A callback invoked when each individual node of the test tree has started executing.
        /// </summary>
        /// <param name="test">The test node currently executed.</param>
        void TestStarted(ITest test);
        /// <summary>
        /// A callback invoked when each individual node of the test tree has finished executing.
        /// </summary>
        /// <param name="result">The result of the test tree node after it had been executed.</param>
        void TestFinished(ITestResult result);
    }
}
