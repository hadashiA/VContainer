using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.PerformanceTesting.Data;

namespace Unity.PerformanceTesting.Editor
{
    class TestListTableItem : TreeViewItem
    {
        public int index;
        public PerformanceTestResult performanceTest;
        public double deviation;
        public double standardDeviation;
        public double median;
        public double min;
        public double max;

        public TestListTableItem(int id, int depth, string displayName, PerformanceTestResult performanceTest)
            : base(id, depth,
                displayName)
        {
            this.performanceTest = performanceTest;

            index = id;
            deviation = 0f;
            if (performanceTest != null)
            {
                foreach (var sample in performanceTest.SampleGroups)
                {
                    if (sample.Name == "Time")
                    {
                        standardDeviation = sample.StandardDeviation;
                        median = sample.Median;
                        min = sample.Min;
                        max = sample.Max;

                        deviation = standardDeviation / median;
                        break;
                    }

                    if (sample.Samples.Count <= 1)
                    {
                        standardDeviation = sample.StandardDeviation;
                        median = sample.Median;
                        min = sample.Min;
                        max = sample.Max;

                        deviation = standardDeviation / median;
                        break;
                    }

                    double thisDeviation = sample.StandardDeviation / sample.Median;
                    if (thisDeviation > deviation)
                    {
                        standardDeviation = sample.StandardDeviation;
                        median = sample.Median;
                        min = sample.Min;
                        max = sample.Max;

                        deviation = thisDeviation;
                    }
                }
            }
        }
    }

    class TestListTable : TreeView
    {
        TestReportWindow m_testReportWindow;

        const float kRowHeights = 20f;
        readonly List<TreeViewItem> m_Rows = new List<TreeViewItem>(100);

        // All columns
        public enum MyColumns
        {
            Index,
            Name,
            SampleCount,
            StandardDeviation,
            Deviation,
            Median,
            Min,
            Max,
        }

        public enum SortOption
        {
            Index,
            Name,
            SampleCount,
            StandardDeviation,
            Deviation,
            Median,
            Min,
            Max,
        }

        // Sort options per column
        SortOption[] m_SortOptions =
        {
            SortOption.Index,
            SortOption.Name,
            SortOption.SampleCount,
            SortOption.StandardDeviation,
            SortOption.Deviation,
            SortOption.Median,
            SortOption.Min,
            SortOption.Max,
        };

        public TestListTable(TreeViewState state, MultiColumnHeader multicolumnHeader,
            TestReportWindow testReportWindow)
            : base(state, multicolumnHeader)
        {
            m_testReportWindow = testReportWindow;

            Assert.AreEqual(m_SortOptions.Length, Enum.GetValues(typeof(MyColumns)).Length,
                "Ensure number of sort options are in sync with number of MyColumns enum values");

            // Custom setup
            rowHeight = kRowHeights;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset =
                (kRowHeights - EditorGUIUtility.singleLineHeight) *
                0.5f; // center foldout in the row since we also center content. See RowGUI

            // extraSpaceBeforeIconAndLabel = 0;
            multicolumnHeader.sortingChanged += OnSortingChanged;

            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            int idForhiddenRoot = -1;
            int depthForHiddenRoot = -1;
            TestListTableItem root = new TestListTableItem(idForhiddenRoot, depthForHiddenRoot, "root", null);

            var results = m_testReportWindow.GetResults();
            if (results != null)
            {
                int index = 0;
                foreach (var result in results.Results)
                {
                    var item = new TestListTableItem(index, 0, result.Name, result);
                    root.AddChild(item);

                    // Maintain index to map to main markers
                    index += 1;
                }
            }

            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            m_Rows.Clear();

            if (rootItem != null && rootItem.children != null)
            {
                foreach (TestListTableItem node in rootItem.children)
                {
                    m_Rows.Add(node);
                }
            }

            SortIfNeeded(m_Rows);

            return m_Rows;
        }

        void OnSortingChanged(MultiColumnHeader _multiColumnHeader)
        {
            SortIfNeeded(GetRows());
        }

        void SortIfNeeded(IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
            {
                return;
            }

            if (multiColumnHeader.sortedColumnIndex == -1)
            {
                return; // No column to sort for (just use the order the data are in)
            }

            // Sort the roots of the existing tree items
            SortByMultipleColumns();

            // Update the data with the sorted content
            rows.Clear();
            foreach (TestListTableItem node in rootItem.children)
            {
                rows.Add(node);
            }

            Repaint();
        }

        void SortByMultipleColumns()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
            {
                return;
            }

