using System;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    [Serializable]
    internal class FeatureFlags
    {
        public bool fileCleanUpCheck;
        public bool requiresSplashScreen;
        public bool strictDomainReload;
    }
}
