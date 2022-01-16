using UnityEditor;
using UnityEngine;

namespace VContainer.Editor
{
#if UNITY_2020_1_OR_NEWER
    // UnityEditor.FilePathAttribute has been internal class until Unity 2020.1.
    [FilePath(VContainerEditorSettings.projectSettingPath, FilePathAttribute.Location.ProjectFolder)]
#endif
    public class VContainerEditorSettings : ScriptableSingleton<VContainerEditorSettings>
    {
        const string projectSettingPath = "ProjectSettings/VContainerEditorSettings.asset";
        const string settingsProviderPath = "Project/VContainer";
        const string settingsProviderLabel = "VContainer";

        [SerializeField]
        bool enableScriptModifier = true;

        public bool EnableScriptModifier
        {
            get => enableScriptModifier;
            set => enableScriptModifier = value;
        }

        public void Save()
        {
#if UNITY_2020_1_OR_NEWER
            Save(true); // This method uses FilePathAttribute internally, so this works for above Unity 2020.1.
#else
            UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new[] { this }, projectSettingPath, true);
#endif
        }

        [SettingsProvider]
        static SettingsProvider CreateProjectSettingsProvider()
        {
            return new SettingsProvider(settingsProviderPath, SettingsScope.Project)
            {
                label = settingsProviderLabel,
                guiHandler = OnGUI
            };
        }

        static void OnGUI(string searchContext)
        {
            EditorGUIUtility.labelWidth = 200;
            using (var s = new EditorGUI.ChangeCheckScope())
            {
                instance.EnableScriptModifier = EditorGUILayout.Toggle("Enable Script Modifier", instance.EnableScriptModifier);
                if (s.changed)
                {
                    instance.Save();
                }
            }
        }
    }
}
