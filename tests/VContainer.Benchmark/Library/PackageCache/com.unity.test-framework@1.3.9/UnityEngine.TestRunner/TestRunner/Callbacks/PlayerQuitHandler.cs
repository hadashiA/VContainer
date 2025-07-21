using System;
using NUnit.Framework.Interfaces;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.TestRunner.TestLaunchers;

namespace UnityEngine.TestTools.TestRunner.Callbacks
{
    internal class PlayerQuitHandler : MonoBehaviour, ITestRunnerListener
    {
        public void Start()
        {
            PlayerConnection.instance.Register(PlayerConnectionMessageIds.quitPlayerMessageId, ProcessPlayerQuiteMessage);
        }

        private void ProcessPlayerQuiteMessage(MessageEventArgs arg0)
        {
            //Some platforms don't quit, so we need to disconnect to make sure they will not connect to another editor instance automatically.
            PlayerConnection.instance.DisconnectAll();

#if !UNITY_2021_1_OR_NEWER
            //XBOX has an error when quitting
            if (Application.platform == RuntimePlatform.XboxOne)
            {
                return;
            }
#endif
            Application.Quit();
        }

        public void RunStarted(ITest testsToRun)
        {
        }

        public void RunFinished(ITestResult testResults)
        {
        }

        public void TestStarted(ITest test)
        {
        }

        public void TestFinished(ITestResult result)
        {
        }
    }
}
