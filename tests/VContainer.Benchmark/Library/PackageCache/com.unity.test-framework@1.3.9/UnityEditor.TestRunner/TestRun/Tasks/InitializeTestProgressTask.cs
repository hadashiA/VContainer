using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class InitializeTestProgressTask : TestTaskBase
    {
        public InitializeTestProgressTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.TestStartedEvent.AddListener(test => OnTestStarted(test, testJobData));
            testJobData.TestFinishedEvent.AddListener(test => OnTestFinished(test, testJobData));

            if (testJobData.taskInfoStack.Peek().taskMode == TaskMode.Resume)
            {
                yield break;
            }

            if (testJobData.testTree == null)
            {
                throw new RequiredTestRunDataMissingException(nameof(testJobData.testTree));
            }

            var allTests =
                GetTestsExpectedToRun(testJobData.testTree, testJobData.executionSettings.BuildNUnitFilter());
            testJobData.testProgress = new TestProgress(allTests.ToArray());
            var numTasks = testJobData.Tasks.Count();
            var numTests = testJobData.testProgress.AllTestsToRun.Length;
            var progressAvailableToTests = 1.0f - numTasks * RunProgress.progressPrTask;

            if (numTests > 0)
            {
                testJobData.runProgress.progressPrTest = progressAvailableToTests / numTests;
            }
        }

        private void OnTestStarted(ITest test, TestJobData data)
        {
            if (!test.IsSuite)
            {
                data.runProgress.stepName = test.Name;
            }
        }

        private void OnTestFinished(ITestResult result, TestJobData data)
        {
            if (!result.Test.IsSuite)
            {
                data.runProgress.progress += data.runProgress.progressPrTest;
            }
        }

        private static List<string> GetTestsExpectedToRun(ITest test, ITestFilter filter)
        {
            var expectedTests = new List<string>();

            if (filter.Pass(test))
            {
                if (test.IsSuite)
                {
                    expectedTests.AddRange(test.Tests.SelectMany(subTest => GetTestsExpectedToRun(subTest, filter)));
                }
                else
                {
                    expectedTests.Add(test.FullName);
                }
            }

            return expectedTests;
        }
    }
}
