using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestRunner.NUnitExtensions.Filters;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal class TestTreeViewBuilder
    {
        public List<TestRunnerResult> results = new List<TestRunnerResult>();
        private readonly Dictionary<string, TestRunnerResult> m_OldTestResults;
        private readonly TestRunnerUIFilter m_UIFilter;
        private readonly ITestAdaptor m_TestListRoot;

        private readonly List<string> m_AvailableCategories = new List<string>();

        public string[] AvailableCategories
        {
            get { return m_AvailableCategories.Distinct().OrderBy(a => a).ToArray(); }
        }

        public TestTreeViewBuilder(ITestAdaptor tests, Dictionary<string, TestRunnerResult> oldTestResultResults, TestRunnerUIFilter uiFilter)
        {
            m_AvailableCategories.Add(CategoryFilterExtended.k_DefaultCategory);
            m_OldTestResults = oldTestResultResults;
            m_TestListRoot = tests;
            m_UIFilter = uiFilter;
        }

        public TreeViewItem BuildTreeView(TestFilterSettings settings, bool sceneBased, string sceneName)
        {
            var rootItem = new TreeViewItem(int.MaxValue, 0, null, "Invisible Root Item");
            ParseTestTree(0, rootItem, m_TestListRoot);
            return rootItem;
        }

        private bool IsFilteredOutByUIFilter(ITestAdaptor test, TestRunnerResult result)
        {
            if (m_UIFilter.PassedHidden && result.resultStatus == TestRunnerResult.ResultStatus.Passed)
                return true;
            if (m_UIFilter.FailedHidden && (result.resultStatus == TestRunnerResult.ResultStatus.Failed || result.resultStatus == TestRunnerResult.ResultStatus.Inconclusive))
                return true;
            if (m_UIFilter.NotRunHidden && (result.resultStatus == TestRunnerResult.ResultStatus.NotRun || result.resultStatus == TestRunnerResult.ResultStatus.Skipped))
                return true;
            if (m_UIFilter.CategoryFilter.Length > 0)
                return !test.Categories.Any(category => m_UIFilter.CategoryFilter.Contains(category));
            return false;
        }

        private void ParseTestTree(int depth, TreeViewItem rootItem, ITestAdaptor testElement)
        {
            m_AvailableCategories.AddRange(testElement.Categories);

            var testElementId = testElement.UniqueName;
            if (!testElement.HasChildren)
            {
                m_OldTestResults.TryGetValue(testElementId, out var result);

                if (result != null &&
                    (result.ignoredOrSkipped
                     || result.notRunnable
                     || testElement.RunState == RunState.NotRunnable
                     || testElement.RunState == RunState.Ignored
                     || testElement.RunState == RunState.Skipped
                    )
                )
                {
                    //if the test was or becomes ignored or not runnable, we recreate the result in case it has changed
                    result = null;
                }
                if (result == null)
                {
                    result = new TestRunnerResult(testElement);
                }
                results.Add(result);

                var test = new TestTreeViewItem(testElement, depth, rootItem);
                if (!IsFilteredOutByUIFilter(testElement, result))
                    rootItem.AddChild(test);
                test.SetResult(result);
                return;
            }

            m_OldTestResults.TryGetValue(testElementId, out var groupResult);
            if (groupResult == null)
            {
                groupResult = new TestRunnerResult(testElement);
            }

            results.Add(groupResult);
            var group = new TestTreeViewItem(testElement, depth, rootItem);
            group.SetResult(groupResult);

            depth++;
            foreach (var child in testElement.Children)
            {
                ParseTestTree(depth, group, child);
            }


            if (testElement.IsTestAssembly && !testElement.HasChildren)
                return;

            if (group.hasChildren)
                rootItem.AddChild(group);
        }
    }
}
