using System;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Filters;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.TestRunner.GUI;

namespace UnityEngine.TestTools.TestRunner
{
    [Serializable]
    internal class PlaymodeTestsControllerSettings
    {
        [SerializeField]
        public RuntimeTestRunnerFilter[] filters;
        public bool sceneBased;
        public string originalScene;
        public string bootstrapScene;
        public bool runInBackgroundValue;
        public bool consoleErrorPaused;
        public string[] orderedTestNames;
        public FeatureFlags featureFlags;
        [SerializeField]
        public int retryCount;

        [SerializeField]
        public int repeatCount;

        [SerializeField]
        public bool automated;

        [SerializeField]
        public int randomOrderSeed;

        public static PlaymodeTestsControllerSettings CreateRunnerSettings(RuntimeTestRunnerFilter[] filters, string[] orderedTestNames, int randomSeed, FeatureFlags featureFlags, int retryCount, int repeatCount, bool automated)
        {
            var settings = new PlaymodeTestsControllerSettings
            {
                filters = filters,
                sceneBased = false,
                originalScene = SceneManager.GetActiveScene().path,
                bootstrapScene = null,
                orderedTestNames = orderedTestNames,
                randomOrderSeed = randomSeed,
                featureFlags = featureFlags,
                retryCount = retryCount,
                repeatCount = repeatCount,
                automated = automated
            };
            return settings;
        }

        internal ITestFilter BuildNUnitFilter()
        {
            return new OrFilter(filters.Select(f => f.BuildNUnitFilter()).ToArray());
        }
    }
}
