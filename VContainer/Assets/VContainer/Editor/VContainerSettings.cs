using System;
using System.Linq;
using UnityEngine;

namespace VContainer.Editor
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
#endif

        void OnEnable()
        {
            Instance = this;
        }
    }
}