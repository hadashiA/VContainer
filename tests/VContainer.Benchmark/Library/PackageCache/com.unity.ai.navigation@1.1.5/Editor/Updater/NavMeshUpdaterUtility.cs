#if UNITY_2022_2_OR_NEWER
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Unity.AI.Navigation.Updater
{
    internal static class NavMeshUpdaterUtility
    {
        const string k_NavMeshSettingsPropertyPath = "NavMeshSettings";
        const string k_NavMeshDataPropertyPath = "m_NavMeshData";
        const string k_EmptyFileIdSerialization = "{fileID: 0}";

        const string k_DefaultNavMeshSurfaceName = "Navigation";

        const int k_WalkableAreaId = 0;

        public static bool ConvertScene(string path)
        {
            Scene previousActiveScene = SceneManager.GetActiveScene();

            OpenAndSetActiveScene(path, out Scene convertedScene, out bool alreadyOpened);

            // Retrieve the legacy NavMesh data from the active scene
            var settingObject = new SerializedObject(UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject);
            var navMeshDataProperty = settingObject.FindProperty(k_NavMeshDataPropertyPath);
            var navMeshData = navMeshDataProperty.objectReferenceValue as NavMeshData;

            if (navMeshData == null)
            {
                Debug.LogWarning("The NavMesh asset referenced in the scene is missing or corrupted.");
                return false;
            }

            // Convert static Navigation Flags with a NavMeshModifier
            GameObject[] rootObjects = convertedScene.GetRootGameObjects();
            var sceneObjects = SceneModeUtility.GetObjects(rootObjects, true);
            foreach (GameObject go in sceneObjects)
                ConvertStaticValues(go);

            // Create a global NavMeshSurface and copy legacy NavMesh settings
            var surfaceObject = new GameObject(k_DefaultNavMeshSurfaceName, typeof(NavMeshSurface));
            var navMeshSurface = surfaceObject.GetComponent<NavMeshSurface>();

            navMeshSurface.navMeshData = navMeshData;

            NavMeshBuildSettings settings = navMeshData.buildSettings;
            navMeshSurface.collectObjects = CollectObjects.MarkedWithModifier;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            navMeshSurface.overrideVoxelSize = settings.overrideVoxelSize;
            navMeshSurface.voxelSize = settings.voxelSize;
            navMeshSurface.overrideTileSize = settings.overrideTileSize;
            navMeshSurface.tileSize = settings.tileSize;
            navMeshSurface.minRegionArea = settings.minRegionArea;
            navMeshSurface.buildHeightMesh = settings.buildHeightMesh;

            // Remove NavMeshData reference from the scene
            navMeshDataProperty.objectReferenceValue = null;
            settingObject.ApplyModifiedProperties();

            // Rename NavMesh asset
            var assetPath = AssetDatabase.GetAssetPath(navMeshData);
            AssetDatabase.RenameAsset(assetPath, "NavMesh-" + navMeshSurface.name + ".asset");
            AssetDatabase.Refresh();

            EditorSceneManager.SaveScene(convertedScene);

            navMeshSurface.AddData();

            if (!alreadyOpened)
                EditorSceneManager.CloseScene(convertedScene, true);

            SceneManager.SetActiveScene(previousActiveScene);

            return true;
        }

        private static void OpenAndSetActiveScene(string path, out Scene scene, out bool alreadyOpened)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                    continue;

                if (path == scene.path)
                {
                    if (SceneManager.GetActiveScene() != scene)
                        SceneManager.SetActiveScene(scene);

                    alreadyOpened = true;
                    return;
                }
            }

            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            alreadyOpened = false;
        }

        private static void ConvertStaticValues(GameObject go)
        {
            // Disable CS0618 warning about StaticEditorFlags.NavigationStatic and StaticEditorFlags.OffMeshLinkGeneration being deprecated
#pragma warning disable 618
            var staticFlags = GameObjectUtility.GetStaticEditorFlags(go);
            if ((staticFlags & StaticEditorFlags.NavigationStatic) != 0)
            {
                NavMeshModifier modifier = go.AddComponent<NavMeshModifier>();
                modifier.area = GameObjectUtility.GetNavMeshArea(go);
                if (modifier.area != k_WalkableAreaId)
                    modifier.overrideArea = true;

                if ((staticFlags & StaticEditorFlags.OffMeshLinkGeneration) != 0)
                {
                    modifier.overrideGenerateLinks = true;
                    modifier.generateLinks = true;
                }

                staticFlags &= ~StaticEditorFlags.NavigationStatic;
                GameObjectUtility.SetStaticEditorFlags(go, staticFlags);
            }
#pragma warning restore 618
        }

        internal static bool IsSceneReferencingLegacyNavMesh(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (path.StartsWith("Packages"))
                return false;

            if (!path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                return false;

            using (StreamReader file = File.OpenText(path))
            {
                string line;
                bool skipUntilSettings = true;
                while ((line = file.ReadLine()) != null)
                {
                    if (skipUntilSettings)
                    {
                        if (line.Contains($"{k_NavMeshSettingsPropertyPath}:"))
                            skipUntilSettings = false;
                    }
                    else
                    {
                        if (line.Contains($"{k_NavMeshDataPropertyPath}:"))
                        {
                            return !line.Contains(k_EmptyFileIdSerialization);
                        }
                    }
                }
            }

            return false;
        }
    }
}
#endif
