using System.IO;
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

        //     [MenuItem("Edit//Create Project Context")]
        //     public static void CreateProjectContextInDefaultLocation()
        //     {
        //         var fullDirPath = Path.Combine(Application.dataPath, "Resources");
        //
        //         if (!Directory.Exists(fullDirPath))
        //         {
        //             Directory.CreateDirectory(fullDirPath);
        //         }
        //
        //         CreateProjectContextInternal("Assets/Resources");
        //     }
        // }
    }
}