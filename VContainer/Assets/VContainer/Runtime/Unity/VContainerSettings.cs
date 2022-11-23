using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VContainer.Unity
{
    public sealed class VContainerSettings : ScriptableObject
    {
        public static VContainerSettings Instance { get; private set; }

        public static bool DiagnosticsEnabled => Instance != null && Instance.EnableDiagnostics;

        [SerializeField]
        [Tooltip("Set the Prefab to be the parent of the entire Project.")]
        public LifetimeScope RootLifetimeScope;

        [SerializeField]
        [Tooltip("Enables the collection of information that can be viewed in the VContainerDiagnosticsWindow. Note: Performance degradation")]
        public bool EnableDiagnostics;

        [SerializeField]
        [Tooltip("Disables script modification for LifetimeScope scripts.")]
        public bool DisableScriptModifier;

        [SerializeField]
        [Tooltip("Removes (Clone) postfix in IObjectResolver.Instantiate() and IContainerBuilder.RegisterComponentInNewPrefab().")]
        public bool RemoveClonePostfix;

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
                if (instance.RootLifetimeScope != null)
                    instance.RootLifetimeScope.DisposeCore();
                instance.OnEnable();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RuntimeInitialize()
        {
            // For editor, we need to load the Preload asset manually.
            LoadInstanceFromPreloadAssets();
        }
#endif

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                if (RootLifetimeScope != null)
                {
                    RootLifetimeScope.IsRoot = true;
                }

                Instance = this;

                var activeScene = SceneManager.GetActiveScene();
                if (activeScene.isLoaded)
                {
                    OnFirstSceneLoaded(activeScene, default);
                }
                else
                {
                    SceneManager.sceneLoaded -= OnFirstSceneLoaded;
                    SceneManager.sceneLoaded += OnFirstSceneLoaded;
                }

                Application.quitting -= OnApplicationQuit;
                Application.quitting += OnApplicationQuit;
            }
        }

        void OnFirstSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (RootLifetimeScope != null &&
                RootLifetimeScope.Container == null &&
                RootLifetimeScope.autoRun)
            {
                RootLifetimeScope.Build();
            }
            SceneManager.sceneLoaded -= OnFirstSceneLoaded;
        }

        void OnApplicationQuit()
        {
            if (RootLifetimeScope != null)
            {
                if (RootLifetimeScope.Container != null)
                {
                    // Execute Dispose once at the slowest possible time.
                    // However, the GameObject may be destroyed at that time.
                    PlayerLoopHelper.Dispatch(PlayerLoopTiming.LateUpdate, new AsyncLoopItem(() =>
                    {
                        RootLifetimeScope.DisposeCore();
                    }));
                }
            }
        }
    }
}
