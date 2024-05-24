using UnityEngine.TestRunner;
using Unity.PerformanceTesting;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.Scripting;
using UnityEngine;

[assembly: TestRunCallback(typeof(PlayerCallbacks))]

namespace Unity.PerformanceTesting
{
    [Preserve]
    public class PlayerCallbacks : ITestRunCallback
    {
        internal static bool saved;

        public void RunStarted(ITest testsToRun)
        {
        }

        public void RunFinished(ITestResult testResults)
        {
            saved = false;
        }

        public void TestStarted(ITest test)
        {
        }

        public void TestFinished(ITestResult result)
        {
        }

        internal static void LogMetadata()
        {
            if (saved) return;

            var run = Metadata.GetFromResources();
            var json = JsonUtility.ToJson(run);
            TestContext.Out?.WriteLine("##performancetestruninfo2:" + json);
            saved = true;
        }
    }
}