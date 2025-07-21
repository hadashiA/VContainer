using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestRunner.TestLaunchers
{
    internal class RemoteTestResultDataFactory : IRemoteTestResultDataFactory
    {
        public RemoteTestResultDataWithTestData CreateFromTestResult(ITestResult result)
        {
            var tests = CreateTestDataList(result.Test);
            tests.First().testCaseTimeout = UnityTestExecutionContext.CurrentContext.TestCaseTimeout;
            return new RemoteTestResultDataWithTestData
            {
                results = CreateTestResultDataList(result),
                tests = tests
            };
        }

        public RemoteTestResultDataWithTestData CreateFromTest(ITest test)
        {
            var tests = CreateTestDataList(test);
            if (UnityTestExecutionContext.CurrentContext != null)
            {
                tests.First().testCaseTimeout = UnityTestExecutionContext.CurrentContext.TestCaseTimeout;
            }

            return new RemoteTestResultDataWithTestData
            {
                tests = tests
            };
        }

        private RemoteTestData[] CreateTestDataList(ITest test)
        {
            var list = new List<RemoteTestData>();
            list.Add(new RemoteTestData(test));
            list.AddRange(test.Tests.SelectMany(CreateTestDataList));
            return list.ToArray();
        }

        private static RemoteTestResultData[] CreateTestResultDataList(ITestResult result)
        {
            var list = new List<RemoteTestResultData>();
            list.Add(new RemoteTestResultData(result, result.Test.Parent == null));
            list.AddRange(result.Children.SelectMany(CreateTestResultDataList));
            return list.ToArray();
        }
    }
}
