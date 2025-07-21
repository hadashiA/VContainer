using System;

namespace UnityEditor.TestTools.TestRunner
{
    internal interface IPlatformSetup
    {
        void Setup();
        void PostBuildAction();
        void PostSuccessfulBuildAction();
        void PostSuccessfulLaunchAction();
        void CleanUp();
    }
}
