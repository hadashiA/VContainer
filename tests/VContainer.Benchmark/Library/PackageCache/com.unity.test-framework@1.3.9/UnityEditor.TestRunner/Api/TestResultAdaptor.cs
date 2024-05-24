using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.TestLaunchers;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal class TestResultAdaptor : ITestResultAdaptor
    {
        private TNode m_Node;
        private ITestResult m_Result;

        internal TestResultAdaptor(ITestResult result, ITestAdaptor test, ITestResultAdaptor[] children = null)
        {
            Test = test;
            Name = result.Name;
            FullName = result.FullName;
            ResultState = result.ResultState.ToString();
            TestStatus = ParseTestStatus(result.ResultState.Status);
            Duration = result.Duration;
            StartTime = result.StartTime;
            EndTime = result.EndTime;
            Message = result.Message;
            StackTrace = result.StackTrace;
            AssertCount = result.AssertCount;
            FailCount = result.FailCount;
            PassCount = result.PassCount;
            SkipCount = result.SkipCount;
            InconclusiveCount = result.InconclusiveCount;
            HasChildren = result.HasChildren;
            Output = result.Output;
            Children = children;
            m_Result = result;
            RetryIteration = result.Test.GetRetryIteration();
            RepeatIteration = result.Test.GetRepeatIteration();
        }

        internal TestResultAdaptor(RemoteTestResultData result, RemoteTestResultDataWithTestData allData)
        {
            Test = new TestAdaptor(allData.tests.First(t => t.id == result.testId));
            Name = result.name;
            FullName = result.fullName;
            ResultState = result.resultState;
            TestStatus = ParseTestStatus(result.testStatus);
            Duration = result.duration;
            StartTime = result.startTime;
            EndTime = result.endTime;
            Message = result.message;
            StackTrace = result.stackTrace;
            AssertCount = result.assertCount;
            FailCount = result.failCount;
            PassCount = result.passCount;
            SkipCount = result.skipCount;
            InconclusiveCount = result.inconclusiveCount;
            HasChildren = result.hasChildren;
            Output = result.output;
            Children = result.childrenIds.Select(childId =>
                new TestResultAdaptor(allData.results.First(r => r.testId == childId), allData)).ToArray();
            if (!string.IsNullOrEmpty(result.xml))
            {
                m_Node = TNode.FromXml(result.xml);
            }
        }

        public ITestAdaptor Test { get; private set; }
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public string ResultState { get; private set; }
        public TestStatus TestStatus { get; private set; }
        public double Duration { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
        public int AssertCount { get; private set; }
        public int FailCount { get; private set; }
        public int PassCount { get; private set; }
        public int SkipCount { get; private set; }
        public int InconclusiveCount { get; private set; }
        public bool HasChildren { get; private set; }
        public IEnumerable<ITestResultAdaptor> Children { get; private set; }
        public string Output { get; private set; }

        public TNode ToXml()
        {
            if (m_Node == null)
            {
                m_Node = m_Result.ToXml(true);
            }

            return m_Node;
        }

        internal int RetryIteration { get; set; }
        internal int RepeatIteration { get; set; }

        private static TestStatus ParseTestStatus(NUnit.Framework.Interfaces.TestStatus testStatus)
        {
            return (TestStatus)Enum.Parse(typeof(TestStatus), testStatus.ToString());
        }
    }
}
