using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using VContainer.Diagnostics;
using VContainer.Unity;

namespace VContainer.Editor.Diagnostics
{
    public sealed class DiagnosticsInfoTreeViewItem : TreeViewItem
    {
        public string ScopeName { get; set; }
        public DiagnosticsInfo DiagnosticsInfo { get; }

        public RegistrationBuilder RegistrationBuilder => DiagnosticsInfo.RegisterInfo.RegistrationBuilder;
        public Registration Registration => DiagnosticsInfo.ResolveInfo.Registration;
        public int? RefCount => DiagnosticsInfo.ResolveInfo.RefCount;

        public string TypeSummary => TypeNameHelper.GetTypeAlias(Registration.ImplementationType);

        public string ContractTypesSummary
        {
            get
            {
                if (Registration.InterfaceTypes != null)
                {
                    var values = Registration.InterfaceTypes.Select(TypeNameHelper.GetTypeAlias);
                    return string.Join(", ", values);
                }
                return "";
            }
        }

        public string RegisterSummary
        {
            get
            {
                if (RegistrationBuilder == null)
                    return "";

                var type = RegistrationBuilder.GetType();
                if (type == typeof(RegistrationBuilder))
                {
                    return "";
                }

                var typeName = type.Name;
                var suffixIndex = typeName.IndexOf("Builder");
                if (suffixIndex > 0)
                {
                    typeName = typeName.Substring(0, suffixIndex);
                }
                suffixIndex = typeName.IndexOf("Registration");
                if (suffixIndex > 0)
                {
                    typeName = typeName.Substring(0, suffixIndex);
                }

                if (typeName.StartsWith("Instance") && TypeSummary.StartsWith("Func<"))
                {
                    return "FuncFactory";
                }

                return typeName;
            }
        }

        public DiagnosticsInfoTreeViewItem(DiagnosticsInfo info)
        {
            ScopeName = info.ScopeName;
            DiagnosticsInfo = info;
            displayName = TypeSummary;
        }
    }

