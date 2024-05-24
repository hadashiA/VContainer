using System;
using System.Collections;
using System.Linq;
using UnityEditor.TestRunner.TestLaunchers;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class LegacyPlayerRunTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var executionSettings = testJobData.executionSettings;
            var settings = PlaymodeTestsControllerSettings.CreateRunnerSettings(executionSettings.filters.Select(filter => filter.ToRuntimeTestRunnerFilter(executionSettings.runSynchronously)).ToArray(), testJobData.executionSettings.orderedTestNames, testJobData.executionSettings.randomOrderSeed, testJobData.executionSettings.featureFlags, executionSettings.retryCount, executionSettings.repeatCount, IsAutomated());
            var launcher = new PlayerLauncher(settings, executionSettings.targetPlatform, executionSettings.overloadTestRunSettings, executionSettings.playerHeartbeatTimeout, executionSettings.playerSavePath);
            launcher.Run();

            while (RemoteTestRunController.instance.isRunning)
            {
                yield return null;
            }
        }
    }
}
