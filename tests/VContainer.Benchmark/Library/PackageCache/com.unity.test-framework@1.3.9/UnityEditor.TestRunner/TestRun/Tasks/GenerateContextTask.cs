using System;
using System.Collections;
using System.Linq;
using UnityEditor.TestTools.TestRunner.UnityTestProtocol;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class GenerateContextTask : TestTaskBase
    {
        public GenerateContextTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.taskInfoStack.Peek().taskMode == TaskMode.Normal)
            {
                testJobData.setUpTearDownState = new BeforeAfterTestCommandState();
                testJobData.outerUnityTestActionState = new BeforeAfterTestCommandState();
                testJobData.enumerableTestState = new EnumerableTestState();
            }

            testJobData.Context = new UnityTestExecutionContext()
            {
                SetUpTearDownState = testJobData.setUpTearDownState,
                OuterUnityTestActionState = testJobData.outerUnityTestActionState,
                EnumerableTestState = testJobData.enumerableTestState,
                Automated = UnityTestProtocolStarter.IsEnabled(),
                RetryCount = testJobData.executionSettings.retryCount,
                RepeatCount = testJobData.executionSettings.repeatCount,
                RetryRepeatState = testJobData.RetryRepeatState
            };

            if (testJobData.executionSettings.ignoreTests != null)
            {
                testJobData.Context.IgnoreTests = testJobData.executionSettings.ignoreTests.Select(ignoreTest => ignoreTest.ParseToEngine()).ToArray();
            }

            testJobData.Context.FeatureFlags = testJobData.executionSettings.featureFlags;

            UnityTestExecutionContext.CurrentContext = testJobData.Context;

            yield break;
        }
    }
}
