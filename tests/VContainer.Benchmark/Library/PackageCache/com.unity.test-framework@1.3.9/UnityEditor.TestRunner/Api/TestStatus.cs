using System;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// The TestStatus enum indicates the test result status.
    /// </summary>
    public enum TestStatus
    {
        /// <summary>
        /// The test ran with an inconclusive result.
        /// </summary>
        Inconclusive,

        /// <summary>
        /// The test was skipped.
        /// </summary>
        Skipped,

        /// <summary>
        /// The test ran and passed.
        /// </summary>
        Passed,

        /// <summary>
        /// The test ran and failed.
        /// </summary>
        Failed
    }
}
