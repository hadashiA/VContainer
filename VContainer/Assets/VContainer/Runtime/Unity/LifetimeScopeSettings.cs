using System.Linq;
using UnityEngine;

namespace VContainer.Unity
{
    public sealed class LifetimeScopeSettings : ScriptableObject
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (Instance != null && Instance.rootLifetimeScope == null)
            {
                Instance.rootLifetimeScope.Build();
            }
        }

        public static LifetimeScopeSettings Instance;

        [SerializeField]
        public LifetimeScope rootLifetimeScope;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/VContainer/LifetimeScopeSettings")]
        public static void CreateAsset()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                "Save LifetimeScopeSettings",
                "LifetimeScopeSettings",
                "asset",
                string.Empty);

            if (string.IsNullOrEmpty(path))
                return;

            var configObject = CreateInstance<LifetimeScopeSettings>();
            UnityEditor.AssetDatabase.CreateAsset(configObject, path);

            // Add the config asset to the build
            var preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.Add(configObject);
            UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
        }
#endif

        void OnEnable()
        {
            if (rootLifetimeScope != null)
                rootLifetimeScope.IsRoot = true;
            Instance = this;
        }
    }
}