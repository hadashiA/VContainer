#if UNITY_2022_2_OR_NEWER

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualizationSettings = UnityEditor.AI.NavMeshVisualizationSettings;

namespace Unity.AI.Navigation.Editor
{
    class NavigationPreferencesProvider : SettingsProvider
    {
        class Styles
        {
            internal static readonly GUIContent NavMeshVisualizationSettingsLabel =
                EditorGUIUtility.TrTextContent("NavMesh Visualization Settings");

            internal static readonly GUIContent SelectedSurfacesOpacityLabel =
                EditorGUIUtility.TrTextContent("Selected Surfaces Opacity", "Controls the mesh transparency for surfaces inside the selection hierarchy");

            internal static readonly GUIContent UnselectedSurfacesOpacityLabel =
                EditorGUIUtility.TrTextContent("Unselected Surfaces Opacity", "Controls the mesh transparency for surfaces outside the selection hierarchy");

            internal static readonly GUIContent HeightMeshColorLabel =
                EditorGUIUtility.TrTextContent("Height Mesh Color", "Color used to display height mesh information in the scene view");

            internal static readonly GUIContent ResetVisualizationSettingsButtonLabel =
                EditorGUIUtility.TrTextContent("Reset to Defaults", "Revert visualization settings to their original values. Customized values will be lost");
        }

        NavigationPreferencesProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            using (new SettingsWindow.GUIScope())
            {
                EditorGUILayout.LabelField(Styles.NavMeshVisualizationSettingsLabel, EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                
                // Visualization settings
                VisualizationSettings.selectedSurfacesOpacity = EditorGUILayout.Slider(Styles.SelectedSurfacesOpacityLabel, VisualizationSettings.selectedSurfacesOpacity, 0, 1);
                VisualizationSettings.unselectedSurfacesOpacity = EditorGUILayout.Slider(Styles.UnselectedSurfacesOpacityLabel, VisualizationSettings.unselectedSurfacesOpacity, 0, 1);
                VisualizationSettings.heightMeshColor = EditorGUILayout.ColorField(Styles.HeightMeshColorLabel, VisualizationSettings.heightMeshColor);

                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                // Option to reset the visualization settings to their default values
                if (GUILayout.Button(Styles.ResetVisualizationSettingsButtonLabel, GUILayout.Width(160)))
                {
                    VisualizationSettings.ResetSelectedSurfacesOpacity();
                    VisualizationSettings.ResetUnselectedSurfacesOpacity();
                    VisualizationSettings.ResetHeightMeshColor();
                }
                EditorGUILayout.Space(10);
                EditorGUILayout.EndHorizontal();

                // Repaint the scene view when settings changed to update the visualization accordingly
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }
        }

        [SettingsProvider]
        internal static SettingsProvider CreateNavigationProjectSettingProvider()
        {
            var provider = new NavigationPreferencesProvider("Preferences/AI Navigation", SettingsScope.User, GetSearchKeywordsFromGUIContentProperties<Styles>());
            return provider;
        }
    }
}

#endif
