using System.Linq;
using UnityEngine;

namespace VContainer.Unity
{
    public sealed class VContainerSettings : ScriptableObject
    {
        public static VContainerSettings Instance { get; private set; }

        [SerializeField]
        public LifetimeScope RootLifetimeScope;

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
            // For editor, we need to load the Preload asset manually.
            LoadInstanceFromPreloadAssets();
        }

        [UnityEditor.InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            // RootLifetimeScope must be disposed before it can be resumed.
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                switch (state)
                {
                    case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                        if (Instance != null)
                        {
                            if (Instance.RootLifetimeScope != null)
                            {
                                Instance.RootLifetimeScope.DisposeCore();
                            }
                            Instance = null;
                        }
                        break;
                }
            };
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
