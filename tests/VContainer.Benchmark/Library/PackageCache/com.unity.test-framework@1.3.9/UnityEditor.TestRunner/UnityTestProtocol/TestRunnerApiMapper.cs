using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.GUI;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class TestRunnerApiMapper : ITestRunnerApiMapper
    {
        internal IGuiHelper guiHelper =  new GuiHelper(new MonoCecilHelper(), new AssetsDatabaseHelper());
        private readonly string _projectRepoPath;

        public TestRunnerApiMapper(string projectRepoPath)
        {
            _projectRepoPath = projectRepoPath;
        }

        public TestPlanMessage MapTestToTestPlanMessage(ITestAdaptor testsToRun)
        {
            var testsNames = testsToRun != null ? FlattenTestNames(testsToRun) : new List<string>();

            var msg = new TestPlanMessage
            {
                tests = testsNames
            };

            return msg;
        }

        public TestStartedMessage MapTestToTestStartedMessage(ITestAdaptor test)
        {
            return new TestStartedMessage
            {
                name = test.FullName
            };
        }

        public TestFinishedMessage TestResultToTestFinishedMessage(ITestResultAdaptor result)
        {
            string filePathString = default;
            int lineNumber = default;
            if (result.Test.Method != null && result.Test.TypeInfo != null)
            {
                var method = result.Test.Method.MethodInfo;
                var type = result.Test.TypeInfo.Type;
                var fileOpenInfo = guiHelper.GetFileOpenInfo(type, method);
                filePathString = !string.IsNullOrEmpty(_projectRepoPath) ? Path.Combine(_projectRepoPath, fileOpenInfo.FilePath) : fileOpenInfo.FilePath;
                lineNumber = fileOpenInfo.LineNumber;
            }

            var iteration = 0;
            if(result is TestResultAdaptor)
            {
                var adaptor = ((TestResultAdaptor)result);
                iteration = adaptor.RepeatIteration == 0 ? adaptor.RetryIteration : adaptor.RepeatIteration;
            }
            return new TestFinishedMessage
            {
                name = result.Test.FullName,
                duration = Convert.ToUInt64(result.Duration * 1000),
                durationMicroseconds = Convert.ToUInt64(result.Duration * 1000000),
                message = result.Message,
                state = GetTestStateFromResult(result),
                stackTrace = result.StackTrace,
                fileName = filePathString,
                lineNumber = lineNumber,
                iteration = iteration
            };
        }

        public string GetRunStateFromResultNunitXml(ITestResultAdaptor result)
        {
            var doc = new XmlDocument();
            doc.LoadXml(result.ToXml().OuterXml);
            return doc.FirstChild.Attributes["runstate"].Value;
        }

        public TestState GetTestStateFromResult(ITestResultAdaptor result)
        {
            var state = TestState.Failure;

            if (result.TestStatus == TestStatus.Passed)
            {
                state = TestState.Success;
            }
            else if (result.TestStatus == TestStatus.Skipped)
            {
                state = TestState.Skipped;

                if (result.ResultState.ToLowerInvariant().EndsWith("ignored"))
                {
                    state = TestState.Ignored;
                }
            }
            else
            {
                if (result.ResultState.ToLowerInvariant().Equals("inconclusive"))
                {
                    state = TestState.Inconclusive;
                }

                if (result.ResultState.ToLowerInvariant().EndsWith("cancelled") ||
                    result.ResultState.ToLowerInvariant().EndsWith("error"))
                {
                    state = TestState.Error;
                }
            }

            return state;
        }

        public List<string> FlattenTestNames(ITestAdaptor test)
        {
            var results = new List<string>();

            if (!test.IsSuite)
                results.Add(test.FullName);

            if (test.Children != null && test.Children.Any())
                foreach (var child in test.Children)
                    results.AddRange(FlattenTestNames(child));

            return results;
        }
    }
}
