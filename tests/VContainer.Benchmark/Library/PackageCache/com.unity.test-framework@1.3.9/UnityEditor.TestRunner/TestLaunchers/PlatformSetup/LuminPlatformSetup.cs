using System;
using System.Threading;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner
{
    internal class LuminPlatformSetup : IPlatformSetup
    {
        private const string kDeviceAddress = "127.0.0.1";
        private const int kDevicePort = 55000;

        public void Setup()
        {
        }

        public void PostBuildAction()
        {
        }

        public void PostSuccessfulBuildAction()
        {
        }

        public void PostSuccessfulLaunchAction()
        {
            var connectionResult = -1;
            var maxTryCount = 100;
            var tryCount = maxTryCount;
            while (tryCount-- > 0 && connectionResult == -1)
            {
                Thread.Sleep(1000);
                connectionResult = EditorConnectionInternal.ConnectPlayerProxy(kDeviceAddress, kDevicePort);
                if (EditorUtility.DisplayCancelableProgressBar("Editor Connection", "Connecting to the player",
                    1 - ((float)tryCount / maxTryCount)))
                {
                    EditorUtility.ClearProgressBar();
                    throw new TestLaunchFailedException();
                }
            }
            EditorUtility.ClearProgressBar();
            if (connectionResult == -1)
                throw new TestLaunchFailedException(
                    "Timed out trying to connect to the player. Player failed to launch or crashed soon after launching");
        }

        public void CleanUp()
        {
        }
    }
}
