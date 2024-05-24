#if !UNITY_2023_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.TestRun;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.Api.Analytics
{
    internal static class AnalyticsReporter
    {
        private const string VendorKey = "unity.test-framework";
        private const string RunFinishedEventName = "runFinished";
        private const string AnalyzeTestTreeName = "analyzeTestTree";

        private static bool isSetUp;
        private static IDictionary<string, bool> methodsAnalyzed;
        private static IDictionary<string, bool> typesAnalyzed;

        private static void SetUpIfNeeded()
        {
            if (isSetUp)
            {
                return;
            }

            isSetUp = true;
            EditorAnalytics.RegisterEventWithLimit(RunFinishedEventName, 60, 30, VendorKey);
            EditorAnalytics.RegisterEventWithLimit(AnalyzeTestTreeName, 3, 30, VendorKey);
        }

        [InitializeOnLoadMethod]
        private static void RegisterCallbacks()
        {
            ScriptableObject.CreateInstance<TestRunnerApi>().RegisterCallbacks(new AnalyticsTestCallback(ReportRunFinished));
        }

        private static void ReportRunFinished(ITestResultAdaptor testResult)
        {
            SetUpIfNeeded();

            var activeRuns = TestJobDataHolder.instance.TestRuns;
            if (activeRuns.Count == 0)
            {
                return;
            }

            var executionSettings = activeRuns[0].executionSettings;
            var filter = executionSettings.filters.First();
            var runFinishedData = new RunFinishedData
            {
                totalTests = testResult.Test.TestCaseCount,
                numPassedTests = testResult.PassCount,
                numFailedTests = testResult.FailCount,
                numInconclusiveTests = testResult.InconclusiveCount,
                numSkippedTests = testResult.SkipCount,
                testModeFilter = (int)filter.testMode,
                targetPlatform = executionSettings.targetPlatform != null ? executionSettings.targetPlatform.ToString() : "editor",
                runSynchronously = executionSettings.runSynchronously,
                isCustomRunner = false,
                isFiltering = executionSettings.filters.Any(f => f.HasAny()),
                isAutomated = IsCommandLineArgSet("-automated"),
                isFromCommandLine = IsCommandLineArgSet("-runTests"),
                totalTestDuration = testResult.Duration,
                totalRunDuration = (DateTime.Now - Convert.ToDateTime(activeRuns[0].startTime)).TotalSeconds
            };

            EditorAnalytics.SendEventWithLimit(RunFinishedEventName, runFinishedData, 1);
        }

        private static bool IsCommandLineArgSet(string command)
        {
            return Environment.GetCommandLineArgs().Any(c => c == command);
        }

        internal static void AnalyzeTestTreeAndReport(ITest testTree)
        {
            SetUpIfNeeded();

            typesAnalyzed = new Dictionary<string, bool>();
            methodsAnalyzed = new Dictionary<string, bool>();
            var data = new TestTreeData();
            AnalyzeTestTreeNode(testTree, data);
            EditorAnalytics.SendEventWithLimit(AnalyzeTestTreeName, data, 1);
        }

        private static void AnalyzeTestTreeNode(ITest node, TestTreeData data)
        {
            var attributes = GetAttributes(node).ToArray();
            if (attributes.OfType<TestAttribute>().Any())
            {
                data.numTestAttributes++;
            }
            if (attributes.OfType<UnityTestAttribute>().Any())
            {
                data.numUnityTestAttributes++;
            }
            if (attributes.OfType<CategoryAttribute>().Any())
            {
                data.numCategoryAttributes++;
            }
            if (attributes.OfType<TestFixtureAttribute>().Any())
            {
                data.numTestFixtureAttributes++;
            }
            if (attributes.OfType<ConditionalIgnoreAttribute>().Any())
            {
                data.numConditionalIgnoreAttributes++;
            }
            if (attributes.OfType<UnityPlatformAttribute>().Any())
            {
                data.numUnityPlatformAttributes++;
            }

            if (node.HasChildren)
            {
                foreach (var test in node.Tests)
                {
                    AnalyzeTestTreeNode(test, data);
                }
            }
            else
            {
                data.totalNumberOfTests++;
            }
        }

        private static IEnumerable<NUnitAttribute> GetAttributes(ITest node)
        {
            if (node.Method != null)
            {
                var key = $"{node.MethodName},{node.ClassName}";
                if (methodsAnalyzed.ContainsKey(key))
                {
                    yield break;
                }

                methodsAnalyzed[key] = true;
                foreach (var attribute in (node).Method.GetCustomAttributes<NUnitAttribute>(true))
                {
                    yield return attribute;
                }

                var typeKey = node.Method.TypeInfo.FullName;
                if (typesAnalyzed.ContainsKey(typeKey))
                {
                    yield break;
                }

                typesAnalyzed[typeKey] = true;
                foreach (var attribute in node.Method.TypeInfo.GetCustomAttributes<NUnitAttribute>(true))
                {
                    yield return attribute;
                }
            }
        }
    }
}
#endif