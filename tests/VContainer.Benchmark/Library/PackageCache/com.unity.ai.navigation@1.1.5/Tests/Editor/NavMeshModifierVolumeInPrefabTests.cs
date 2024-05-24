//#define KEEP_ARTIFACTS_FOR_INSPECTION

using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
#if !UNITY_2021_2_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Unity.AI.Navigation.Editor.Tests
{
    [Category("PrefabsWithNavMeshModifierVolume")]
    class NavMeshModifierVolumeInPrefabTests
    {
        const string k_AutoSaveKey = "AutoSave";
        const string k_ParentFolder = "Assets";
        const string k_TempFolderName = "TempPrefabAndModifiers";
        static readonly string k_TempFolder = Path.Combine(k_ParentFolder, k_TempFolderName);

        const int k_PinkArea = 3;
        const int k_GreenArea = 4;
        const int k_RedArea = 18;

        const int k_PrefabDefaultArea = k_GreenArea;

        static readonly NavMeshQueryFilter k_QueryAnyArea = new() { agentTypeID = 0, areaMask = NavMesh.AllAreas };

        static bool s_EnterPlayModeOptionsEnabled;
        static EnterPlayModeOptions s_EnterPlayModeOptions;

        [SerializeField]
        string m_PrefabPath;
        [SerializeField]
        string m_PreviousScenePath;
        [SerializeField]
        string m_TempScenePath;
        [SerializeField]
        int m_TestCounter;
        [SerializeField]
        GameObject m_SurfaceInstance;
        [SerializeField]
        GameObject m_ModVolInstance;

#if KEEP_ARTIFACTS_FOR_INSPECTION
        const bool k_KeepSceneObjects = true;
#else
        const bool k_KeepSceneObjects = false;
#endif
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (EditorApplication.isPlaying)
                return;

            AssetDatabase.DeleteAsset(k_TempFolder);
            var folderGUID = AssetDatabase.CreateFolder(k_ParentFolder, k_TempFolderName);
            Assume.That(folderGUID, Is.Not.Empty);

            SessionState.SetBool(k_AutoSaveKey, PrefabStageAutoSavingUtil.GetPrefabStageAutoSave());
            PrefabStageAutoSavingUtil.SetPrefabStageAutoSave(false);
            StageUtility.GoToMainStage();

            m_PreviousScenePath = SceneManager.GetActiveScene().path;
            m_TempScenePath = Path.Combine(k_TempFolder, "NavMeshModifierVolumePrefabTestsScene.unity");
            var tempScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(tempScene, m_TempScenePath);
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(m_TempScenePath);

            s_EnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            s_EnterPlayModeOptions = EditorSettings.enterPlayModeOptions;

            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (EditorApplication.isPlaying)
                return;

            PrefabStageAutoSavingUtil.SetPrefabStageAutoSave(SessionState.GetBool(k_AutoSaveKey, PrefabStageAutoSavingUtil.GetPrefabStageAutoSave()));
            StageUtility.GoToMainStage();

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            if (string.IsNullOrEmpty(m_PreviousScenePath))
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            EditorSettings.enterPlayModeOptionsEnabled = s_EnterPlayModeOptionsEnabled;
            EditorSettings.enterPlayModeOptions = s_EnterPlayModeOptions;

#if !KEEP_ARTIFACTS_FOR_INSPECTION
            AssetDatabase.DeleteAsset(k_TempFolder);
#endif
        }

        [SetUp]
        public void SetupNewPrefabWithEmptyNavMesh()
        {
            if (EditorApplication.isPlaying)
                return;

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "SurfaceSeekingModVol" + ++m_TestCounter + "Prefab";
            var surface = plane.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;

            m_PrefabPath = Path.Combine(k_TempFolder, plane.name + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(plane, m_PrefabPath);
            Object.DestroyImmediate(plane);

            NavMesh.RemoveAllNavMeshData();
        }

        [UnityTearDown]
        public IEnumerator TearDownAndReturnToMainStage()
        {
            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();

            StageUtility.GoToMainStage();

            TestUtility.EliminateFromScene(ref m_ModVolInstance, k_KeepSceneObjects);
            TestUtility.EliminateFromScene(ref m_SurfaceInstance, k_KeepSceneObjects);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ModifierVolume_WhenInsidePrefabMode_ModifiesTheNavMeshInPrefab(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_SurfaceInstance = TestUtility.InstantiatePrefab(prefab, "SurfaceSeekingModVol" + m_TestCounter + "PrefabInstance");

            NavMesh.SamplePosition(Vector3.zero, out var hit, 0.1f, k_QueryAnyArea);
            Assume.That(hit.hit, Is.False, "Prefab should not have a NavMesh in the beginning.");

            if (runMode == RunMode.PlayMode)
            {
                yield return new EnterPlayMode();
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            }

            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var modifierVolume = prefabStage.prefabContentsRoot.AddComponent<NavMeshModifierVolume>();
            modifierVolume.area = k_RedArea;
            modifierVolume.center = Vector3.zero;
            modifierVolume.size = Vector3.one;
            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_PrefabDefaultArea);
            PrefabSavingUtil.SavePrefab(prefabStage);
            StageUtility.GoToMainStage();

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();

            NavMesh.SamplePosition(Vector3.zero, out var hitCenter, 0.1f, k_QueryAnyArea);
            Assume.That(hitCenter.hit, Is.True, "A NavMesh should have been baked in the center of the prefab.");
            Assert.That(hitCenter.mask, Is.EqualTo(1 << k_RedArea),
                "Area type (0x{0:x8}) found in the center should be 0x{1:x8}.", hitCenter.mask, 1 << k_RedArea);

            NavMesh.SamplePosition(new Vector3(0.6f, 0, 0.6f), out var hitSides, 0.1f, k_QueryAnyArea);
            Assume.That(hitSides.hit, Is.True, "A NavMesh should have been baked in the outer sides of the prefab.");
            Assert.That(hitSides.mask, Is.EqualTo(1 << k_PrefabDefaultArea),
                "Area type (0x{0:x8}) found on the sides should be 0x{1:x8}.", hitSides.mask, 1 << k_PrefabDefaultArea);

            Assert.That(hitCenter.mask, Is.Not.EqualTo(hitSides.mask),
                "Area type (0x{0:x8}) in the center should be different than on the sides.", hitCenter.mask);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        [UnityTest]
        public IEnumerator ModifierVolume_WhenInsidePrefabMode_DoesNotAffectTheNavMeshInMainScene(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            m_SurfaceInstance = GameObject.CreatePrimitive(PrimitiveType.Plane);
            m_SurfaceInstance.name = "SurfaceOutsidePrefab" + m_TestCounter;
            var mainSceneSurface = m_SurfaceInstance.AddComponent<NavMeshSurface>();
            mainSceneSurface.defaultArea = k_PinkArea;
            mainSceneSurface.agentTypeID = 0;
            mainSceneSurface.collectObjects = CollectObjects.All;

            NavMesh.SamplePosition(Vector3.zero, out var hit, 0.1f, k_QueryAnyArea);
            Assume.That(hit.hit, Is.False, "The main scene should not have a NavMesh in the beginning.");

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabModVol = prefabStage.prefabContentsRoot.AddComponent<NavMeshModifierVolume>();
            prefabModVol.area = k_PrefabDefaultArea;
            prefabModVol.center = Vector3.zero;
            prefabModVol.size = new Vector3(100, 100, 100);

            // Bake the NavMeshSurface from the main scene while the prefab mode is open
            if (runMode == RunMode.PlayMode)
                yield return new EnterPlayMode();

            mainSceneSurface = m_SurfaceInstance.GetComponent<NavMeshSurface>();
            prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            yield return TestUtility.BakeNavMeshAsync(mainSceneSurface, mainSceneSurface.defaultArea);

            PrefabSavingUtil.SavePrefab(prefabStage);
            if (!EditorApplication.isPlaying)
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            StageUtility.GoToMainStage();

            NavMesh.SamplePosition(Vector3.zero, out hit, 0.1f, k_QueryAnyArea);
            Assert.That(hit.hit, Is.True, "A NavMesh should have been baked by the surface in the main scene.");
            Assert.That(hit.mask, Is.EqualTo(1 << mainSceneSurface.defaultArea),
                "NavMesh has the area type 0x{0:x8} instead of the expected 0x{1:x8}.", hit.mask, 1 << mainSceneSurface.defaultArea);

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator ModifierVolume_WhenOutsidePrefabMode_DoesNotAffectTheNavMeshInPrefab(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            m_ModVolInstance = new GameObject("ModifierVolumeOutsidePrefab" + m_TestCounter);
            var modifierVolume = m_ModVolInstance.AddComponent<NavMeshModifierVolume>();
            modifierVolume.area = k_RedArea;
            modifierVolume.center = Vector3.zero;
            modifierVolume.size = new Vector3(20, 20, 20);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_SurfaceInstance = TestUtility.InstantiatePrefab(prefab, "SurfaceSeekingModVol" + m_TestCounter + "PrefabInstance");

            NavMesh.SamplePosition(Vector3.zero, out var hit, 0.1f, k_QueryAnyArea);
            Assume.That(hit.hit, Is.False, "Prefab should not have a NavMesh in the beginning.");

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            if (runMode == RunMode.PlayMode)
            {
                yield return new EnterPlayMode();

                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            }

            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();

            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_PrefabDefaultArea);

            PrefabSavingUtil.SavePrefab(prefabStage);
            StageUtility.GoToMainStage();

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();

            NavMesh.SamplePosition(Vector3.zero, out hit, 0.1f, k_QueryAnyArea);
            Assume.That(hit.hit, Is.True, "A NavMesh should have been baked in the prefab.");
            Assert.That(hit.mask, Is.EqualTo(1 << k_PrefabDefaultArea),
                "A different area type (0x{0:x8}) was found instead of the expected one (0x{1:x8}).", hit.mask, 1 << k_PrefabDefaultArea);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
    }
}
