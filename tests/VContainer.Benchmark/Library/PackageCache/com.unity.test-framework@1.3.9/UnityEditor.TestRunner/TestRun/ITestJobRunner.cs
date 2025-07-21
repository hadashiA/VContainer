using System;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    internal interface ITestJobRunner
    {
        string RunJob(TestJobData data);
        bool CancelRun();
        bool IsRunningJob();
        TestJobData GetData();
    }
}
