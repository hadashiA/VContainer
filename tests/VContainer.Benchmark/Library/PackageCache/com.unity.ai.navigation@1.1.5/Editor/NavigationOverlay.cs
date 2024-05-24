#if UNITY_2022_2_OR_NEWER

using UnityEditor;
using UnityEditor.AI;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AI.Navigation.Editor
{
    [Overlay(typeof(SceneView), "AINavigationOverlay", "AI Navigation", defaultDisplay = true)]
    [Icon(NavMeshComponentsGUIUtility.k_PackageEditorResourcesFolder + "Overlay/NavigationOverlay.png")]
    internal class NavigationOverlay : Overlay
    {
        static class Style
        {
            internal static readonly GUIContent SurfacesSectionTexts =
                EditorGUIUtility.TrTextContent("Surfaces");

            internal static readonly GUIContent SurfacesSelectedOnlyTexts =
                EditorGUIUtility.TrTextContent("Show Only Selected", "Check this to hide surfaces which are not part of the selection hierarchy");

            internal static readonly GUIContent SurfacesNavMeshTexts =
                EditorGUIUtility.TrTextContent("Show NavMesh", "Display navigation mesh using the associated area's color");

            internal static readonly GUIContent SurfacesHeightMeshTexts =
                EditorGUIUtility.TrTextContent("Show HeightMesh", "Display height mesh used for accurate vertical placement");

            internal static readonly GUIContent AgentsSectionTexts =
                EditorGUIUtility.TrTextContent("Agents");

            internal static readonly GUIContent AgentsPathPolysTexts =
                EditorGUIUtility.TrTextContent("Show Path Polygons", "Shows the polygons leading to goal.");

            internal static readonly GUIContent AgentsPathNodesTexts =
                EditorGUIUtility.TrTextContent("Show Path Query Nodes", "Shows the nodes expanded during last path query.");

            internal static readonly GUIContent AgentsNeighboursTexts =
                EditorGUIUtility.TrTextContent("Show Neighbours", "Show the agent neighbours considered during simulation.");

            internal static readonly GUIContent AgentsWallsTexts =
                EditorGUIUtility.TrTextContent("Show Walls", "Shows the wall segments handled during simulation.");

            internal static readonly GUIContent AgentsAvoidanceTexts =
                EditorGUIUtility.TrTextContent("Show Avoidance", "Shows the processed avoidance geometry from simulation.");

            internal static readonly GUIContent AgentsAvoidancePendingDebugRequestTexts =
                EditorGUIUtility.TrTextContent("Avoidance display is not valid until after next game update.", "Avoidance information will be computed on the next game update");

            internal static readonly GUIContent AgentsAvoidanceDebugRequestsCountExceededTexts =
                EditorGUIUtility.TrTextContent("", "Avoidance information display is limited to a fixed number of agents"); // This text is dynamic

            internal static readonly GUIContent ObstaclesSectionTexts =
                EditorGUIUtility.TrTextContent("Obstacles");

            internal static readonly GUIContent ObstaclesCarveHullText =
                EditorGUIUtility.TrTextContent("Show Carve Hull", "Shows the hull used to carve the obstacle from navmesh.");

            internal static readonly GUIContent DeveloperModeSectionTexts =
                EditorGUIUtility.TrTextContent("Developer Mode");

            internal static readonly GUIContent SurfacesPortalsTexts =
                EditorGUIUtility.TrTextContent("Show NavMesh Portals");

            internal static readonly GUIContent SurfacesTileLinksTexts =
                EditorGUIUtility.TrTextContent("Show NavMesh Tile Links");

            internal static readonly GUIContent SurfacesHeightMeshBVTreeTexts =
                EditorGUIUtility.TrTextContent("Show HeightMesh BV-Tree");

            internal static readonly GUIContent SurfacesHeightMapsTexts =
                EditorGUIUtility.TrTextContent("Show HeightMaps", "Display terrain height maps used for accurate vertical placement");

            internal static readonly GUIContent SurfacesProximityGridTexts =
                EditorGUIUtility.TrTextContent("Show Proximity Grid");

            internal static readonly GUIContent NavigationVisualizationDisabledTexts =
                EditorGUIUtility.TrTextContent("Navigation visualization is not available in prefab edition.", "");
        }

        VisualElement m_RootPanel;
        VisualElement m_OptionsPanel;
        VisualElement m_AgentFoldOut;
        HelpBox m_VisualizationDisabledHelpBox;
        HelpBox m_AgentCountWarning;
        HelpBox m_AgentPendingRequestWarning;

        public override void OnCreated()
        {
            base.OnCreated();

            NavMeshEditorHelpers.agentRejectedDebugInfoRequestsCountChanged += OnAgentRejectedDebugInfoRequestsCountChanged;
            NavMeshEditorHelpers.agentDebugRequestsPending += DisplayAgentPendingRequestWarningBox;
            NavMeshEditorHelpers.agentDebugRequestsProcessed += HideAgentPendingRequestWarningBox;

            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
        }

        public override void OnWillBeDestroyed()
        {
            NavMeshEditorHelpers.agentRejectedDebugInfoRequestsCountChanged -= OnAgentRejectedDebugInfoRequestsCountChanged;
            NavMeshEditorHelpers.agentDebugRequestsPending -= DisplayAgentPendingRequestWarningBox;
            NavMeshEditorHelpers.agentDebugRequestsProcessed -= HideAgentPendingRequestWarningBox;

            PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;

            base.OnWillBeDestroyed();
        }

        public override VisualElement CreatePanelContent()
        {
            m_RootPanel = new VisualElement();

            m_OptionsPanel = new VisualElement();
            m_RootPanel.Add(m_OptionsPanel);

            m_VisualizationDisabledHelpBox = AddHelpBox(HelpBoxMessageType.Info, Style.NavigationVisualizationDisabledTexts, 200, 10, false);
            var visualizationEnabled = PrefabStageUtility.GetCurrentPrefabStage() == null;
            SetVisualizationEnabled(visualizationEnabled);

            // Surfaces
            var surfacesFoldout = AddFoldout(m_OptionsPanel, Style.SurfacesSectionTexts, 5);
            AddToggle(surfacesFoldout, Style.SurfacesSelectedOnlyTexts, NavMeshVisualizationSettings.showOnlySelectedSurfaces,
                (evt => NavMeshVisualizationSettings.showOnlySelectedSurfaces = evt.newValue));
            AddToggle(surfacesFoldout, Style.SurfacesNavMeshTexts, NavMeshVisualizationSettings.showNavMesh,
                evt => NavMeshVisualizationSettings.showNavMesh = evt.newValue);
            AddToggle(surfacesFoldout, Style.SurfacesHeightMeshTexts, NavMeshVisualizationSettings.showHeightMesh,
                evt => NavMeshVisualizationSettings.showHeightMesh = evt.newValue);

            // Agents
            m_AgentFoldOut = AddFoldout(m_OptionsPanel, Style.AgentsSectionTexts, 5);
            AddToggle(m_AgentFoldOut, Style.AgentsPathPolysTexts, NavMeshVisualizationSettings.showAgentPath, evt => NavMeshVisualizationSettings.showAgentPath = evt.newValue);
            AddToggle(m_AgentFoldOut, Style.AgentsPathNodesTexts, NavMeshVisualizationSettings.showAgentPathInfo,
                evt => NavMeshVisualizationSettings.showAgentPathInfo = evt.newValue);
            AddToggle(m_AgentFoldOut, Style.AgentsNeighboursTexts, NavMeshVisualizationSettings.showAgentNeighbours,
                evt => NavMeshVisualizationSettings.showAgentNeighbours = evt.newValue);
            AddToggle(m_AgentFoldOut, Style.AgentsWallsTexts, NavMeshVisualizationSettings.showAgentWalls, evt => NavMeshVisualizationSettings.showAgentWalls = evt.newValue);
            AddToggle(m_AgentFoldOut, Style.AgentsAvoidanceTexts, NavMeshVisualizationSettings.showAgentAvoidance,
                evt => NavMeshVisualizationSettings.showAgentAvoidance = evt.newValue);

            // Create avoidance requests count warning box and display it if needed
            m_AgentCountWarning = AddHelpBox(HelpBoxMessageType.Warning, Style.AgentsAvoidanceDebugRequestsCountExceededTexts, 180, 5, false);
            NavMeshEditorHelpers.GetAgentsDebugInfoRejectedRequestsCount(out var rejected, out var allowed);
            if (rejected > 0)
            {
                DisplayAgentCountWarningBox(rejected, allowed);
            }

            // Create avoidance pending requests warning box and display it if needed
            m_AgentPendingRequestWarning = AddHelpBox(HelpBoxMessageType.Warning, Style.AgentsAvoidancePendingDebugRequestTexts, 180, 5, false);
            if (NavMeshEditorHelpers.HasPendingAgentDebugInfoRequests())
            {
                DisplayAgentPendingRequestWarningBox();
            }

            // Obstacles
            var obstaclesFoldout = AddFoldout(m_OptionsPanel, Style.ObstaclesSectionTexts, 5);
            AddToggle(obstaclesFoldout, Style.ObstaclesCarveHullText, NavMeshVisualizationSettings.showObstacleCarveHull,
                evt => NavMeshVisualizationSettings.showObstacleCarveHull = evt.newValue);

            // Developer Mode only
            if (Unsupported.IsDeveloperMode())
            {
                var developerModeFoldout = AddFoldout(m_OptionsPanel, Style.DeveloperModeSectionTexts, 5);
                AddToggle(developerModeFoldout, Style.SurfacesPortalsTexts, NavMeshVisualizationSettings.showNavMeshPortals,
                    evt => NavMeshVisualizationSettings.showNavMeshPortals = evt.newValue);
                AddToggle(developerModeFoldout, Style.SurfacesTileLinksTexts, NavMeshVisualizationSettings.showNavMeshLinks,
                    evt => NavMeshVisualizationSettings.showNavMeshLinks = evt.newValue);
                AddToggle(developerModeFoldout, Style.SurfacesHeightMeshBVTreeTexts, NavMeshVisualizationSettings.showHeightMeshBVTree,
                    evt => NavMeshVisualizationSettings.showHeightMeshBVTree = evt.newValue);
                AddToggle(developerModeFoldout, Style.SurfacesHeightMapsTexts, NavMeshVisualizationSettings.showHeightMaps,
                    evt => NavMeshVisualizationSettings.showHeightMaps = evt.newValue);
                AddToggle(developerModeFoldout, Style.SurfacesProximityGridTexts, NavMeshVisualizationSettings.showProximityGrid,
                    evt => NavMeshVisualizationSettings.showProximityGrid = evt.newValue);
            }

            return m_RootPanel;
        }

        void OnAgentRejectedDebugInfoRequestsCountChanged(int rejected, int allowed)
        {
            if (rejected == 0)
            {
                HideAgentCountWarningBox();
            }
            else
            {
                DisplayAgentCountWarningBox(rejected, allowed);
            }
        }

        void OnPrefabStageOpened(PrefabStage obj)
        {
            SetVisualizationEnabled(false);
        }

        void OnPrefabStageClosing(PrefabStage obj)
        {
            SetVisualizationEnabled(true);
        }

        static Foldout AddFoldout(VisualElement parent, GUIContent text, int padding)
        {
            var prefName = $"AINavigationOverlay_Foldout_{text.text}";

            var foldout = new Foldout
            {
                text = text.text,
                tooltip = text.tooltip,
                style =
                {
                    paddingBottom = padding
                },
                value = EditorPrefs.GetBool(prefName, true)
            };

            foldout.RegisterValueChangedCallback(evt =>
                EditorPrefs.SetBool(prefName, evt.newValue));

            parent.Add(foldout);

            return foldout;
        }

        static void AddToggle(VisualElement parent, GUIContent text, bool parameter, EventCallback<ChangeEvent<bool>> callback)
        {
            // Create toggle element with the desired text content
            var toggle = new Toggle
            {
                label = text.text,
                value = parameter,
                tooltip = text.tooltip,
                style =
                {
                    marginBottom = 0 // To compact a bit the checkboxes list layout
                }
            };

            // Add padding to guarantee a minimum separation between labels and checkboxes
            toggle.labelElement.style.paddingRight = 20;

            // Look for the checkbox container and make it align the checkbox to the right (so that all checkboxes are justified) 
            foreach (VisualElement child in toggle.Children())
            {
                if (child != toggle.labelElement)
                {
                    child.style.justifyContent = Justify.FlexEnd;
                    break;
                }
            }

            toggle.RegisterCallback(callback);

            parent.Add(toggle);
        }

        static HelpBox AddHelpBox(HelpBoxMessageType messageType, GUIContent text, int maxWidth, int verticalMargin, bool visible)
        {
            var helpBox = new HelpBox(text.text, messageType)
            {
                tooltip = text.tooltip,
                style =
                {
                    marginBottom = verticalMargin,
                    marginTop = verticalMargin,
                    maxWidth = maxWidth,
                    alignSelf = Align.Center,
                },
                visible = visible
            };

            return helpBox;
        }

        void DisplayAgentCountWarningBox(int rejected, int allowed)
        {
            m_AgentCountWarning.text = $"Avoidance visualization can be drawn for {allowed} agents ({allowed + rejected} selected).";
            if (!m_AgentCountWarning.visible)
            {
                m_AgentCountWarning.visible = true;
                m_AgentFoldOut.Add(m_AgentCountWarning);
            }
        }

        void HideAgentCountWarningBox()
        {
            if (m_AgentCountWarning.visible)
            {
                m_AgentCountWarning.visible = false;
                m_AgentFoldOut.Remove(m_AgentCountWarning);
            }
        }

        void DisplayAgentPendingRequestWarningBox()
        {
            if (!m_AgentPendingRequestWarning.visible)
            {
                m_AgentPendingRequestWarning.visible = true;
                m_AgentFoldOut.Add(m_AgentPendingRequestWarning);
            }
        }

        void HideAgentPendingRequestWarningBox()
        {
            if (m_AgentPendingRequestWarning.visible)
            {
                m_AgentPendingRequestWarning.visible = false;
                m_AgentFoldOut.Remove(m_AgentPendingRequestWarning);
            }
        }

        void SetVisualizationEnabled(bool enabled)
        {
            if (m_VisualizationDisabledHelpBox == null)
                return;
            
            if (enabled)
            {
                if (m_VisualizationDisabledHelpBox.visible)
                {
                    m_RootPanel.Remove(m_VisualizationDisabledHelpBox);
                    m_OptionsPanel.SetEnabled(true);
                    m_OptionsPanel.tooltip = "";
                    m_VisualizationDisabledHelpBox.visible = false;
                }
            }
            else
            {
                if (!m_VisualizationDisabledHelpBox.visible)
                {
                    m_RootPanel.Insert(0, m_VisualizationDisabledHelpBox);
                    m_OptionsPanel.SetEnabled(false);
                    m_OptionsPanel.tooltip = Style.NavigationVisualizationDisabledTexts.tooltip;
                    m_VisualizationDisabledHelpBox.visible = true;
                }
            }
        }
    }
}

#endif
