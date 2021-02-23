using System;
using System.Linq;
using UnityEngine;

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

            var newSettings = CreateInstance<VContainerSettings>();
            UnityEditor.AssetDatabase.CreateAsset(newSettings, path);

            var preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.RemoveAll(x => x is VContainerSettings);
            preloadedAssets.Add(newSettings);
            UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
        }

        public static void LoadInstanceFromPreloadAssets()
        {
            var preloadAsset = UnityEditor.PlayerSettings.GetPreloadedAssets().FirstOrDefault(x => x is VContainerSettings);
            if (preloadAsset is VContainerSettings instance)
            {
                instance.OnEnable();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RuntimeInitialize()
        {
            LoadInstanceFromPreloadAssets();
        }

        [UnityEditor.InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            LoadInstanceFromPreloadAssets();
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
