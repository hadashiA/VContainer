namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal enum TestRunState
    {
        /// <summary>
        /// When the test run has started, but no callbacks from the test runner api have yet been received.
        /// </summary>
        NoCallbacksReceived,
        /// <summary>
        /// When at least one test started event have been fired for a test node.
        /// </summary>
        OneOrMoreTestsExecutedWithNoFailures,
        /// <summary>
        /// When at least one test finished event have been fired for a test node and the status is failed.
        /// </summary>
        OneOrMoreTestsExecutedWithOneOrMoreFailed,
        /// <summary>
        /// When the test job in the test runner api have completed, but no test started events for test nodes has happened. E.g. if there are no valid tests in the project.
        /// </summary>
        CompletedJobWithoutAnyTestsExecuted,
        /// <summary>
        /// When the test runner api has raised an error during the run.
        /// </summary>
        RunError
    }
}
