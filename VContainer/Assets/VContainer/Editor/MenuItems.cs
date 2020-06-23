using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace VContainer.Editor
{
    public static class MenuItems
    {
        [MenuItem("GameObject/VContainer/Lifetime Scope", false, 9)]
        public static void CreateGameObjectContext(MenuCommand menuCommand)
        {
            var lifetimeScope = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();
            Selection.activeGameObject = lifetimeScope.gameObject;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        [MenuItem("Edit/Create Project root LifetimeScope")]
        public static void CreateProjectLifetimeScopeInDefaultLocation()
        {
            var fullDirPath = Path.Combine(Application.dataPath, "Resources");

            if (!Directory.Exists(fullDirPath))
            {
                Directory.CreateDirectory(fullDirPath);
            }

            CreateProjectLifetimeScope("Assets/Resources");
        }


        [MenuItem("Assets/Create/VContainer/Project root LifetimeScope", true)]
        public static bool ValidateProjectLifetimeScope()
        {
            var selectedPath = Selection.objects.Select(AssetDatabase.GetAssetPath).FirstOrDefault();
            var selectedDir = default(string);

            if (selectedPath != null)
            {
                if (Directory.Exists(selectedPath))
                {
                    selectedDir = selectedPath;
                }
                else
                {
                    selectedDir = Path.GetDirectoryName(selectedPath);
                }
            }

            if (selectedDir == null)
            {
                return false;
            }

            var basename = Path.GetFileName(selectedDir);
            return basename == "Resources";
        }


        [MenuItem("Assets/Create/VContainer/Project root LifetimeScope", false, 40)]
        public static void CreateProjectLifetimeScope()
        {
            var selectedPath = Selection.objects.Select(AssetDatabase.GetAssetPath).FirstOrDefault();

            var selectedDir = default(string);

            if (selectedPath != null)
            {
                if (Directory.Exists(selectedPath))
                {
                    selectedDir = selectedPath;
                }
                else
                {
                    selectedDir = Path.GetDirectoryName(selectedPath);
                }
            }

            if (selectedDir == null)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    $"Could not find directory to place the '{LifetimeScope.ProjectRootResourcePath}.prefab' asset.  Please try again by right clicking in the desired folder within the projects pane.",
                    "OK");
                return;
            }

            var basename = Path.GetFileName(selectedDir);
            if (basename != "Resources")
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    $"'{LifetimeScope.ProjectRootResourcePath}.prefab' must be placed inside a directory named 'Resources'.  Please try again by right clicking within the Project pane in a valid Resources folder.",
                    "Ok");
                return;
            }
            CreateProjectLifetimeScope(selectedDir);
        }

        static void CreateProjectLifetimeScope(string dir)
        {
            var prefabPath = (Path.Combine(dir, LifetimeScope.ProjectRootResourcePath) + ".prefab")
                .Replace("\\", "/");

            var gameObject = new GameObject();

            try
            {
                gameObject.AddComponent<LifetimeScope>();

#if UNITY_2018_3_OR_NEWER
                var prefabObj = PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
#else
                var prefabObj = PrefabUtility.ReplacePrefab(gameObject, PrefabUtility.CreateEmptyPrefab(prefabPath));
#endif
                Selection.activeObject = prefabObj;
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }

            Debug.Log($"Created new LifetimeScope of project root at '{prefabPath}'");
        }
    }
}