using System;
using System.Collections;
using System.Linq;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class LegacyPlayModeRunTask : TestTaskBase
    {
        public LegacyPlayModeRunTask()
        {
            SupportsResumingEnumerator = true;
        }
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var settings = PlaymodeTestsControllerSettings.CreateRunnerSettings(testJobData.executionSettings.filters.Select(filter => filter.ToRuntimeTestRunnerFilter(testJobData.executionSettings.runSynchronously)).ToArray(), testJobData.executionSettings.orderedTestNames, testJobData.executionSettings.randomOrderSeed, testJobData.executionSettings.featureFlags, testJobData.executionSettings.retryCount, testJobData.executionSettings.repeatCount, IsAutomated());
            var launcher = new PlaymodeLauncher(settings);

            launcher.Run();

            while (!PlaymodeLauncher.HasFinished)
            {
                yield return null;
            }
        }
    }
}
