using System;

namespace UnityEditor.TestTools.TestRunner.Api.Analytics
{
    internal class RunFinishedData
    {
        public int totalTests;
        public int numPassedTests;
        public int numFailedTests;
        public int numInconclusiveTests;
        public int numSkippedTests;
        public int testModeFilter;
        public bool isAutomated;
        public bool isFromCommandLine;
        public bool isFiltering;
        public string targetPlatform;
        public double totalTestDuration;
        public double totalRunDuration;
        public bool runSynchronously;
        public bool isCustomRunner;
    }
}
