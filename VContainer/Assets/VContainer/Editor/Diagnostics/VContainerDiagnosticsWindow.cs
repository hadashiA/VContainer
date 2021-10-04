using System;
using UnityEditor;
using UnityEngine;
using VContainer.Unity;

namespace VContainer.Editor.Diagnostics
{
    public sealed class VContainerDiagnosticsWindow : EditorWindow
    {
        static VContainerDiagnosticsWindow window;

        static readonly GUIContent EnableAutoReloadHeadContent = EditorGUIUtility.TrTextContent("Enable AutoReload", "Reload view automatically.");
        static readonly GUIContent EnableCollapseHeadContent = EditorGUIUtility.TrTextContent("Collapse", "Collapse StackTraces.");
        static readonly GUIContent ReloadHeadContent = EditorGUIUtility.TrTextContent("Reload", "Reload View.");

        internal static bool EnableAutoReload;
        internal static bool EnableCaptureStackTrace;
        internal static bool EnableCollapse = true;

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

        object splitterState;
        Vector2 tableScrollPosition;
        Vector2 detailsScrollPosition;

        void OnEnable()
        {
            window = this; // set singleton.
            splitterState = SplitterGUILayout.CreateSplitterState(new [] { 75f, 25f }, new [] { 32, 32 }, null);
            treeView = new VContainerDiagnosticsInfoTreeView();
        }

        void OnGUI()
        {
            RenderHeadPanel();

            // Splittable
            SplitterGUILayout.BeginVerticalSplit(splitterState, Array.Empty<GUILayoutOption>());
            {
                RenderTable();
                RenderDetailsPanel();
            }
            SplitterGUILayout.EndVerticalSplit();
        }

        // [Enable CaptureStackTrace] | [Enable AutoReload] | .... | Reload
        void RenderHeadPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Toggle(EnableCollapse, EnableCollapseHeadContent, EditorStyles.toolbarButton) != EnableCollapse)
                {
                    EnableCollapse = !EnableCollapse;
                    treeView.ReloadAndSort();
                    Repaint();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(ReloadHeadContent, EditorStyles.toolbarButton))
                {
                    treeView.ReloadAndSort();
                    Repaint();
                }
            }
        }

        void RenderTable()
        {
            using (new EditorGUILayout.VerticalScope(TableListStyle))
            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(tableScrollPosition,
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(2000f)))
            {
                tableScrollPosition = scrollViewScope.scrollPosition;

                var controlRect = EditorGUILayout.GetControlRect(
                    GUILayout.ExpandHeight(true),
                    GUILayout.ExpandWidth(true));

                treeView?.OnGUI(new Rect(0, 0, position.width, position.height));
            }
        }

        void RenderDetailsPanel()
        {
            var message = "";
            if (LifetimeScope.DiagnosticsEnabled)
            {
                var selectedItem = treeView.GetSelectedItem();
                if (selectedItem != null)
                {
                    var registerInfo = selectedItem.RegisterInfo;
                    message = $"<a href=\"{registerInfo.GetScriptAssetPath()}\" line=\"{registerInfo.GetFileLineNumber()}\">Register at {registerInfo.GetHeadline()}</a>" +
                              Environment.NewLine +
                              Environment.NewLine +
                              selectedItem.RegisterInfo.StackTrace;
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