using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VContainer.Diagnostics;
using VContainer.Unity;

namespace VContainer.Editor.Diagnostics
{
    public sealed class VContainerDiagnosticsWindow : EditorWindow
    {
        static VContainerDiagnosticsWindow window;

        static readonly GUIContent FlattenHeadContent = EditorGUIUtility.TrTextContent("Flatten", "Flatten dependencies");
        static readonly GUIContent ReloadHeadContent = EditorGUIUtility.TrTextContent("Reload", "Reload View");

        internal static bool EnableAutoReload;
        internal static bool EnableCaptureStackTrace;

        [MenuItem("Window/VContainer Diagnostics")]
        public static void OpenWindow()
        {
            if (window != null)
            {
                window.Close();
            }
            GetWindow<VContainerDiagnosticsWindow>("VContainer Diagnostics").Show();
        }

        GUIStyle TableListStyle
        {
            get
            {
                var style = new GUIStyle("CN Box");
                style.margin.top = 0;
                style.padding.left = 3;
                return style;
            }
        }

        GUIStyle DetailsStyle
        {
            get
            {
                var detailsStyle = new GUIStyle("CN Message");
                detailsStyle.wordWrap = false;
                detailsStyle.stretchHeight = true;
                detailsStyle.margin.right = 15;
                return detailsStyle;
            }
        }

        VContainerDiagnosticsInfoTreeView treeView;
        VContainerInstanceTreeView instanceTreeView;
        SearchField searchField;

        object verticalSplitterState;
        object horizontalSplitterState;
        Vector2 tableScrollPosition;
        Vector2 detailsScrollPosition;
        Vector2 instanceScrollPosition;

        public void Reload(IObjectResolver resolver)
        {
            treeView.ReloadAndSort();
            Repaint();
        }

        void OnPlayModeStateChange(PlayModeStateChange state)
        {
            treeView.ReloadAndSort();
            Repaint();
        }

        void OnEnable()
        {
            window = this; // set singleton.
            verticalSplitterState = SplitterGUILayout.CreateSplitterState(new [] { 75f, 25f }, new [] { 32, 32 }, null);
            horizontalSplitterState = SplitterGUILayout.CreateSplitterState(new[] { 75, 25f }, new[] { 32, 32 }, null);
            treeView = new VContainerDiagnosticsInfoTreeView();
            instanceTreeView = new VContainerInstanceTreeView();
            searchField = new SearchField();

            DiagnositcsContext.OnContainerBuilt += Reload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        void OnDisable()
        {
            DiagnositcsContext.OnContainerBuilt -= Reload;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
        }

        void OnGUI()
        {
            RenderHeadPanel();

            SplitterGUILayout.BeginVerticalSplit(verticalSplitterState, Array.Empty<GUILayoutOption>());
            {
                SplitterGUILayout.BeginHorizontalSplit(horizontalSplitterState);
                {
                    RenderBuildPanel();
                    RenderInstancePanel();
                }
                SplitterGUILayout.EndHorizontalSplit();

                RenderStackTracePanel();
            }
            SplitterGUILayout.EndVerticalSplit();
        }

        void RenderHeadPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var flattenOn = GUILayout.Toggle(treeView.Flatten, FlattenHeadContent, EditorStyles.toolbarButton);
                if (flattenOn != treeView.Flatten)
                {
                    treeView.Flatten = flattenOn;
                    treeView.ReloadAndSort();
                    Repaint();
                }

                GUILayout.FlexibleSpace();

                treeView.searchString = searchField.OnToolbarGUI(treeView.searchString);

                if (GUILayout.Button(ReloadHeadContent, EditorStyles.toolbarButton))
                {
                    treeView.ReloadAndSort();
                    Repaint();
                }
            }
        }

        void RenderBuildPanel()
        {
            using (new EditorGUILayout.VerticalScope(TableListStyle))
            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(tableScrollPosition,
                // true,
                // true,
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(2000f)))
            {
                tableScrollPosition = scrollViewScope.scrollPosition;

                var controlRect = EditorGUILayout.GetControlRect(
                    GUILayout.ExpandHeight(true),
                    GUILayout.ExpandWidth(true));
                treeView?.OnGUI(controlRect);
            }
        }

        void RenderInstancePanel()
        {
            if (!VContainerSettings.DiagnosticsEnabled)
            {
                return;
            }

            var selectedItem = treeView.GetSelectedItem();
            if (selectedItem?.DiagnosticsInfo.ResolveInfo is ResolveInfo resolveInfo)
            {
                if (resolveInfo.Instances.Count > 0)
                {
                    instanceTreeView.CurrentDiagnosticsInfo = selectedItem.DiagnosticsInfo;
                    instanceTreeView.Reload();

                    using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(instanceScrollPosition, GUILayout.ExpandHeight(true)))
                    {
                        instanceScrollPosition = scrollViewScope.scrollPosition;
                        var controlRect = EditorGUILayout.GetControlRect(
                            GUILayout.ExpandHeight(true),
                            GUILayout.ExpandWidth(true));
                        instanceTreeView?.OnGUI(controlRect);
                    }
                }
                else
                {
                    EditorGUILayout.SelectableLabel("No instance reference");
                }
            }
        }

        void RenderStackTracePanel()
        {
            var message = "";
            if (VContainerSettings.DiagnosticsEnabled)
            {
                var selectedItem = treeView.GetSelectedItem();
                if (selectedItem?.DiagnosticsInfo?.RegisterInfo is RegisterInfo registerInfo)
                {
                    message = $"<a href=\"{registerInfo.GetScriptAssetPath()}\" line=\"{registerInfo.GetFileLineNumber()}\">Register at {registerInfo.GetHeadline()}</a>" +
                              Environment.NewLine +
                              Environment.NewLine +
                              selectedItem.DiagnosticsInfo.RegisterInfo.StackTrace;
                }
            }
            else
            {
                message = "VContainer Diagnostics collector is disabled. To enable, please check VContainerSettings.";
            }
            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(detailsScrollPosition))
            {
                detailsScrollPosition = scrollViewScope.scrollPosition;
                var vector = DetailsStyle.CalcSize(new GUIContent(message));
                EditorGUILayout.SelectableLabel(message, DetailsStyle,
                    GUILayout.ExpandHeight(true),
                    GUILayout.ExpandWidth(true),
                    GUILayout.MinWidth(vector.x),
                    GUILayout.MinHeight(vector.y));
            }
        }
   }
}
