// ****************************************************************
// Based on nUnit 2.6.2 (http://www.nunit.org/)
// ****************************************************************

using System;
using System.Collections.Generic;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    /// <summary>
    /// Summary description for ResultSummarizer.
    /// </summary>
    internal class ResultSummarizer
    {
        private int m_ErrorCount = -1;
        private int m_FailureCount;
        private int m_IgnoreCount = -1;
        private int m_InconclusiveCount = -1;
        private int m_NotRunnable = -1;
        private int m_ResultCount;
        private int m_SkipCount;
        private int m_SuccessCount;
        private int m_TestsRun;

        private TimeSpan m_Duration = TimeSpan.FromSeconds(0);

        public ResultSummarizer(IEnumerable<TestRunnerResult> results)
        {
            foreach (var result in results)
                Summarize(result);
        }

        public bool success
        {
            get { return m_FailureCount == 0; }
        }

        /// <summary>
        /// Returns the number of test cases for which results
        /// have been summarized. Any tests excluded by use of
        /// Category or Explicit attributes are not counted.
        /// </summary>
        public int ResultCount
        {
            get { return m_ResultCount; }
        }

        /// <summary>
        /// Returns the number of test cases actually run, which
        /// is the same as ResultCount, less any Skipped, Ignored
        /// or NonRunnable tests.
        /// </summary>
        public int TestsRun
        {
            get { return m_TestsRun; }
        }

        /// <summary>
        /// Returns the number of tests that passed
        /// </summary>
        public int Passed
        {
            get { return m_SuccessCount; }
        }

        /// <summary>
        /// Returns the number of test cases that had an error.
        /// </summary>
        public int errors
        {
            get { return m_ErrorCount; }
        }

        /// <summary>
        /// Returns the number of test cases that failed.
        /// </summary>
        public int failures
        {
            get { return m_FailureCount; }
        }

        /// <summary>
        /// Returns the number of test cases that failed.
        /// </summary>
        public int inconclusive
        {
            get { return m_InconclusiveCount; }
        }

        /// <summary>
        /// Returns the number of test cases that were not runnable
        /// due to errors in the signature of the class or method.
        /// Such tests are also counted as Errors.
        /// </summary>
        public int notRunnable
        {
            get { return m_NotRunnable; }
        }

        /// <summary>
        /// Returns the number of test cases that were skipped.
        /// </summary>
        public int Skipped
        {
            get { return m_SkipCount; }
        }

        public int ignored
        {
            get { return m_IgnoreCount; }
        }

        public double duration
        {
            get { return m_Duration.TotalSeconds; }
        }

        public int testsNotRun
        {
            get { return m_SkipCount + m_IgnoreCount + m_NotRunnable; }
        }

        public void Summarize(TestRunnerResult result)
        {
            m_Duration += TimeSpan.FromSeconds(result.duration);
            m_ResultCount++;

            if (result.resultStatus != TestRunnerResult.ResultStatus.NotRun)
            {
                //TODO implement missing features
                //                if(result.IsIgnored)
                //                {
                //                    m_IgnoreCount++;
                //                    return;
                //                }

                m_SkipCount++;
                return;
            }

            switch (result.resultStatus)
            {
                case TestRunnerResult.ResultStatus.Passed:
                    m_SuccessCount++;
                    m_TestsRun++;
                    break;
                case TestRunnerResult.ResultStatus.Failed:
                    m_FailureCount++;
                    m_TestsRun++;
                    break;
                //TODO implement missing features
                //                case TestResultState.Error:
                //                case TestResultState.Cancelled:
                //                    m_ErrorCount++;
                //                    m_TestsRun++;
                //                    break;
                //                case TestResultState.Inconclusive:
                //                    m_InconclusiveCount++;
                //                    m_TestsRun++;
                //                    break;
                //                case TestResultState.NotRunnable:
                //                    m_NotRunnable++;
                //                    // errorCount++;
                //                    break;
                //                case TestResultState.Ignored:
                //                    m_IgnoreCount++;
                //                    break;
                default:
                    m_SkipCount++;
                    break;
            }
        }
    }
}
