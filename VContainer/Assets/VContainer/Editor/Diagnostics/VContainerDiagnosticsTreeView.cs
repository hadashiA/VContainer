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

        public RegistrationBuilder RegistrationBuilder => DiagnosticsInfo?.RegisterInfo.RegistrationBuilder;
        public IRegistration Registration => DiagnosticsInfo?.ResolveInfo.Registration;
        public int? RefCount => DiagnosticsInfo?.ResolveInfo.RefCount;

        public string TypeSummary
        {
            get
            {
                if (Registration?.ImplementationType != null)
                {
                    return TypeNameHelper.GetTypeAlias(Registration.ImplementationType);
                }

                return "";
            }
        }

        public string ContractTypesSummary
        {
            get
            {
                if (Registration?.InterfaceTypes != null)
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
                return typeName;
            }
        }

        public DiagnosticsInfoTreeViewItem(string scopeName)
        {
            ScopeName = scopeName;
        }

        public DiagnosticsInfoTreeViewItem(DiagnosticsInfo info)
        {
            ScopeName = info.ScopeName;
            DiagnosticsInfo = info;
        }
    }

    public sealed class VContainerDiagnosticsInfoTreeView : TreeView
    {
        static readonly MultiColumnHeaderState.Column[] Columns = new[]
        {
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Type") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("ContractTypes"), canSort = false },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Lifetime") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Register") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("RefCount"), width = 5f },
        };

        static readonly MultiColumnHeaderState.Column[] FlattenColumns = new[]
        {
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Scope") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Type") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("ContractTypes"), canSort = false },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Lifetime") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Register") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("RefCount"), width = 5f },
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
                multiColumnHeader.state = flatten
                    ? new MultiColumnHeaderState(FlattenColumns)
                    : new MultiColumnHeaderState(Columns);
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

            header.sortedColumnIndex = SessionState.GetInt(SessionStateKeySortedColumnIndex, 0);
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
                switch (columnIndex)
                {
                    case 0:
                        items = ascending
                            ? items.OrderBy(x => x.ScopeName)
                            : items.OrderByDescending(x => x.ScopeName);
                        break;
                    case 1:
                        items = ascending
                            ? items.OrderBy(x => x.TypeSummary)
                            : items.OrderByDescending(x => x.TypeSummary);
                        break;
                    case 3:
                        items = ascending
                            ? items.OrderBy(x => x.Registration.Lifetime)
                            : items.OrderByDescending(x => x.Registration.Lifetime);
                        break;
                    case 4:
                        items = ascending
                            ? items.OrderBy(x => x.RegisterSummary)
                            : items.OrderByDescending(x => x.RegisterSummary);
                        break;
                    case 5:
                        items = ascending
                            ? items.OrderBy(x => x.RefCount)
                            : items.OrderByDescending(x => x.RefCount);
                        break;
                }
                rootItem.children = new List<TreeViewItem>(items);
            }
            else
            {
                foreach (var child in rootItem.children)
                {
                    var items = child.children.Cast<DiagnosticsInfoTreeViewItem>();
                    switch (columnIndex)
                    {
                        case 0:
                            items = ascending
                                ? items.OrderBy(x => x.TypeSummary)
                                : items.OrderByDescending(x => x.TypeSummary);
                            break;
                        case 2:
                            items = ascending
                                ? items.OrderBy(x => x.Registration.Lifetime)
                                : items.OrderByDescending(x => x.Registration.Lifetime);
                            break;
                        case 3:
                            items = ascending
                                ? items.OrderBy(x => x.RegisterSummary)
                                : items.OrderByDescending(x => x.RegisterSummary);
                            break;
                        case 4:
                            items = ascending
                                ? items.OrderBy(x => x.RefCount)
                                : items.OrderByDescending(x => x.RefCount);
                            break;
                    }
                    child.children = new List<TreeViewItem>(items);
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
                            displayName = info.ScopeName,
                            ScopeName = info.ScopeName,
                        });
                    }
                }
                else
                {
                    var grouped = DiagnositcsContext.GetGroupedDiagnosticsInfos();
                    foreach (var scope in grouped)
                    {
                        var scopeItem = new DiagnosticsInfoTreeViewItem(scope.Key)
                        {
                            id = NextId(),
                            depth = 0,
                            displayName = scope.Key
                        };
                        children.Add(scopeItem);
                        SetExpanded(scopeItem.id, true);

                        foreach (var info in scope)
                        {
                            AddChildItemRecursive(info, scopeItem);
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

            for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                var cellRect = args.GetCellRect(visibleColumnIndex);
                CenterRectUsingSingleLineHeight(ref cellRect);
                var columnIndex = args.GetColumn(visibleColumnIndex);

                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.MiddleLeft;

                if (columnIndex == 0)
                {
                    base.RowGUI(args);
                }
                else if (Flatten && columnIndex == 1)
                {
                    EditorGUI.LabelField(cellRect, item.TypeSummary, labelStyle);
                }
                else if (Flatten && columnIndex == 2 || columnIndex == 1)
                {
                    EditorGUI.LabelField(cellRect, item.ContractTypesSummary, labelStyle);
                }
                else if (Flatten && columnIndex == 3 || columnIndex == 2)
                {
                    EditorGUI.LabelField(cellRect, item.Registration?.Lifetime.ToString(), labelStyle);
                }
                else if (Flatten && columnIndex == 4 || columnIndex == 3)
                {
                    EditorGUI.LabelField(cellRect, item.RegisterSummary, labelStyle);
                }
                else if (Flatten && columnIndex == 5 || columnIndex == 4)
                {
                    EditorGUI.LabelField(cellRect, item.RefCount.ToString(), labelStyle);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                }
            }
        }

        void AddChildItemRecursive(DiagnosticsInfo info, DiagnosticsInfoTreeViewItem parent)
        {
            var item = new DiagnosticsInfoTreeViewItem(info)
            {
                id = NextId(),
                depth = parent.depth + 1,
                displayName = TypeNameHelper.GetTypeAlias(info.ResolveInfo.Registration.ImplementationType),
            };
            parent.AddChild(item);

            foreach (var dependency in info.Dependencies)
            {
                AddChildItemRecursive(dependency, item);
            }
        }
    }
}

