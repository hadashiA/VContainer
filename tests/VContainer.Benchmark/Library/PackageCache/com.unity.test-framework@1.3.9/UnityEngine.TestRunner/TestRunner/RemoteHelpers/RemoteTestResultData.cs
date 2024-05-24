using System;
using System.Linq;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestRunner.TestLaunchers
{
    [Serializable]
    internal class RemoteTestResultData
    {
        public string testId;
        public string name;
        public string fullName;
        public string resultState;
        public TestStatus testStatus;
        public double duration;
        public DateTime startTime;
        public DateTime endTime;
        public string message;
        public string stackTrace;
        public int assertCount;
        public int failCount;
        public int passCount;
        public int skipCount;
        public int inconclusiveCount;
        public bool hasChildren;
        public string output;
        public string xml;
        public string[] childrenIds;

        internal RemoteTestResultData(ITestResult result, bool isTopLevel)
        {
            testId = result.Test.Id;
            name = result.Name;
            fullName = result.FullName;
            resultState = result.ResultState.ToString();
            testStatus = result.ResultState.Status;
            duration = result.Duration;
            startTime = result.StartTime;
            endTime = result.EndTime;
            message = result.Message;
            stackTrace = result.StackTrace;
            assertCount = result.AssertCount;
            failCount = result.FailCount;
            passCount = result.PassCount;
            skipCount = result.SkipCount;
            inconclusiveCount = result.InconclusiveCount;
            hasChildren = result.HasChildren;
            output = result.Output;
            if (isTopLevel)
            {
                xml = result.ToXml(true).OuterXml;
            }
            childrenIds = result.Children.Select(child => child.Test.Id).ToArray();
        }
    }
}
