using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal class TestListTreeViewDataSource : TreeViewDataSource
    {
        private bool m_ExpandTreeOnCreation;
        private readonly TestListGUI m_TestListGUI;
        private ITestAdaptor m_RootTest;

        public TestListTreeViewDataSource(TreeViewController testListTree, TestListGUI testListGUI, ITestAdaptor rootTest) : base(testListTree)
        {
            showRootItem = false;
            rootIsCollapsable = false;
            m_TestListGUI = testListGUI;
            m_RootTest = rootTest;
        }

        public void UpdateRootTest(ITestAdaptor rootTest)
        {
            m_RootTest = rootTest;
        }

        public override void FetchData()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("InitTestScene"))
                sceneName = PlaymodeTestsController.GetController().settings.originalScene;

            var testListBuilder = new TestTreeViewBuilder(m_RootTest, m_TestListGUI.ResultsByKey, m_TestListGUI.m_TestRunnerUIFilter);

            m_RootItem = testListBuilder.BuildTreeView(null, false, sceneName);
            SetExpanded(m_RootItem, true);
            if (m_RootItem.hasChildren && m_RootItem.children.Count == 1)
                SetExpanded(m_RootItem.children[0], true);

            if (m_ExpandTreeOnCreation)
                SetExpandedWithChildren(m_RootItem, true);

            m_TestListGUI.newResultList = new List<TestRunnerResult>(testListBuilder.results);
            m_TestListGUI.m_TestRunnerUIFilter.availableCategories = testListBuilder.AvailableCategories;
            m_NeedRefreshRows = true;
        }

        public override bool IsRenamingItemAllowed(TreeViewItem item)
        {
            return false;
        }

        public void ExpandTreeOnCreation()
        {
            m_ExpandTreeOnCreation = true;
        }

        public override bool IsExpandable(TreeViewItem item)
        {
            if (item is TestTreeViewItem)
                return ((TestTreeViewItem)item).IsGroupNode;
            return base.IsExpandable(item);
        }

        protected override List<TreeViewItem> Search(TreeViewItem rootItem, string search)
        {
            var result = new List<TreeViewItem>();

            if (rootItem.hasChildren)
            {
                foreach (var child in rootItem.children)
                {
                    SearchTestTree(child, search, result);
                }
            }
            return result;
        }

        protected void SearchTestTree(TreeViewItem item, string search, IList<TreeViewItem> searchResult)
        {
            var testItem = item as TestTreeViewItem;
            if (!testItem.IsGroupNode)
            {
                if (testItem.FullName.ToLower().Contains(search))
                {
                    searchResult.Add(item);
                }
            }
            else if (item.children != null)
            {
                foreach (var child in item.children)
                    SearchTestTree(child, search, searchResult);
            }
        }
    }
}
