#if UNITY_2022_2_OR_NEWER

using UnityEngine;
using UnityEditor.AI;
using UnityEditorInternal;
using EditorNavMeshBuilder = UnityEditor.AI.NavMeshBuilder;
using Object = UnityEngine.Object;
using UnityEditor;

namespace Unity.AI.Navigation.Editor
{
    [InitializeOnLoad]
    [EditorWindowTitle(title = "Navigation")]
    internal class NavigationWindow : EditorWindow
    {
        static NavigationWindow s_NavigationWindow;

        // Scene based bake configuration
        SerializedObject m_SettingsObject;

        // Project based configuration
        SerializedObject m_NavMeshProjectSettingsObject;
        SerializedProperty m_Areas;

        SerializedProperty m_AgentTypes;
        SerializedProperty m_SettingNames;

        Vector2 m_ScrollPos = Vector2.zero;
        bool m_Advanced;

        ReorderableList m_AreasList;
        ReorderableList m_AgentTypeList;

        enum Mode
        {
            AgentTypeSettings = 0,
            AreaSettings = 1,
        }

        Mode m_Mode = Mode.AgentTypeSettings;

        static class Styles
        {
            internal static readonly GUIContent k_AgentTypesHeader = EditorGUIUtility.TrTextContent("Agent Types");
            internal static readonly GUIContent k_NameLabel = EditorGUIUtility.TrTextContent("Name");
            internal static readonly GUIContent k_CostLabel = EditorGUIUtility.TrTextContent("Cost");
            internal static readonly GUIContent[] k_ModeToggles =
            {
                EditorGUIUtility.TrTextContent("Agents", "Navmesh agent settings."),
                EditorGUIUtility.TrTextContent("Areas", "Navmesh area settings."),
            };
            internal static readonly GUIStyle k_ContentMargins = new GUIStyle
            {
                padding = new RectOffset(4, 4, 4, 4)
            };
        };

        static NavigationWindow()
        {
            NavMeshEditorHelpers.areaSettingsClicked += OpenAreaSettings;
            NavMeshEditorHelpers.agentTypeSettingsClicked += OpenAgentSettings;
        }

        [MenuItem("Window/AI/Navigation", false, 1)]
        public static void SetupWindow()
        {
            var window = GetWindow<NavigationWindow>();
            window.minSize = new Vector2(300, 360);
        }

        static void OpenAreaSettings()
        {
            SetupWindow();
            if (s_NavigationWindow == null)
                return;

            s_NavigationWindow.m_Mode = Mode.AreaSettings;
            s_NavigationWindow.InitProjectSettings();
            s_NavigationWindow.InitAgentTypes();
        }

        static void OpenAgentSettings(int agentTypeID)
        {
            SetupWindow();
            if (s_NavigationWindow == null)
                return;

            s_NavigationWindow.m_Mode = Mode.AgentTypeSettings;
            s_NavigationWindow.InitProjectSettings();
            s_NavigationWindow.InitAgentTypes();

            s_NavigationWindow.m_AgentTypeList.index = -1;
            for (int i = 0; i < s_NavigationWindow.m_AgentTypes.arraySize; i++)
            {
                SerializedProperty agentType = s_NavigationWindow.m_AgentTypes.GetArrayElementAtIndex(i);
                SerializedProperty idProp = agentType.FindPropertyRelative("agentTypeID");
                if (idProp.intValue == agentTypeID)
                {
                    s_NavigationWindow.m_AgentTypeList.index = i;
                    break;
                }
            }
        }

        public void OnEnable()
        {
            var iconPath = $"{NavMeshComponentsGUIUtility.k_PackageEditorResourcesFolder}NavigationWindowIcon.png";
            titleContent = EditorGUIUtility.TrTextContentWithIcon("Navigation", iconPath);
            s_NavigationWindow = this;
            EditorApplication.searchChanged += Repaint;

            Repaint();
        }

