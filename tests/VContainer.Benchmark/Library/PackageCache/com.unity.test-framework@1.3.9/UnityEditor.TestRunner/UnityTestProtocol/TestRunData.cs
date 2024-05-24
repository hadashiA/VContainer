using System;

namespace UnityEditor.TestRunner.UnityTestProtocol
{
    [Serializable]
    public class TestRunData
    {
        public string SuiteName;
        public string[] TestsInFixture;
        public long OneTimeSetUpDuration;
        public long OneTimeTearDownDuration;
    }
}