            var myTypes = rootItem.children.Cast<TestListTableItem>();
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (int i = 1; i < sortedColumns.Length; i++)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.Index:
                        orderedQuery = orderedQuery.ThenBy(l => l.index, ascending);
                        break;
                    case SortOption.Name:
                        orderedQuery = orderedQuery.ThenBy(l => l.displayName, ascending);
                        break;
                    case SortOption.SampleCount:
                        orderedQuery = orderedQuery.ThenBy(l => l.performanceTest.SampleGroups.Count, ascending);
                        break;
                    case SortOption.Deviation:
                        orderedQuery = orderedQuery.ThenBy(l => l.deviation, ascending);
                        break;
                    case SortOption.StandardDeviation:
                        orderedQuery = orderedQuery.ThenBy(l => l.standardDeviation, ascending);
                        break;
                    case SortOption.Median:
                        orderedQuery = orderedQuery.ThenBy(l => l.median, ascending);
                        break;
                    case SortOption.Min:
                        orderedQuery = orderedQuery.ThenBy(l => l.min, ascending);
                        break;
                    case SortOption.Max:
                        orderedQuery = orderedQuery.ThenBy(l => l.max, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TestListTableItem> InitialOrder(IEnumerable<TestListTableItem> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.Index:
                    return myTypes.Order(l => l.index, ascending);
                case SortOption.Name:
                    return myTypes.Order(l => l.displayName, ascending);
                case SortOption.SampleCount:
                    return myTypes.Order(l => l.performanceTest.SampleGroups.Count, ascending);
                case SortOption.Deviation:
                    return myTypes.Order(l => l.deviation, ascending);
                case SortOption.StandardDeviation:
                    return myTypes.Order(l => l.standardDeviation, ascending);
                case SortOption.Median:
                    return myTypes.Order(l => l.median, ascending);
                case SortOption.Min:
                    return myTypes.Order(l => l.min, ascending);
                case SortOption.Max:
                    return myTypes.Order(l => l.max, ascending);
                default:
                    Assert.IsTrue(false, "Unhandled enum");
                    break;
            }

            // default
            return myTypes.Order(l => l.index, ascending);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TestListTableItem)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TestListTableItem item, MyColumns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case MyColumns.Index:
                    EditorGUI.LabelField(cellRect, string.Format("{0}", item.index));
                    break;
                case MyColumns.Name:
                    EditorGUI.LabelField(cellRect, string.Format("{0}", item.displayName));
                    break;
                case MyColumns.SampleCount:
                    EditorGUI.LabelField(cellRect, string.Format("{0}", item.performanceTest.SampleGroups.Count));
                    break;
                case MyColumns.Deviation:
                    EditorGUI.LabelField(cellRect, string.Format("{0:f2}", item.deviation));
                    break;
                case MyColumns.StandardDeviation:
                    EditorGUI.LabelField(cellRect, string.Format("{0:f2}", item.standardDeviation));
                    break;
                case MyColumns.Median:
                    EditorGUI.LabelField(cellRect, string.Format("{0:f2}", item.median));
                    break;
                case MyColumns.Min:
                    EditorGUI.LabelField(cellRect, string.Format("{0:f2}", item.min));
                    break;
                case MyColumns.Max:
                    EditorGUI.LabelField(cellRect, string.Format("{0:f2}", item.max));
                    break;
            }
        }

        // Misc
        //--------

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        struct HeaderData
        {
            public readonly GUIContent content;
            public readonly float width;
            public readonly float minWidth;
            public readonly bool autoResize;
            public readonly bool allowToggleVisibility;
            public readonly bool ascending;

            public HeaderData(string name, string tooltip = "", float width = 100, float minWidth = 50, bool autoResize = true, bool allowToggleVisibility = true, bool ascending = false)
            {
                content = new GUIContent(name, tooltip);
                this.width = width;
                this.minWidth = minWidth;
                this.autoResize = autoResize;
                this.allowToggleVisibility = allowToggleVisibility;
                this.ascending = ascending;
            }
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columnList = new List<MultiColumnHeaderState.Column>();
            HeaderData[] headerData = new HeaderData[]
            {
                new HeaderData("Index", "Ordering from the test run", width : 40, minWidth : 50),
                new HeaderData("Name", "Name of test", width : 500, minWidth : 100, autoResize : false, allowToggleVisibility : false, ascending : true),
                new HeaderData("Groups", "Number of Sample Groups", width : 60, minWidth : 50),
                new HeaderData("SD", "Standard Deviation"), //  (of sample group with largest deviation)
                new HeaderData("Deviation", "Standard Deviation / Median"),
                new HeaderData("Median", "Median value"),
                new HeaderData("Min", "Min value"),
                new HeaderData("Max", "Max value"),
            };
            foreach (var header in headerData)
            {
                columnList.Add(new MultiColumnHeaderState.Column
                {
                    headerContent = header.content,
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = header.ascending,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = header.width,
                    minWidth = header.minWidth,
                    autoResize = header.autoResize,
                    allowToggleVisibility = header.allowToggleVisibility
                });
            };
            var columns = columnList.ToArray();

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(MyColumns)).Length,
                "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            state.visibleColumns = new int[]
            {
                (int)MyColumns.Index,
                (int)MyColumns.Name,
                (int)MyColumns.SampleCount,
                (int)MyColumns.Deviation,
                (int)MyColumns.StandardDeviation,
                (int)MyColumns.Median,
                (int)MyColumns.Min,
                (int)MyColumns.Max,
            };
            return state;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            if (selectedIds.Count > 0)
                m_testReportWindow.SelectTest(selectedIds[0]);
        }
    }

    static class MyExtensionMethods
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector,
            bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector,
            bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }
    }
}
