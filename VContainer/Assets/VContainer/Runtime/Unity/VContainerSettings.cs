using System;
using System.Linq;
using UnityEngine;
using VContainer.Unity;

namespace VContainer.Unity
{
    [Serializable]
    public sealed class CodeGenSettings
    {
        public bool Enabled;
        public string[] AssemblyNames;
        public string[] Namespaces;
    }

    public sealed class VContainerSettings : ScriptableObject
    {
        [SerializeField]
        public LifetimeScope RootLifetimeScope;

        [SerializeField]
        public CodeGenSettings CodeGen;

        public static VContainerSettings Instance { get; private set; }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/VContainer/VContainer Settings")]
        public static void CreateAsset()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                "Save VContainerSettings",
                "VContainerSettings",
                "asset",
                string.Empty);

            if (string.IsNullOrEmpty(path))
                return;

            var configObject = CreateInstance<VContainerSettings>();
            UnityEditor.AssetDatabase.CreateAsset(configObject, path);

            // Add the config asset to the build
            var preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.Add(configObject);
            UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
        }

        [UnityEditor.InitializeOnLoadMethod]
        public static void LoadInstanceFromAssetDatabase()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:VContainerSettings");
            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                Instance = UnityEditor.AssetDatabase.LoadAssetAtPath<VContainerSettings>(path);
            }
        }
#endif

        void OnEnable()
        {
            if (RootLifetimeScope != null)
                RootLifetimeScope.IsRoot = true;
            Instance = this;
        }
    }
}