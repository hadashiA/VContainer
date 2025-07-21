using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal abstract class TestListGUI
    {
        private static readonly GUIContent s_GUIRunSelectedTests = EditorGUIUtility.TrTextContent("Run Selected", "Run selected test(s)");
        private static readonly GUIContent s_GUIRunAllTests = EditorGUIUtility.TrTextContent("Run All", "Run all tests");
        private static readonly GUIContent s_GUIRerunFailedTests = EditorGUIUtility.TrTextContent("Rerun Failed", "Rerun all failed tests");
        private static readonly GUIContent s_GUIRun = EditorGUIUtility.TrTextContent("Run");
        private static readonly GUIContent s_GUIRunUntilFailed = EditorGUIUtility.TrTextContent("Run Until Failed");
        private static readonly GUIContent s_GUIRun100Times = EditorGUIUtility.TrTextContent("Run 100 times");
        private static readonly GUIContent s_GUIOpenTest = EditorGUIUtility.TrTextContent("Open source code");
        private static readonly GUIContent s_GUIOpenErrorLine = EditorGUIUtility.TrTextContent("Open error line");
        private static readonly GUIContent s_GUIClearResults = EditorGUIUtility.TrTextContent("Clear Results", "Clear all test results");

        [SerializeField]
        protected TestRunnerWindow m_Window;

        public List<TestRunnerResult> newResultList
        {
            get { return m_NewResultList; }
            set { 
                m_ResultByKey = null;
                m_NewResultList = value;
            }
        }
        [SerializeField]
        private List<TestRunnerResult> m_NewResultList = new List<TestRunnerResult>();

        private Dictionary<string, TestRunnerResult> m_ResultByKey;
        internal Dictionary<string, TestRunnerResult> ResultsByKey
        {
            get
            {
                if (m_ResultByKey == null)
                    m_ResultByKey = newResultList.ToDictionary(k => k.uniqueId);
                return m_ResultByKey;
            }
        }
        
        [SerializeField]
        private string m_ResultText;
        [SerializeField]
        private string m_ResultStacktrace;

        private TreeViewController m_TestListTree;
        [SerializeField]
        internal TreeViewState m_TestListState;
        [SerializeField]
        internal TestRunnerUIFilter m_TestRunnerUIFilter = new TestRunnerUIFilter();

        private Vector2 m_TestInfoScroll, m_TestListScroll;
        private string m_PreviousProjectPath;
        private List<TestRunnerResult> m_QueuedResults = new List<TestRunnerResult>();

        protected TestListGUI()
        {
            MonoCecilHelper = new MonoCecilHelper();
            AssetsDatabaseHelper = new AssetsDatabaseHelper();

            GuiHelper = new GuiHelper(MonoCecilHelper, AssetsDatabaseHelper);
        }

        protected IMonoCecilHelper MonoCecilHelper { get; private set; }
        protected IAssetsDatabaseHelper AssetsDatabaseHelper { get; private set; }
        protected IGuiHelper GuiHelper { get; private set; }
        protected UITestRunnerFilter[] SelectedTestsFilter => GetSelectedTestsAsFilter(m_TestListTree.GetSelection());

        public abstract TestMode TestMode { get; }

        public virtual void PrintHeadPanel()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            using (new EditorGUI.DisabledScope(IsBusy()))
            {
                if (GUILayout.Button(s_GUIRunAllTests, EditorStyles.toolbarButton))
                {
                    var filter = new UITestRunnerFilter {categoryNames = m_TestRunnerUIFilter.CategoryFilter};
                    RunTests(filter);
                    GUIUtility.ExitGUI();
                }
            }
            using (new EditorGUI.DisabledScope(m_TestListTree == null || !m_TestListTree.HasSelection() || IsBusy()))
            {
                if (GUILayout.Button(s_GUIRunSelectedTests, EditorStyles.toolbarButton))
                {
                    RunTests(SelectedTestsFilter);
                    GUIUtility.ExitGUI();
                }
            }
            using (new EditorGUI.DisabledScope(m_TestRunnerUIFilter.FailedCount == 0 || IsBusy()))
            {
                if (GUILayout.Button(s_GUIRerunFailedTests, EditorStyles.toolbarButton))
                {
                    var failedTestnames = new List<string>();
                    foreach (var result in newResultList)
                    {
                        if (result.isSuite)
                            continue;
                        if (result.resultStatus == TestRunnerResult.ResultStatus.Failed ||
                            result.resultStatus == TestRunnerResult.ResultStatus.Inconclusive)
                            failedTestnames.Add(result.fullName);
                    }
                    RunTests(new UITestRunnerFilter {testNames = failedTestnames.ToArray(), categoryNames = m_TestRunnerUIFilter.CategoryFilter});
                    GUIUtility.ExitGUI();
                }
            }
            using (new EditorGUI.DisabledScope(IsBusy()))
            {
                if (GUILayout.Button(s_GUIClearResults, EditorStyles.toolbarButton))
                {
                    foreach (var result in newResultList)
                    {
                        result.Clear();
                    }
                    m_TestRunnerUIFilter.UpdateCounters(newResultList);
                    Reload();
                    GUIUtility.ExitGUI();
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            m_TestRunnerUIFilter.Draw();
            EditorGUILayout.EndHorizontal();
        }

        public bool HasTreeData()
        {
            return m_TestListTree != null;
        }

        public virtual void RenderTestList()
        {
            if (m_TestListTree == null)
            {
                GUILayout.Label("Loading...");
                return;
            }

            m_TestListScroll = EditorGUILayout.BeginScrollView(m_TestListScroll,
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(2000));

            if (m_TestListTree.data.root == null || m_TestListTree.data.rowCount == 0 || (!m_TestListTree.isSearching && !m_TestListTree.data.GetItem(0).hasChildren))
            {
                if (m_TestRunnerUIFilter.IsFiltering)
                {
                    if (GUILayout.Button("Clear filters"))
                    {
                        m_TestRunnerUIFilter.Clear();
                        m_TestListTree.ReloadData();
                        m_Window.Repaint();
                    }
                }
                RenderNoTestsInfo();
            }
            else
            {
                var treeRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                var treeViewKeyboardControlId = GUIUtility.GetControlID(FocusType.Keyboard);

                m_TestListTree.OnGUI(treeRect, treeViewKeyboardControlId);
            }

            EditorGUILayout.EndScrollView();
        }

        public virtual void RenderNoTestsInfo()
        {
            EditorGUILayout.HelpBox("No tests to show", MessageType.Info);
        }

        public void RenderDetails()
        {
            m_TestInfoScroll = EditorGUILayout.BeginScrollView(m_TestInfoScroll);
            var resultTextSize = TestRunnerWindow.Styles.info.CalcSize(new GUIContent(m_ResultText));
            EditorGUILayout.SelectableLabel(m_ResultText, TestRunnerWindow.Styles.info,
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true),
                GUILayout.MinWidth(resultTextSize.x),
                GUILayout.MinHeight(resultTextSize.y));
            EditorGUILayout.EndScrollView();
        }

        public void Reload()
        {
            if (m_TestListTree != null)
            {
                m_TestListTree.ReloadData();
                UpdateQueuedResults();
            }
        }

        public void Repaint()
        {
            if (m_TestListTree == null || m_TestListTree.data.root == null)
            {
                return;
            }

            m_TestListTree.Repaint();
            if (m_TestListTree.data.rowCount == 0)
                m_TestListTree.SetSelection(new int[0], false);
            TestSelectionCallback(m_TestListState.selectedIDs.ToArray());
        }

        public void Init(TestRunnerWindow window, ITestAdaptor rootTest)
        {
            if (m_Window == null)
            {
                m_Window = window;
            }

            if (m_TestListTree == null)
            {
                if (m_TestListState == null)
                {
                    m_TestListState = new TreeViewState();
                }
                if (m_TestListTree == null)
                    m_TestListTree = new TreeViewController(m_Window, m_TestListState);

                m_TestListTree.deselectOnUnhandledMouseDown = false;

                m_TestListTree.selectionChangedCallback += TestSelectionCallback;
                m_TestListTree.itemDoubleClickedCallback += TestDoubleClickCallback;
                m_TestListTree.contextClickItemCallback += TestContextClickCallback;

                var testListTreeViewDataSource = new TestListTreeViewDataSource(m_TestListTree, this, rootTest);

                if (!newResultList.Any())
                    testListTreeViewDataSource.ExpandTreeOnCreation();

                m_TestListTree.Init(new Rect(),
                    testListTreeViewDataSource,
                    new TestListTreeViewGUI(m_TestListTree),
                    null);
            }

            EditorApplication.update += RepaintIfProjectPathChanged;

            m_TestRunnerUIFilter.UpdateCounters(newResultList);
            m_TestRunnerUIFilter.RebuildTestList = () => m_TestListTree.ReloadData();
            m_TestRunnerUIFilter.SearchStringChanged = s => m_TestListTree.searchString = s;
            m_TestRunnerUIFilter.SearchStringCleared = () => FrameSelection();
        }

        public void UpdateResult(TestRunnerResult result)
        {
            if (!HasTreeData())
            {
                m_QueuedResults.Add(result);
                return;
            }

            if (ResultsByKey.TryGetValue(result.uniqueId, out var testRunnerResult))
            {
                testRunnerResult.Update(result);
                Repaint();
                m_Window.Repaint();
            }
        }

        public void UpdateTestTree(ITestAdaptor test)
        {
            if (!HasTreeData())
            {
                return;
            }
            
            (m_TestListTree.data as TestListTreeViewDataSource).UpdateRootTest(test);
            
            m_TestListTree.ReloadData();
            Repaint();
            m_Window.Repaint();
        }

        private void UpdateQueuedResults()
        {
            foreach (var testRunnerResult in m_QueuedResults)
            {
                if (ResultsByKey.TryGetValue(testRunnerResult.uniqueId, out var existingResult))
                {
                    existingResult.Update(testRunnerResult);
                }
            }
            m_QueuedResults.Clear();
            TestSelectionCallback(m_TestListState.selectedIDs.ToArray());
            m_TestRunnerUIFilter.UpdateCounters(newResultList);
            Repaint();
            m_Window.Repaint();
        }

        internal void TestSelectionCallback(int[] selected)
        {
            if (m_TestListTree != null && selected.Length == 1)
            {
                if (m_TestListTree != null)
                {
                    var node = m_TestListTree.FindItem(selected[0]);
                    if (node is TestTreeViewItem)
                    {
                        var test = node as TestTreeViewItem;
                        m_ResultText = test.GetResultText();
                        m_ResultStacktrace = test.result.stacktrace;
                    }
                }
            }
            else if (selected.Length == 0)
            {
                m_ResultText = "";
            }
        }

        protected virtual void TestDoubleClickCallback(int id)
        {
            if (IsBusy())
                return;

            RunTests(GetSelectedTestsAsFilter(new List<int> { id }));
            GUIUtility.ExitGUI();
        }

        protected virtual void RunTests(params UITestRunnerFilter[] filters)
        {
            throw new NotImplementedException();
        }

        protected virtual void TestContextClickCallback(int id)
        {
            if (id == 0)
                return;

            var m = new GenericMenu();
            var testFilters = GetSelectedTestsAsFilter(m_TestListState.selectedIDs);
            var multilineSelection = m_TestListState.selectedIDs.Count > 1;

            if (!multilineSelection)
            {
                var testNode = GetSelectedTest();
                var isNotSuite = !testNode.IsGroupNode;
                if (isNotSuite)
                {
                    if (!string.IsNullOrEmpty(m_ResultStacktrace))
                    {
                        m.AddItem(s_GUIOpenErrorLine,
                            false,
                            data =>
                            {
                                if (!GuiHelper.OpenScriptInExternalEditor(m_ResultStacktrace))
                                {
                                    GuiHelper.OpenScriptInExternalEditor(testNode.type, testNode.method);
                                }
                            },
                            "");
                    }

                    m.AddItem(s_GUIOpenTest,
                        false,
                        data => GuiHelper.OpenScriptInExternalEditor(testNode.type, testNode.method),
                        "");
                    m.AddSeparator("");
                }
            }

            if (!IsBusy())
            {
                m.AddItem(multilineSelection ? s_GUIRunSelectedTests : s_GUIRun,
                    false,
                    data => RunTests(testFilters),
                    "");
            }
            else
                m.AddDisabledItem(multilineSelection ? s_GUIRunSelectedTests : s_GUIRun, false);

            m.ShowAsContext();
        }

        private UITestRunnerFilter[] GetSelectedTestsAsFilter(IEnumerable<int> selectedIDs)
        {
            var namesToRun = new List<string>();
            var assembliesForNamesToRun = new List<string>();
            var exactNamesToRun = new List<string>();
            var assembliesToRun = new List<string>();
            foreach (var lineId in selectedIDs)
            {
                var line = m_TestListTree.FindItem(lineId);
                if (line is TestTreeViewItem)
                {
                    var testLine = line as TestTreeViewItem;
                    if (testLine.IsGroupNode && !testLine.FullName.Contains("+")) 
                    {
                        if (testLine.parent != null && testLine.parent.displayName == "Invisible Root Item")
                        {
                            //Root node selected. Use an empty TestRunnerFilter to run every test
                            return new[] {new UITestRunnerFilter()};
                        }

                        if (testLine.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            assembliesToRun.Add(UITestRunnerFilter.AssemblyNameFromPath(testLine.FullName));
                        }
                        else
                        {
                            namesToRun.Add($"^{Regex.Escape(testLine.FullName)}$");
                            var assembly = UITestRunnerFilter.AssemblyNameFromPath(testLine.GetAssemblyName());
                            if (!string.IsNullOrEmpty(assembly) && !assembliesForNamesToRun.Contains(assembly))
                            {
                                assembliesForNamesToRun.Add(assembly);
                            }
                        }
                    }
                    else
                    {
                        exactNamesToRun.Add(testLine.FullName);
                    }
                }
            }

            var filters = new List<UITestRunnerFilter>();

            if (assembliesToRun.Count > 0)
            {
                filters.Add(new UITestRunnerFilter
                {
                    assemblyNames = assembliesToRun.ToArray()
                });
            }
            
            if (namesToRun.Count > 0)
            {
                filters.Add(new UITestRunnerFilter
                {
                    groupNames = namesToRun.ToArray(),
                    assemblyNames = assembliesForNamesToRun.ToArray()
                });
            }
            
            if (exactNamesToRun.Count > 0)
            {
                filters.Add(new UITestRunnerFilter
                {
                    testNames = exactNamesToRun.ToArray()
                });
            }
            
            if (filters.Count == 0)
            {
                filters.Add(new UITestRunnerFilter());
            }

            var categories = m_TestRunnerUIFilter.CategoryFilter.ToArray();
            if (categories.Length > 0)
            {
                foreach (var filter in filters)
                {
                    filter.categoryNames = categories;
                }
            }
            
            return filters.ToArray();
        }

        private TestTreeViewItem GetSelectedTest()
        {
            foreach (var lineId in m_TestListState.selectedIDs)
            {
                var line = m_TestListTree.FindItem(lineId);
                if (line is TestTreeViewItem)
                {
                    return line as TestTreeViewItem;
                }
            }
            return null;
        }

        private void FrameSelection()
        {
            if (m_TestListTree.HasSelection())
            {
                var firstClickedID = m_TestListState.selectedIDs.First() == m_TestListState.lastClickedID ? m_TestListState.selectedIDs.Last() : m_TestListState.selectedIDs.First();
                m_TestListTree.Frame(firstClickedID, true, false);
            }
        }

        public abstract TestPlatform TestPlatform { get; }

        public void RebuildUIFilter()
        {
            m_TestRunnerUIFilter.UpdateCounters(newResultList);
            if (m_TestRunnerUIFilter.IsFiltering)
            {
                m_TestListTree.ReloadData();
            }
        }

        public void RepaintIfProjectPathChanged()
        {
            var path = TestListGUIHelper.GetActiveFolderPath();
            if (path != m_PreviousProjectPath)
            {
                m_PreviousProjectPath = path;
                TestRunnerWindow.s_Instance.Repaint();
            }

            EditorApplication.update -= RepaintIfProjectPathChanged;
        }

        protected abstract bool IsBusy();
    }
}
