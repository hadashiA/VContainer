using UnityEditor.TestTools.TestRunner.Api;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class UtpMessageReporter : IUtpMessageReporter
    {
        public ITestRunnerApiMapper TestRunnerApiMapper;
        public IUtpLogger Logger;

        public UtpMessageReporter(IUtpLogger utpLogger, string projectRepoPath)
        {
            TestRunnerApiMapper = new TestRunnerApiMapper(projectRepoPath);
            Logger = utpLogger;
        }

        public void ReportTestRunStarted(ITestAdaptor testsToRun)
        {
            var testPlanMessage = TestRunnerApiMapper.MapTestToTestPlanMessage(testsToRun);
            Logger.Log(testPlanMessage);
            
            Logger.Log(UtpMessageBuilder.BuildScreenSettings());
            Logger.Log(UtpMessageBuilder.BuildPlayerSettings());
            Logger.Log(UtpMessageBuilder.BuildBuildSettings());
            Logger.Log(UtpMessageBuilder.BuildPlayerSystemInfo());
            Logger.Log(UtpMessageBuilder.BuildQualitySettings());
        }

        public void ReportTestStarted(ITestAdaptor test)
        {
            if (test.IsSuite)
                return;

            var msg = TestRunnerApiMapper.MapTestToTestStartedMessage(test);

            Logger.Log(msg);
        }

        public void ReportTestFinished(ITestResultAdaptor result)
        {
            if (result.Test.IsSuite)
                return;

            var msg = TestRunnerApiMapper.TestResultToTestFinishedMessage(result);

            Logger.Log(msg);
        }
    }
}