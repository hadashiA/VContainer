using System;

namespace UnityEditor.TestTools.TestRunner
{

    #if !UNITY_2021_1_OR_NEWER

    internal class XboxOnePlatformSetup : IPlatformSetup
    {
        private XboxOneDeployMethod oldXboxOneDeployMethod;
        private XboxOneDeployDrive oldXboxOneDeployDrive;
        private string oldXboxOneAdditionalDebugPorts;

        public void Setup()
        {
            oldXboxOneDeployMethod = EditorUserBuildSettings.xboxOneDeployMethod;
            oldXboxOneDeployDrive = EditorUserBuildSettings.xboxOneDeployDrive;
            oldXboxOneAdditionalDebugPorts = EditorUserBuildSettings.xboxOneAdditionalDebugPorts;

            EditorUserBuildSettings.xboxOneDeployMethod = XboxOneDeployMethod.Package;
            EditorUserBuildSettings.xboxOneDeployDrive = XboxOneDeployDrive.Default;

            // This causes the XboxOne post processing systems to open this port in your package manifest.
            // In addition it will open the ephemeral range for debug connections as well.
            // Failure to do this will cause connection problems.
            EditorUserBuildSettings.xboxOneAdditionalDebugPorts = "34999";
        }

        public void PostBuildAction()
        {
        }

        public void PostSuccessfulBuildAction()
        {
        }

        public void PostSuccessfulLaunchAction()
        {
        }

        public void CleanUp()
        {
            EditorUserBuildSettings.xboxOneDeployMethod = oldXboxOneDeployMethod;
            EditorUserBuildSettings.xboxOneDeployDrive = oldXboxOneDeployDrive;

            // This causes the XboxOne post processing systems to open this port in your package manifest.
            // In addition it will open the ephemeral range for debug connections as well.
            // Failure to do this will cause connection problems.
            EditorUserBuildSettings.xboxOneAdditionalDebugPorts = oldXboxOneAdditionalDebugPorts;
        }
    }
    #endif
}
