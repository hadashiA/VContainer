using System;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    internal interface ITestJobDataHolder
    {
        void RegisterRun(ITestJobRunner runner, TestJobData data);
        void UnregisterRun(ITestJobRunner runner, TestJobData data);
        ITestJobRunner GetRunner(string guid);
        ITestJobRunner[] GetAllRunners();
    }
}