        void InitProjectSettings()
        {
            if (m_NavMeshProjectSettingsObject == null)
            {
                Object obj = Unsupported.GetSerializedAssetInterfaceSingleton("NavMeshProjectSettings");
                m_NavMeshProjectSettingsObject = new SerializedObject(obj);
            }
        }

        void InitAreas()
        {
            if (m_Areas == null)
            {
                m_Areas = m_NavMeshProjectSettingsObject.FindProperty("areas");
            }
            if (m_AreasList == null)
            {
                m_AreasList = new ReorderableList(m_NavMeshProjectSettingsObject, m_Areas, false, true, false, false);
                m_AreasList.drawElementCallback = DrawAreaListElement;
                m_AreasList.drawHeaderCallback = DrawAreaListHeader;
                m_AreasList.elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        void InitAgentTypes()
        {
            if (m_AgentTypes == null)
            {
                m_AgentTypes = m_NavMeshProjectSettingsObject.FindProperty("m_Settings");
                m_SettingNames = m_NavMeshProjectSettingsObject.FindProperty("m_SettingNames");
            }
            if (m_AgentTypeList == null)
            {
                m_AgentTypeList = new ReorderableList(m_NavMeshProjectSettingsObject, m_AgentTypes, false, true, true, true);
                m_AgentTypeList.drawElementCallback = DrawAgentTypeListElement;
                m_AgentTypeList.drawHeaderCallback = DrawAgentTypeListHeader;
                m_AgentTypeList.onAddCallback = AddAgentType;
                m_AgentTypeList.onRemoveCallback = RemoveAgentType;
                m_AgentTypeList.elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        // This code is replicated from the navmesh debug draw code.
        int Bit(int a, int b)
        {
            return (a & (1 << b)) >> b;
        }

        Color GetAreaColor(int i)
        {
            if (i == 0)
                return new Color(0, 0.75f, 1.0f, 0.5f);
            int r = (Bit(i, 4) + Bit(i, 1) * 2 + 1) * 63;
            int g = (Bit(i, 3) + Bit(i, 2) * 2 + 1) * 63;
            int b = (Bit(i, 5) + Bit(i, 0) * 2 + 1) * 63;
            return new Color(r / 255.0f, g / 255.0f, b / 255.0f, 0.5f);
        }

        public void OnDisable()
        {
            s_NavigationWindow = null;
            EditorApplication.searchChanged -= Repaint;
        }


        void OnSelectionChange()
        {
            m_ScrollPos = Vector2.zero;
        }

        void ModeToggle()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            m_Mode = (Mode)GUILayout.Toolbar((int)m_Mode, Styles.k_ModeToggles, "LargeButton", GUI.ToolbarButtonSize.FitToContents);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        static void GetAreaListRects(Rect rect, out Rect stripeRect, out Rect labelRect, out Rect nameRect, out Rect costRect)
        {
            float stripeWidth = EditorGUIUtility.singleLineHeight * 0.8f;
            float labelWidth = EditorGUIUtility.singleLineHeight * 5;
            float costWidth = EditorGUIUtility.singleLineHeight * 4;
            float nameWidth = rect.width - stripeWidth - labelWidth - costWidth;
            float x = rect.x;
            stripeRect = new Rect(x, rect.y, stripeWidth - 4, rect.height);
            x += stripeWidth;
            labelRect = new Rect(x, rect.y, labelWidth - 4, rect.height);
            x += labelWidth;
            nameRect = new Rect(x, rect.y, nameWidth - 4, rect.height);
            x += nameWidth;
            costRect = new Rect(x, rect.y, costWidth, rect.height);
        }

        void DrawAreaListHeader(Rect rect)
        {
            GetAreaListRects(rect, out _, out _, out Rect nameRect, out Rect costRect);
            GUI.Label(nameRect, Styles.k_NameLabel);
            GUI.Label(costRect, Styles.k_CostLabel);
        }

        void DrawAreaListElement(Rect rect, int index, bool selected, bool focused)
        {
            SerializedProperty areaProp = m_Areas.GetArrayElementAtIndex(index);
            if (areaProp == null)
                return;
            SerializedProperty nameProp = areaProp.FindPropertyRelative("name");
            SerializedProperty costProp = areaProp.FindPropertyRelative("cost");
            if (nameProp == null || costProp == null)
                return;

            rect.height -= 2; // nicer looking with selected list row and a text field in it

            bool builtInLayer;
            bool allowChangeName;
            bool allowChangeCost;
            switch (index)
            {
                case 0: // Default
                    builtInLayer = true;
                    allowChangeName = false;
                    allowChangeCost = true;
                    break;
                case 1: // NonWalkable
                    builtInLayer = true;
                    allowChangeName = false;
                    allowChangeCost = false;
                    break;
                case 2: // Jump
                    builtInLayer = true;
                    allowChangeName = false;
                    allowChangeCost = true;
                    break;
                default:
                    builtInLayer = false;
                    allowChangeName = true;
                    allowChangeCost = true;
                    break;
            }
            
            GetAreaListRects(rect, out Rect stripeRect, out Rect labelRect, out Rect nameRect, out Rect costRect);

            bool oldEnabled = GUI.enabled;
            Color color = GetAreaColor(index);
            Color dimmed = new Color(color.r * 0.1f, color.g * 0.1f, color.b * 0.1f, 0.6f);
            EditorGUI.DrawRect(stripeRect, color);

            EditorGUI.DrawRect(new Rect(stripeRect.x, stripeRect.y, 1, stripeRect.height), dimmed);
            EditorGUI.DrawRect(new Rect(stripeRect.x + stripeRect.width - 1, stripeRect.y, 1, stripeRect.height), dimmed);
            EditorGUI.DrawRect(new Rect(stripeRect.x + 1, stripeRect.y, stripeRect.width - 2, 1), dimmed);
            EditorGUI.DrawRect(new Rect(stripeRect.x + 1, stripeRect.y + stripeRect.height - 1, stripeRect.width - 2, 1), dimmed);

            if (builtInLayer)
                GUI.Label(labelRect, EditorGUIUtility.TrTempContent("Built-in " + index));
            else
                GUI.Label(labelRect, EditorGUIUtility.TrTempContent("User " + index));

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            GUI.enabled = oldEnabled && allowChangeName;
            EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);

            GUI.enabled = oldEnabled && allowChangeCost;
            EditorGUI.PropertyField(costRect, costProp, GUIContent.none);

            GUI.enabled = oldEnabled;

            EditorGUI.indentLevel = oldIndent;
        }

        static void AddAgentType(ReorderableList list)
        {
            UnityEngine.AI.NavMesh.CreateSettings();
            list.index = UnityEngine.AI.NavMesh.GetSettingsCount() - 1;
        }

        void RemoveAgentType(ReorderableList list)
        {
            SerializedProperty agentTypeProp = m_AgentTypes.GetArrayElementAtIndex(list.index);
            if (agentTypeProp == null)
                return;
            SerializedProperty idProp = agentTypeProp.FindPropertyRelative("agentTypeID");
            if (idProp == null)
                return;
            // Cannot delete default.
            if (idProp.intValue == 0)
                return;

            m_SettingNames.DeleteArrayElementAtIndex(list.index);
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }

        static void DrawAgentTypeListHeader(Rect rect)
        {
            GUI.Label(rect, Styles.k_AgentTypesHeader);
        }

        void DrawAgentTypeListElement(Rect rect, int index, bool selected, bool focused)
        {
            SerializedProperty agentProp = m_AgentTypes.GetArrayElementAtIndex(index);
            if (agentProp == null)
                return;
            SerializedProperty idProp = agentProp.FindPropertyRelative("agentTypeID");
            if (idProp == null)
                return;

            rect.height -= 2; // nicer looking with selected list row and a text field in it

            bool isDefault = idProp.intValue == 0;
            using (new EditorGUI.DisabledScope(isDefault))
            {
                var settingsName = UnityEngine.AI.NavMesh.GetSettingsNameFromID(idProp.intValue);
                GUI.Label(rect, EditorGUIUtility.TrTempContent(settingsName));
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();
            ModeToggle();
            EditorGUILayout.Space();

            InitProjectSettings();

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
            switch (m_Mode)
            {
                case Mode.AreaSettings:
                    AreaSettings();
                    break;
                case Mode.AgentTypeSettings:
                    AgentTypeSettings();
                    break;
            }
            EditorGUILayout.EndScrollView();
        }

        void AreaSettings()
        {
            if (m_Areas == null)
                InitAreas();
            m_NavMeshProjectSettingsObject.Update();

            using (new GUILayout.VerticalScope(Styles.k_ContentMargins))
            {
                m_AreasList.DoLayoutList();
            }

            m_NavMeshProjectSettingsObject.ApplyModifiedProperties();
        }

        void AgentTypeSettings()
        {
            if (m_AgentTypes == null)
                InitAgentTypes();
            m_NavMeshProjectSettingsObject.Update();

            if (m_AgentTypeList.index < 0)
                m_AgentTypeList.index = 0;

            using (new GUILayout.VerticalScope(Styles.k_ContentMargins))
            {
                m_AgentTypeList.DoLayoutList();
            }

            if (m_AgentTypeList.index >= 0 && m_AgentTypeList.index < m_AgentTypes.arraySize)
            {
                SerializedProperty nameProp = m_SettingNames.GetArrayElementAtIndex(m_AgentTypeList.index);
                SerializedProperty selectedAgentType = m_AgentTypes.GetArrayElementAtIndex(m_AgentTypeList.index);

                SerializedProperty radiusProp = selectedAgentType.FindPropertyRelative("agentRadius");
                SerializedProperty heightProp = selectedAgentType.FindPropertyRelative("agentHeight");
                SerializedProperty stepHeightProp = selectedAgentType.FindPropertyRelative("agentClimb");
                SerializedProperty maxSlopeProp = selectedAgentType.FindPropertyRelative("agentSlope");
                SerializedProperty ledgeDropHeightProp = selectedAgentType.FindPropertyRelative("ledgeDropHeight");
                SerializedProperty jumpDistanceProp = selectedAgentType.FindPropertyRelative("maxJumpAcrossDistance");

                const float kDiagramHeight = 120.0f;
                Rect agentDiagramRect = EditorGUILayout.GetControlRect(false, kDiagramHeight);
                NavMeshEditorHelpers.DrawAgentDiagram(agentDiagramRect, radiusProp.floatValue, heightProp.floatValue, stepHeightProp.floatValue, maxSlopeProp.floatValue);

                EditorGUILayout.PropertyField(nameProp, EditorGUIUtility.TrTempContent("Name"));
                EditorGUILayout.PropertyField(radiusProp, EditorGUIUtility.TrTempContent("Radius"));
                EditorGUILayout.PropertyField(heightProp, EditorGUIUtility.TrTempContent("Height"));
                EditorGUILayout.PropertyField(stepHeightProp, EditorGUIUtility.TrTempContent("Step Height"));

                const float kMaxSlopeAngle = 60.0f;
                EditorGUILayout.Slider(maxSlopeProp, 0.0f, kMaxSlopeAngle, EditorGUIUtility.TrTextContent("Max Slope"));

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(EditorGUIUtility.TrTempContent("Generated Links"), EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(ledgeDropHeightProp, EditorGUIUtility.TrTempContent("Drop Height"));
                EditorGUILayout.PropertyField(jumpDistanceProp, EditorGUIUtility.TrTempContent("Jump Distance"));
            }

            EditorGUILayout.Space();

            m_NavMeshProjectSettingsObject.ApplyModifiedProperties();
        }
    }
}

#endif
