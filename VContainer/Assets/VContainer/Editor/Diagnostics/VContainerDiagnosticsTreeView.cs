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
        public RegisterInfo RegisterInfo { get; set; }
        public ResolveInfo ResolveInfo { get; set; }

        public string ContractTypesSummary
        {
            get
            {
                if (ResolveInfo?.Registration.InterfaceTypes != null)
                {
                    return string.Join(", ", ResolveInfo.Registration.InterfaceTypes.Select(x => x.Name));
                }
                return "";
            }
        }

        public string RegisterSummary
        {
            get
            {
                if (RegisterInfo == null)
                    return "";

                var type = RegisterInfo.RegistrationBuilder.GetType();
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

        public DiagnosticsInfoTreeViewItem(int id) : base(id)
        {
        }
    }

    public sealed class VContainerDiagnosticsInfoTreeView : TreeView
    {
        const string SortedColumnIndexStateKey = "VContainer.Editor.DiagnosticsInfoTreeView_sortedColumnIndex";

        static readonly MultiColumnHeaderState.Column[] CollapsedColumns = new[]
        {
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Type") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("ContractTypes") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Lifetime") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Register") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("RefCount"), width = 5f },
        };

        static readonly MultiColumnHeaderState.Column[] ExpandedColumns = new[]
        {
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Scope") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Type") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("ContractTypes") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Lifetime") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Register") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("RefCount"), width = 5f },
        };

        static int idSeed;
        static int NextId() => ++idSeed;

        public bool EnableCollapsed
        {
            get => enableCollapsed;
            set
            {
                enableCollapsed = value;
                multiColumnHeader.state = enableCollapsed
                    ? new MultiColumnHeaderState(CollapsedColumns)
                    : new MultiColumnHeaderState(ExpandedColumns);
            }
        }

        bool enableCollapsed = true;

        public VContainerDiagnosticsInfoTreeView()
            : this(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(CollapsedColumns)))
        {
        }

        VContainerDiagnosticsInfoTreeView(TreeViewState state, MultiColumnHeader header)
            : base(state, header)
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            extraSpaceBeforeIconAndLabel = 20;
            showBorder = true;
            header.sortingChanged += OnSortedChanged;

            header.ResizeToFit();
            Reload();

            header.sortedColumnIndex = SessionState.GetInt(SortedColumnIndexStateKey, 1);
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
            // SessionState.SetInt(SortedColumnIndexStateKey, multiColumnHeader.sortedColumnIndex);
            // var index = multiColumnHeader.sortedColumnIndex;
            // var ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
            //
            // var items = rootItem.children.Cast<DiagnosticsInfoTreeViewItem>();
            //
            // IOrderedEnumerable<DiagnosticsInfoTreeViewItem> orderedEnumerable;
            // switch (index)
            // {
            //     case 0:
            //         orderedEnumerable = ascending ? items.OrderBy(item => item.Head) : items.OrderByDescending(item => item.Head);
            //         break;
            //     case 1:
            //         orderedEnumerable = ascending ? items.OrderBy(item => item.Elapsed) : items.OrderByDescending(item => item.Elapsed);
            //         break;
            //     case 2:
            //         orderedEnumerable = ascending ? items.OrderBy(item => item.Count) : items.OrderByDescending(item => item.Count);
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(index), index, null);
            // }
            //
            // CurrentBindingItems = rootItem.children = orderedEnumerable.Cast<TreeViewItem>().ToList();
            BuildRows(rootItem);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { depth = -1 };
            var children = new List<TreeViewItem>();

            if (LifetimeScope.DiagnosticsEnabled)
            {
                if (EnableCollapsed)
                {
                    var grouped = DiagnositcsContext.GetGroupedDiagnosticsInfos();
                    foreach (var scope in grouped)
                    {
                        var scopeItem = new DiagnosticsInfoTreeViewItem(NextId())
                        {
                            depth = 0,
                            displayName = scope.Key,
                            ScopeName = scope.Key,
                        };
                        children.Add(scopeItem);
                        SetExpanded(scopeItem.id, true);

                        foreach (var info in scope)
                        {
                            AddChildItemRecursive(info, scopeItem);
                        }
                    }
                }
                else
                {
                    var infos = DiagnositcsContext.GetDiagnosticsInfos();
                    foreach (var info in infos)
                    {
                        children.Add(new DiagnosticsInfoTreeViewItem(NextId())
                        {
                            depth = 0,
                            displayName = info.ScopeName,
                            ScopeName = info.ScopeName,
                            RegisterInfo = info.RegisterInfo,
                            ResolveInfo = info.ResolveInfo
                        });
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

                if (EnableCollapsed)
                {
                    // Columns of collapsed
                    switch (columnIndex)
                    {
                        case 0:
                            base.RowGUI(args);
                            break;
                        case 1:
                            EditorGUI.LabelField(cellRect, item.ContractTypesSummary, labelStyle);
                            break;
                        case 2:
                            EditorGUI.LabelField(cellRect, item.ResolveInfo?.Registration?.Lifetime.ToString(), labelStyle);
                            break;
                        case 3:
                            EditorGUI.LabelField(cellRect, item.RegisterSummary, labelStyle);
                            break;
                        case 4:
                            EditorGUI.LabelField(cellRect, item.ResolveInfo?.RefCount.ToString(), labelStyle);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                    }
                }
                else
                {
                    // Columns of expanded
                    switch (columnIndex)
                    {
                        case 0:
                            // EditorGUI.LabelField(rect, item.ScopeName, labelStyle);
                            base.RowGUI(args);
                            break;
                        case 1:
                            EditorGUI.LabelField(cellRect, item.RegisterInfo?.RegistrationBuilder.ImplementationType.Name, labelStyle);
                            break;
                        case 2:
                            EditorGUI.LabelField(cellRect, item.ContractTypesSummary, labelStyle);
                            break;
                        case 3:
                            EditorGUI.LabelField(cellRect, item.ResolveInfo?.Registration?.Lifetime.ToString(), labelStyle);
                            break;
                        case 4:
                            EditorGUI.LabelField(cellRect, item.RegisterSummary, labelStyle);
                            break;
                        case 5:
                            EditorGUI.LabelField(cellRect, item.ResolveInfo?.RefCount.ToString(), labelStyle);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                    }
                }
            }
        }

        void AddChildItemRecursive(DiagnosticsInfo info, DiagnosticsInfoTreeViewItem parent)
        {
            var item = new DiagnosticsInfoTreeViewItem(NextId())
            {
                depth = parent.depth + 1,
                displayName = info.RegisterInfo?.RegistrationBuilder.ImplementationType.Name,
                ScopeName = parent.ScopeName,
                RegisterInfo = info.RegisterInfo,
                ResolveInfo = info.ResolveInfo,
            };
            parent.AddChild(item);

            foreach (var dependency in info.Dependencies)
            {
                AddChildItemRecursive(dependency, item);
            }
        }
    }
}

