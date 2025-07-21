using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if !UNITY_2021_2_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.AI.Navigation.Editor
{
    /// <summary>
    /// Manages assets and baking operation of the navmesh 
    /// </summary>
    public class NavMeshAssetManager : ScriptableSingleton<NavMeshAssetManager>
    {
        internal struct AsyncBakeOperation
        {
            internal NavMeshSurface surface;
            internal NavMeshData bakeData;
            internal AsyncOperation bakeOperation;
            internal int progressReportId;
        }

        List<AsyncBakeOperation> m_BakeOperations = new List<AsyncBakeOperation>();
        internal List<AsyncBakeOperation> GetBakeOperations() { return m_BakeOperations; }

        struct SavedPrefabNavMeshData
        {
            internal NavMeshSurface surface;
            internal NavMeshData navMeshData;
        }

        List<SavedPrefabNavMeshData> m_PrefabNavMeshDataAssets = new List<SavedPrefabNavMeshData>();

        static string GetAndEnsureTargetPath(NavMeshSurface surface)
        {
            // Create directory for the asset if it does not exist yet.
            var activeScenePath = surface.gameObject.scene.path;

            var targetPath = "Assets";
            if (!string.IsNullOrEmpty(activeScenePath))
            {
                targetPath = Path.Combine(Path.GetDirectoryName(activeScenePath), Path.GetFileNameWithoutExtension(activeScenePath));
            }
            else if (surface.IsPartOfPrefab())
            {
                var prefabStage = PrefabStageUtility.GetPrefabStage(surface.gameObject);
#if UNITY_2020_1_OR_NEWER
                var assetPath = prefabStage.assetPath;
#else
                var assetPath = prefabStage.prefabAssetPath;
#endif
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var prefabDirectoryName = Path.GetDirectoryName(assetPath);
                    if (!string.IsNullOrEmpty(prefabDirectoryName))
                        targetPath = prefabDirectoryName;
                }
            }

            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            return targetPath;
        }

        static void CreateNavMeshAsset(NavMeshSurface surface)
        {
            var targetPath = GetAndEnsureTargetPath(surface);

            var combinedAssetPath = Path.Combine(targetPath, "NavMesh-" + surface.name + ".asset");
            combinedAssetPath = AssetDatabase.GenerateUniqueAssetPath(combinedAssetPath);
            AssetDatabase.CreateAsset(surface.navMeshData, combinedAssetPath);
        }

        NavMeshData GetNavMeshAssetToDelete(NavMeshSurface navSurface)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(navSurface) && !PrefabUtility.IsPartOfModelPrefab(navSurface))
            {
                // Don't allow deleting the asset belonging to the prefab parent
                var parentSurface = PrefabUtility.GetCorrespondingObjectFromSource(navSurface) as NavMeshSurface;
                if (parentSurface && navSurface.navMeshData == parentSurface.navMeshData)
                    return null;
            }

            // Do not delete the NavMeshData asset referenced from a prefab until the prefab is saved
            if (navSurface.IsPartOfPrefab() && IsCurrentPrefabNavMeshDataStored(navSurface))
                return null;

            return navSurface.navMeshData;
        }

        void ClearSurface(NavMeshSurface navSurface)
        {
            var hasNavMeshData = navSurface.navMeshData != null;
            StoreNavMeshDataIfInPrefab(navSurface);

            var assetToDelete = GetNavMeshAssetToDelete(navSurface);
            navSurface.RemoveData();

            if (hasNavMeshData)
            {
                SetNavMeshData(navSurface, null);
                EditorSceneManager.MarkSceneDirty(navSurface.gameObject.scene);
            }

            if (assetToDelete)
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assetToDelete));
        }

        /// <summary>
        /// Start baking a list of NavMeshSurface
        /// </summary>
        /// <param name="surfaces">List of surfaces</param>
        public void StartBakingSurfaces(Object[] surfaces)
        {
            // Remove first to avoid double registration of the callback
            EditorApplication.update -= UpdateAsyncBuildOperations;
            EditorApplication.update += UpdateAsyncBuildOperations;

            foreach (NavMeshSurface surf in surfaces)
            {
                StoreNavMeshDataIfInPrefab(surf);

                var oper = new AsyncBakeOperation();

                oper.bakeData = InitializeBakeData(surf);
                oper.bakeOperation = surf.UpdateNavMesh(oper.bakeData);
                oper.surface = surf;
                oper.progressReportId = Progress.Start(L10n.Tr("Baking a NavMesh"), string.Format(L10n.Tr("Surface held by {0} for agent type {1}"), surf.gameObject.name, NavMesh.GetSettingsNameFromID(surf.agentTypeID)));

                Progress.RegisterCancelCallback(oper.progressReportId, () =>
                {
                    NavMeshBuilder.Cancel(oper.bakeData);
                    return true;
                });
                    
                m_BakeOperations.Add(oper);
            }
        }

        static NavMeshData InitializeBakeData(NavMeshSurface surface)
        {
            var emptySources = new List<NavMeshBuildSource>();
            var emptyBounds = new Bounds();
            return UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(surface.GetBuildSettings(), emptySources, emptyBounds
                , surface.transform.position, surface.transform.rotation);
        }

        void UpdateAsyncBuildOperations()
        {
            foreach (var oper in m_BakeOperations)
            {
                if (oper.surface == null || oper.bakeOperation == null)
                    continue;

                if (oper.bakeOperation.isDone)
                {
                    var surface = oper.surface;
                    var delete = GetNavMeshAssetToDelete(surface);
                    if (delete != null)
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(delete));

                    surface.RemoveData();
                    SetNavMeshData(surface, oper.bakeData);

                    if (surface.isActiveAndEnabled)
                        surface.AddData();
                    CreateNavMeshAsset(surface);

                    if (!EditorApplication.isPlaying || surface.IsPartOfPrefab())
                        EditorSceneManager.MarkSceneDirty(surface.gameObject.scene);

                    Progress.Finish(oper.progressReportId);
                }
                
                Progress.Report(oper.progressReportId, oper.bakeOperation.progress);
            }
            m_BakeOperations.RemoveAll(o => o.bakeOperation == null || o.bakeOperation.isDone);
            if (m_BakeOperations.Count == 0)
                EditorApplication.update -= UpdateAsyncBuildOperations;
        }

        /// <summary>
        /// Checks if an operation of baking is in progress for a specified surface
        /// </summary>
        /// <param name="surface">A navmesh surface</param>
        /// <returns>True if the specified surface is baking</returns>
        public bool IsSurfaceBaking(NavMeshSurface surface)
        {
            if (surface == null)
                return false;

            foreach (var oper in m_BakeOperations)
            {
                if (oper.surface == null || oper.bakeOperation == null)
                    continue;

                if (oper.surface == surface)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Clear navmesh surfaces
        /// </summary>
        /// <param name="surfaces">List of surfaces</param>
        public void ClearSurfaces(Object[] surfaces)
        {
            foreach (NavMeshSurface s in surfaces)
                ClearSurface(s);
        }

        static void SetNavMeshData(NavMeshSurface navSurface, NavMeshData navMeshData)
        {
            var so = new SerializedObject(navSurface);
            var navMeshDataProperty = so.FindProperty("m_NavMeshData");
            navMeshDataProperty.objectReferenceValue = navMeshData;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        void StoreNavMeshDataIfInPrefab(NavMeshSurface surfaceToStore)
        {
            if (!surfaceToStore.IsPartOfPrefab())
                return;

            // check if data has already been stored for this surface
            foreach (var storedAssetInfo in m_PrefabNavMeshDataAssets)
                if (storedAssetInfo.surface == surfaceToStore)
                    return;

            if (m_PrefabNavMeshDataAssets.Count == 0)
            {
                PrefabStage.prefabSaving -= DeleteStoredNavMeshDataAssetsForOwnedSurfaces;
                PrefabStage.prefabSaving += DeleteStoredNavMeshDataAssetsForOwnedSurfaces;

                PrefabStage.prefabStageClosing -= ForgetUnsavedNavMeshDataChanges;
                PrefabStage.prefabStageClosing += ForgetUnsavedNavMeshDataChanges;
            }

            var isDataOwner = true;
            if (PrefabUtility.IsPartOfPrefabInstance(surfaceToStore) && !PrefabUtility.IsPartOfModelPrefab(surfaceToStore))
            {
                var basePrefabSurface = PrefabUtility.GetCorrespondingObjectFromSource(surfaceToStore) as NavMeshSurface;
                isDataOwner = basePrefabSurface == null || surfaceToStore.navMeshData != basePrefabSurface.navMeshData;
            }
            m_PrefabNavMeshDataAssets.Add(new SavedPrefabNavMeshData { surface = surfaceToStore, navMeshData = isDataOwner ? surfaceToStore.navMeshData : null });
        }

        bool IsCurrentPrefabNavMeshDataStored(NavMeshSurface surface)
        {
            if (surface == null)
                return false;

            foreach (var storedAssetInfo in m_PrefabNavMeshDataAssets)
            {
                if (storedAssetInfo.surface == surface)
                    return storedAssetInfo.navMeshData == surface.navMeshData;
            }

            return false;
        }

        void DeleteStoredNavMeshDataAssetsForOwnedSurfaces(GameObject gameObjectInPrefab)
        {
            // Debug.LogFormat("DeleteStoredNavMeshDataAsset() when saving prefab {0}", gameObjectInPrefab.name);

            var surfaces = gameObjectInPrefab.GetComponentsInChildren<NavMeshSurface>(true);
            foreach (var surface in surfaces)
                DeleteStoredPrefabNavMeshDataAsset(surface);
        }

        void DeleteStoredPrefabNavMeshDataAsset(NavMeshSurface surface)
        {
            for (var i = m_PrefabNavMeshDataAssets.Count - 1; i >= 0; i--)
            {
                var storedAssetInfo = m_PrefabNavMeshDataAssets[i];
                if (storedAssetInfo.surface == surface)
                {
                    var storedNavMeshData = storedAssetInfo.navMeshData;
                    if (storedNavMeshData != null && storedNavMeshData != surface.navMeshData)
                    {
                        if (!EditorApplication.isPlaying)
                        {
                            var assetPath = AssetDatabase.GetAssetPath(storedNavMeshData);
                            AssetDatabase.DeleteAsset(assetPath);
                        }
                        else
                        {
                            Debug.LogWarning(
                                $"The asset of the previous NavMesh data ({storedNavMeshData.name}), owned by " +
                                $"the prefab that was saved just now ({storedAssetInfo.surface.gameObject.transform.root.gameObject.name}), " +
                                "cannot be deleted automatically because it might still be used by " +
                                "NavMeshSurface components in the running scenes. You can safely delete " +
                                $"the \"{storedNavMeshData.name}.asset\" file after playmode ends.", storedNavMeshData);
                        }
                    }

                    m_PrefabNavMeshDataAssets.RemoveAt(i);
                    break;
                }
            }

            if (m_PrefabNavMeshDataAssets.Count == 0)
            {
                PrefabStage.prefabSaving -= DeleteStoredNavMeshDataAssetsForOwnedSurfaces;
                PrefabStage.prefabStageClosing -= ForgetUnsavedNavMeshDataChanges;
            }
        }

        void ForgetUnsavedNavMeshDataChanges(PrefabStage prefabStage)
        {
            // Debug.Log("On prefab closing - forget about this object's surfaces and stop caring about prefab saving");

            if (prefabStage == null)
                return;

            var allSurfacesInPrefab = prefabStage.prefabContentsRoot.GetComponentsInChildren<NavMeshSurface>(true);
            NavMeshSurface surfaceInPrefab = null;
            var index = 0;
            do
            {
                if (allSurfacesInPrefab.Length > 0)
                    surfaceInPrefab = allSurfacesInPrefab[index];

                for (var i = m_PrefabNavMeshDataAssets.Count - 1; i >= 0; i--)
                {
                    var storedPrefabInfo = m_PrefabNavMeshDataAssets[i];
                    if (storedPrefabInfo.surface == null)
                    {
                        // Debug.LogFormat("A surface from the prefab got deleted after it has baked a new NavMesh but it hasn't saved it. Now the unsaved asset gets deleted. ({0})", storedPrefabInfo.navMeshData);

                        // surface got deleted, thus delete its initial NavMeshData asset
                        if (storedPrefabInfo.navMeshData != null)
                        {
                            var assetPath = AssetDatabase.GetAssetPath(storedPrefabInfo.navMeshData);
                            AssetDatabase.DeleteAsset(assetPath);
                        }

                        m_PrefabNavMeshDataAssets.RemoveAt(i);
                    }
                    else if (surfaceInPrefab != null && storedPrefabInfo.surface == surfaceInPrefab)
                    {
                        //Debug.LogFormat("The surface {0} from the prefab was storing the original navmesh data and now will be forgotten", surfaceInPrefab);

                        var baseSurface = PrefabUtility.GetCorrespondingObjectFromSource(surfaceInPrefab) as NavMeshSurface;
                        if (baseSurface == null || surfaceInPrefab.navMeshData != baseSurface.navMeshData)
                        {
                            var assetPath = AssetDatabase.GetAssetPath(surfaceInPrefab.navMeshData);
                            AssetDatabase.DeleteAsset(assetPath);

                            //Debug.LogFormat("The surface {0} from the prefab has baked new NavMeshData but did not save this change so the asset has been now deleted. ({1})",
                            //    surfaceInPrefab, assetPath);
                        }

                        m_PrefabNavMeshDataAssets.RemoveAt(i);
                    }
                }
            } while (++index < allSurfacesInPrefab.Length);

            if (m_PrefabNavMeshDataAssets.Count == 0)
            {
                PrefabStage.prefabSaving -= DeleteStoredNavMeshDataAssetsForOwnedSurfaces;
                PrefabStage.prefabStageClosing -= ForgetUnsavedNavMeshDataChanges;
            }
        }
    }
}
