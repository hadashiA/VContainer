using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    // This class is used for serialization purposes
    // which requires public access to fields and a default empty constructor
    [Serializable]
    internal class BuildSettings
    {
        public BuildSettings(string platform, string buildTarget, bool developmentPlayer, string androidBuildSystem = "")
        {
            Platform = platform;
            BuildTarget = buildTarget;
            DevelopmentPlayer = developmentPlayer;
            AndroidBuildSystem = androidBuildSystem;
        }

        public string Platform;
        public string BuildTarget;
        public bool DevelopmentPlayer;
        public string AndroidBuildSystem;
    }
}