    public sealed class VContainerDiagnosticsInfoTreeView : TreeView
    {
        static readonly MultiColumnHeaderState.Column[] Columns =
        {
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Type") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("ContractTypes"), canSort = false },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Lifetime"), width = 15f },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Register"), width = 15f },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("RefCount"), width = 5f },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Scope"), width = 20f },
        };

        static int idSeed;
        static int NextId() => ++idSeed;

        const string SessionStateKeySortedColumnIndex = "VContainer.Editor.DiagnosticsInfoTreeView:sortedColumnIndex";

        public bool Flatten
        {
            get => flatten;
            set
            {
                flatten = value;
                multiColumnHeader.ResizeToFit();
            }
        }

        bool flatten;

        public VContainerDiagnosticsInfoTreeView()
            : this(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(Columns)))
        {
        }

        VContainerDiagnosticsInfoTreeView(TreeViewState state, MultiColumnHeader header)
            : base(state, header)
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            header.sortingChanged += OnSortedChanged;

            header.ResizeToFit();
            Reload();

            header.sortedColumnIndex = Math.Min(
                header.state.columns.Length - 1,
                SessionState.GetInt(SessionStateKeySortedColumnIndex, 0));
        }

        public DiagnosticsInfoTreeViewItem GetSelectedItem()
        {
            if (state.selectedIDs.Count <= 0) return null;

            var selectedId = state.selectedIDs[0];
            return GetRows().FirstOrDefault(x => x.id == selectedId) as DiagnosticsInfoTreeViewItem;
        }

        public void ReloadAndSort()
        {
            var currentSelected = state.selectedIDs;
            Reload();
            OnSortedChanged(multiColumnHeader);
            state.selectedIDs = currentSelected;
        }

        void OnSortedChanged(MultiColumnHeader multiColumnHeader)
        {
            var columnIndex = multiColumnHeader.sortedColumnIndex;
            if (columnIndex < 0) return;

            SessionState.SetInt(SessionStateKeySortedColumnIndex, columnIndex);
            var ascending = multiColumnHeader.IsSortedAscending(columnIndex);

            if (Flatten)
            {
                var items = rootItem.children.Cast<DiagnosticsInfoTreeViewItem>();
                rootItem.children = new List<TreeViewItem>(Sort(items, columnIndex, ascending));
            }
            else
            {
                foreach (var sectionHeaderItem in rootItem.children)
                {
                    var items = sectionHeaderItem.children.Cast<DiagnosticsInfoTreeViewItem>();
                    sectionHeaderItem.children = new List<TreeViewItem>(Sort(items, columnIndex, ascending));
                }
            }
            BuildRows(rootItem);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { depth = -1 };
            var children = new List<TreeViewItem>();

            if (VContainerSettings.DiagnosticsEnabled)
            {
                if (Flatten)
                {
                    var infos = DiagnositcsContext.GetDiagnosticsInfos();
                    foreach (var info in infos)
                    {
                        children.Add(new DiagnosticsInfoTreeViewItem(info)
                        {
                            id = NextId(),
                            depth = 0,
                            ScopeName = info.ScopeName,
                        });
                    }
                }
                else
                {
                    var grouped = DiagnositcsContext.GetGroupedDiagnosticsInfos();
                    foreach (var scope in grouped)
                    {
                        var sectionHeaderItem = new TreeViewItem(NextId(), 0, scope.Key);
                        children.Add(sectionHeaderItem);
                        SetExpanded(sectionHeaderItem.id, true);

                        foreach (var info in scope)
                        {
                            AddDependencyItemsRecursive(info, sectionHeaderItem);
                        }
                    }
                }
            }

            root.children = children;
            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false;

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as DiagnosticsInfoTreeViewItem;
            if (item is null)
            {
                base.RowGUI(args);
                return;
            }

            for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                var cellRect = args.GetCellRect(visibleColumnIndex);
                // CenterRectUsingSingleLineHeight(ref cellRect);
                var columnIndex = args.GetColumn(visibleColumnIndex);

                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.MiddleLeft;

                switch (columnIndex)
                {
                    case 0:
                        base.RowGUI(args);
                        break;
                    case 1:
                        EditorGUI.LabelField(cellRect, item.ContractTypesSummary, labelStyle);
                        break;
                    case 2:
                        EditorGUI.LabelField(cellRect, item.Registration.Lifetime.ToString(), labelStyle);
                        break;
                    case 3:
                        EditorGUI.LabelField(cellRect, item.RegisterSummary, labelStyle);
                        break;
                    case 4:
                        EditorGUI.LabelField(cellRect, item.RefCount.ToString(), labelStyle);
                        break;
                    case 5:
                        EditorGUI.LabelField(cellRect, item.ScopeName, labelStyle);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                }
            }
        }

        void AddDependencyItemsRecursive(DiagnosticsInfo info, TreeViewItem parent)
        {
            var item = new DiagnosticsInfoTreeViewItem(info)
            {
                id = NextId(),
                depth = parent.depth + 1
            };
            parent.AddChild(item);
            SetExpanded(item.id, item.depth <= 1);

            foreach (var dependency in info.Dependencies)
            {
                AddDependencyItemsRecursive(dependency, item);
            }
        }

        IEnumerable<DiagnosticsInfoTreeViewItem> Sort(
            IEnumerable<DiagnosticsInfoTreeViewItem> items,
            int sortedColumnIndex,
            bool ascending)
        {
            switch (sortedColumnIndex)
            {
                case 0:
                    return ascending
                        ? items.OrderBy(x => x.TypeSummary)
                        : items.OrderByDescending(x => x.TypeSummary);
                case 2:
                    return ascending
                        ? items.OrderBy(x => x.Registration.Lifetime)
                        : items.OrderByDescending(x => x.Registration.Lifetime);
                case 3:
                    return ascending
                        ? items.OrderBy(x => x.RegisterSummary)
                        : items.OrderByDescending(x => x.RegisterSummary);
                case 4:
                    return ascending
                        ? items.OrderBy(x => x.RefCount)
                        : items.OrderByDescending(x => x.RefCount);
                case 5:
                    return ascending
                        ? items.OrderBy(x => x.ScopeName)
                        : items.OrderByDescending(x => x.ScopeName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortedColumnIndex), sortedColumnIndex, null);
            }
        }
    }
}
