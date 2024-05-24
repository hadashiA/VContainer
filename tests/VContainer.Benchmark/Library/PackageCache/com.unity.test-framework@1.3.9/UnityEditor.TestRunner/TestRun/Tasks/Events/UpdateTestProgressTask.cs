using System;
using System.Collections;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Events
{
    internal class UpdateTestProgressTask : TestTaskBase
    {
        private TestProgress m_TestProgress;

        public UpdateTestProgressTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.testProgress == null)
            {
                throw new RequiredTestRunDataMissingException(nameof(testJobData.testProgress));
            }

            if (testJobData.TestStartedEvent == null)
            {
                throw new RequiredTestRunDataMissingException(nameof(testJobData.TestStartedEvent));
            }

            if (testJobData.TestFinishedEvent == null)
            {
                throw new RequiredTestRunDataMissingException(nameof(testJobData.TestFinishedEvent));
            }

            m_TestProgress = testJobData.testProgress;
            testJobData.TestStartedEvent.AddListener(TestStarted);
            testJobData.TestFinishedEvent.AddListener(TestFinished);

            yield break;
        }

        private void TestStarted(ITest test)
        {
            if (test.IsSuite || !(test is TestMethod))
            {
                return;
            }

            m_TestProgress.CurrentTest = test.Name;
        }

        private void TestFinished(ITestResult testResult)
        {
            if (testResult.Test.IsSuite)
            {
                return;
            }
            var name = testResult.Test.FullName;
            m_TestProgress.RemainingTests.Remove(name);
            m_TestProgress.CompletedTests.Add(name);
        }
    }
}
