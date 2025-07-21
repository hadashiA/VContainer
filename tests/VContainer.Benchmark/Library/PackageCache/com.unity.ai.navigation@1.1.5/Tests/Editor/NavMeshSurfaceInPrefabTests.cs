//#define KEEP_ARTIFACTS_FOR_INSPECTION
//#define ENABLE_TEST_LOGS

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
    [Category("PrefabsWithNavMeshComponents")]
    class NavMeshSurfaceInPrefabTests
    {
        const string k_AutoSaveKey = "AutoSave";
        const string k_ParentFolder = "Assets";
        const string k_TempFolderName = "TempPrefab";

        static readonly string k_TempFolder = Path.Combine(k_ParentFolder, k_TempFolderName);

        const int k_GrayArea = 7;
        const int k_BrownArea = 10;
        const int k_RedArea = 18;
        const int k_OrangeArea = 26;
        const int k_YellowArea = 30;

        const int k_PrefabDefaultArea = k_YellowArea;

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
        GameObject m_MainInstance;
        [SerializeField]
        GameObject m_SecondInstance;

#if KEEP_ARTIFACTS_FOR_INSPECTION
        const bool k_KeepSceneObjects = true;
#else
        const bool k_KeepSceneObjects = false;
#endif
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // Skip the entire setup phase that runs again each time an editor test enters playmode
            if (EditorApplication.isPlaying)
                return;

            AssetDatabase.DeleteAsset(k_TempFolder);
            var folderGUID = AssetDatabase.CreateFolder(k_ParentFolder, k_TempFolderName);
            Assume.That(folderGUID, Is.Not.Empty);

            SessionState.SetBool(k_AutoSaveKey, PrefabStageAutoSavingUtil.GetPrefabStageAutoSave());
            PrefabStageAutoSavingUtil.SetPrefabStageAutoSave(false);
            StageUtility.GoToMainStage();

            m_PreviousScenePath = SceneManager.GetActiveScene().path;
            m_TempScenePath = Path.Combine(k_TempFolder, "NavMeshSurfacePrefabTestsScene.unity");
            var tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(tempScene, m_TempScenePath);
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
            {
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            }

            EditorSettings.enterPlayModeOptionsEnabled = s_EnterPlayModeOptionsEnabled;
            EditorSettings.enterPlayModeOptions = s_EnterPlayModeOptions;

#if !KEEP_ARTIFACTS_FOR_INSPECTION
            AssetDatabase.DeleteAsset(k_TempFolder);
#endif
        }

        [UnitySetUp]
        public IEnumerator SetupNewPrefabWithNavMesh()
        {
            if (EditorApplication.isPlaying)
                yield break;

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "NavMeshSurface" + (++m_TestCounter) + "Prefab";
            var surface = plane.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;

            m_PrefabPath = Path.Combine(k_TempFolder, plane.name + ".prefab");
            var planePrefab = PrefabUtility.SaveAsPrefabAsset(plane, m_PrefabPath);
            Object.DestroyImmediate(plane);

            AssetDatabase.OpenAsset(planePrefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_PrefabDefaultArea);
            PrefabSavingUtil.SavePrefab(prefabStage);
            StageUtility.GoToMainStage();

            NavMesh.RemoveAllNavMeshData();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDownAndReturnToMainStage()
        {
            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
                prefabStage.ClearDirtiness();

            StageUtility.GoToMainStage();

            TestUtility.EliminateFromScene(ref m_MainInstance, k_KeepSceneObjects);
            TestUtility.EliminateFromScene(ref m_SecondInstance, k_KeepSceneObjects);

            yield return null;
        }

        static void TestNavMeshExistsAloneAtPosition(int expectedArea, Vector3 pos)
        {
            var expectedAreaMask = 1 << expectedArea;

#if ENABLE_TEST_LOGS
        var areaExists = HasNavMeshAtPosition(pos, expectedAreaMask);
        var otherAreasExist = HasNavMeshAtPosition(pos, ~expectedAreaMask);
        Debug.Log(" mask=" + expectedAreaMask.ToString("x8") + " area " + expectedArea +
            " Exists=" + areaExists + " otherAreasExist=" + otherAreasExist + " at position " + pos);
        if (otherAreasExist)
        {
            for (var i = 0; i < 32; i++)
            {
                if (i == expectedArea)
                    continue;

                var thisOtherAreaExists = HasNavMeshAtPosition(pos, 1 << i);
                if (thisOtherAreaExists)
                {
                    Debug.Log(" _another area that exists here " + i);
                }
            }
        }
#endif
            Assert.IsTrue(HasNavMeshAtPosition(pos, expectedAreaMask), "Expected NavMesh with area {0} at position {1}.", expectedArea, pos);
            Assert.IsFalse(HasNavMeshAtPosition(pos, ~expectedAreaMask), "A NavMesh with an area other than {0} exists at position {1}.", expectedArea, pos);
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenOpenedInPrefabMode_DoesNotActivateItsNavMesh(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            AssetDatabase.OpenAsset(prefab);

            if (runMode == RunMode.PlayMode)
                yield return new EnterPlayMode();

            NavMesh.SamplePosition(Vector3.zero, out var hit, 1000000f, new NavMeshQueryFilter { areaMask = NavMesh.AllAreas, agentTypeID = 0 });
            Assert.That(hit.hit, Is.False, "The NavMesh instance of a prefab opened for edit should not be active under any circumstances.");

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_AfterBakingInPrefabMode_DoesNotActivateItsNavMesh(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            AssetDatabase.OpenAsset(prefab);

            if (runMode == RunMode.PlayMode)
                yield return new EnterPlayMode();

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            NavMeshAssetManager.instance.ClearSurfaces(new Object[] { prefabSurface });
            PrefabSavingUtil.SavePrefab(prefabStage);

            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_RedArea);

            NavMesh.SamplePosition(Vector3.zero, out var hit, 1000000f, new NavMeshQueryFilter { areaMask = NavMesh.AllAreas, agentTypeID = 0 });
            Assert.That(hit.hit, Is.False, "The NavMesh instance of a prefab opened for edit should not be active after baking the surface.");

            prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            PrefabSavingUtil.SavePrefab(prefabStage);

            NavMesh.SamplePosition(Vector3.zero, out hit, 1000000f, new NavMeshQueryFilter { areaMask = NavMesh.AllAreas, agentTypeID = 0 });
            Assert.That(hit.hit, Is.False, "The NavMesh instance of a prefab opened for edit should not be active after baking the surface.");

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_AfterBakingInPrefabMode_LeavesMainSceneUntouched(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            Assume.That(HasNavMeshAtPosition(Vector3.zero), Is.False);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            AssetDatabase.OpenAsset(prefab);

            if (runMode == RunMode.PlayMode)
                yield return new EnterPlayMode();

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            Assume.That(prefabStage, Is.Not.Null);
            Assume.That(prefabStage.prefabContentsRoot, Is.Not.Null);

            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var initialPrefabNavMeshData = prefabSurface.navMeshData;
            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_RedArea);

            Assert.AreNotSame(initialPrefabNavMeshData, prefabSurface.navMeshData);

            PrefabSavingUtil.SavePrefab(prefabStage);
            StageUtility.GoToMainStage();

            yield return null;

            Assert.IsFalse(HasNavMeshAtPosition(Vector3.zero, NavMesh.AllAreas, 0, 1000.0f));

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenInstantiated_ReferencesTheSameNavMeshData(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            if (runMode == RunMode.PlayMode)
                yield return new EnterPlayMode();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstance");
            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, Vector3.zero);

            var instanceSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceSurface, Is.Not.Null);
            var instanceNavMeshData = instanceSurface.navMeshData;

            var clonePosition = new Vector3(20, 0, 0);
            m_SecondInstance = Object.Instantiate(m_MainInstance, clonePosition, Quaternion.identity);
            Assume.That(m_SecondInstance, Is.Not.Null);
            m_SecondInstance.name = "Surface" + m_TestCounter + "PrefabInstanceClone";

            const int expectedAreaMask = 1 << k_PrefabDefaultArea;
            Assert.IsTrue(HasNavMeshAtPosition(clonePosition, expectedAreaMask));
            Assert.IsFalse(HasNavMeshAtPosition(clonePosition, ~expectedAreaMask));

            var instanceCloneSurface = m_SecondInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceCloneSurface, Is.Not.Null);
            var instanceCloneNavMeshData = instanceCloneSurface.navMeshData;
            Assert.AreSame(instanceNavMeshData, instanceCloneNavMeshData);

            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            Assume.That(prefabStage, Is.Not.Null);
            Assume.That(prefabStage.prefabContentsRoot, Is.Not.Null);

            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var prefabNavMeshData = prefabSurface.navMeshData;
            Assert.AreSame(prefabNavMeshData, instanceNavMeshData);

            StageUtility.GoToMainStage();

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenInstantiatedAndCleared_InstanceHasEmptyNavMeshData(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstance");
            var instanceSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceSurface.navMeshData != null, "NavMeshSurface in prefab instance must have NavMeshData.");

            if (runMode == RunMode.PlayMode)
                yield return new EnterPlayMode();

            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            NavMeshAssetManager.instance.ClearSurfaces(new Object[] { prefabSurface });
            PrefabSavingUtil.SavePrefab(prefabStage);

            if (EditorApplication.isPlaying)
            {
                yield return new ExitPlayMode();

                instanceSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            }

            StageUtility.GoToMainStage();
            Assert.IsTrue(instanceSurface.navMeshData == null,
                "After the NavMeshSurface in the prefab has been cleared the prefab instance should no longer hold NavMeshData.");
            const int expectedAreaMask = 1 << k_PrefabDefaultArea;
            Assert.IsFalse(HasNavMeshAtPosition(Vector3.zero, expectedAreaMask));

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenBakesNewNavMesh_UpdatesTheInstance(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            if (runMode == RunMode.PlayMode)
                yield return new EnterPlayMode();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstanceOne");

            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, m_MainInstance.transform.position);

            AssetDatabase.OpenAsset(prefab);
            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, m_MainInstance.transform.position);

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();

            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_RedArea);

            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, m_MainInstance.transform.position);

            PrefabSavingUtil.SavePrefab(prefabStage);

            StageUtility.GoToMainStage();

            m_SecondInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstanceTwo");

            // Reactivate the object to apply the change of position immediately
            m_SecondInstance.SetActive(false);
            m_SecondInstance.transform.position = new Vector3(20, 0, 0);
            m_SecondInstance.SetActive(true);

            // Check that the second prefab instance has the new prefab area type
            TestNavMeshExistsAloneAtPosition(k_RedArea, m_SecondInstance.transform.position);

            // Only in edit mode, check that the prefab change has been picked up by the first instance
            if (!EditorApplication.isPlaying)
            {
                TestNavMeshExistsAloneAtPosition(k_RedArea, m_MainInstance.transform.position);

                // Modify the first instance
                var instanceOneSurface = m_MainInstance.GetComponent<NavMeshSurface>();
                yield return TestUtility.BakeNavMeshAsync(instanceOneSurface, k_BrownArea);

                // Check that the first prefab instance kept its modified area type
                TestNavMeshExistsAloneAtPosition(k_BrownArea, m_MainInstance.transform.position);
            }
            else
            {
                // After the prefab has been saved the running prefab instance should still have the old NavMeshData
                TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, m_MainInstance.transform.position);
            }

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenInstanceRebaked_HasDifferentNavMeshData(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            if (runMode == RunMode.PlayMode)
                yield return new EnterPlayMode();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstance");

            var clonePosition = new Vector3(20, 0, 0);
            m_SecondInstance = Object.Instantiate(m_MainInstance, clonePosition, Quaternion.identity);
            Assume.That(m_SecondInstance, Is.Not.Null);
            m_SecondInstance.name = "Surface" + m_TestCounter + "PrefabInstanceClone";

            var mainSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(mainSurface, Is.Not.Null);
            yield return TestUtility.BakeNavMeshAsync(mainSurface, k_RedArea);
            var mainNavMeshData = mainSurface.navMeshData;

            TestNavMeshExistsAloneAtPosition(k_RedArea, m_MainInstance.transform.position);

            // For when multiple instances of the same NavMesh prefab modify their data in playmode the behavior is currently undefined
            if (runMode != RunMode.PlayMode)
            {
                var cloneSurface = m_SecondInstance.GetComponent<NavMeshSurface>();

                Assert.IsTrue(cloneSurface.navMeshData != null, "The clone should still have NavMesh data.");

                const int expectedAreaMask = 1 << k_PrefabDefaultArea;
                Assert.IsTrue(HasNavMeshAtPosition(clonePosition, expectedAreaMask), "The clone should still reference the prefab's data.");
                Assert.IsFalse(HasNavMeshAtPosition(clonePosition, ~expectedAreaMask));
            }

            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var prefabNavMeshData = prefabSurface.navMeshData;
            Assert.AreNotSame(mainNavMeshData, prefabNavMeshData);

            if (runMode != RunMode.PlayMode)
            {
                var instanceCloneSurface = m_SecondInstance.GetComponent<NavMeshSurface>();
                Assume.That(instanceCloneSurface, Is.Not.Null);
                var instanceCloneNavMeshData = instanceCloneSurface.navMeshData;
                Assert.AreNotSame(instanceCloneNavMeshData, mainNavMeshData);
                Assert.That(instanceCloneNavMeshData, Is.EqualTo(prefabNavMeshData));
            }

            StageUtility.GoToMainStage();

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenInstanceCleared_InstanceHasEmptyNavMeshData()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstance");

            var clonePosition = new Vector3(20, 0, 0);
            m_SecondInstance = Object.Instantiate(m_MainInstance, clonePosition, Quaternion.identity);
            Assume.That(m_SecondInstance, Is.Not.Null);
            m_SecondInstance.name = "Surface" + m_TestCounter + "PrefabInstanceClone";

            var instanceSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceSurface, Is.Not.Null);
            NavMeshAssetManager.instance.ClearSurfaces(new Object[] { instanceSurface });

            const int expectedAreaMask = 1 << k_PrefabDefaultArea;
            Assert.IsFalse(HasNavMeshAtPosition(Vector3.zero, expectedAreaMask));

            Assert.IsTrue(HasNavMeshAtPosition(clonePosition, expectedAreaMask));
            Assert.IsFalse(HasNavMeshAtPosition(clonePosition, ~expectedAreaMask));

            var instanceCloneSurface = m_SecondInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceCloneSurface, Is.Not.Null);
            var instanceCloneNavMeshData = instanceCloneSurface.navMeshData;

            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var prefabNavMeshData = prefabSurface.navMeshData;
            Assert.AreNotSame(prefabNavMeshData, instanceSurface.navMeshData);
            Assert.AreNotSame(instanceCloneNavMeshData, instanceSurface.navMeshData);
            Assert.AreSame(prefabNavMeshData, instanceCloneNavMeshData);

            StageUtility.GoToMainStage();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenInstanceCleared_PrefabKeepsNavMeshData()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstance");

            var instanceSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceSurface, Is.Not.Null);
            var initialPrefabNavMeshData = instanceSurface.navMeshData;
            NavMeshAssetManager.instance.ClearSurfaces(new Object[] { instanceSurface });

            const int expectedAreaMask = 1 << k_PrefabDefaultArea;
            Assert.IsFalse(HasNavMeshAtPosition(Vector3.zero, expectedAreaMask));

            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var prefabNavMeshData = prefabSurface.navMeshData;
            Assert.IsTrue(prefabNavMeshData != null,
                "NavMeshSurface in the prefab must still have NavMeshData even though the instance was cleared.");
            Assert.AreSame(initialPrefabNavMeshData, prefabNavMeshData);

            StageUtility.GoToMainStage();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenRebakedButInstanceModified_DoesNotChangeDataReferencedByInstance(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            if (runMode == RunMode.PlayMode)
                yield return new EnterPlayMode();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstance");

            var instanceSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceSurface, Is.Not.Null);
            yield return TestUtility.BakeNavMeshAsync(instanceSurface, k_RedArea);
            var instanceNavMeshData = instanceSurface.navMeshData;

            TestNavMeshExistsAloneAtPosition(k_RedArea, Vector3.zero);

            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var initialPrefabNavMeshData = prefabSurface.navMeshData;
            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_GrayArea);
            PrefabSavingUtil.SavePrefab(prefabStage);
            StageUtility.GoToMainStage();

            AssetDatabase.OpenAsset(prefab);
            var prefabStageReopened = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurfaceReopened = prefabStageReopened.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var prefabNavMeshData = prefabSurfaceReopened.navMeshData;
            Assert.IsTrue(prefabNavMeshData != null,
                "NavMeshSurface in prefab must have NavMeshData after baking, saving, closing and reopening.");
            Assert.AreNotSame(instanceNavMeshData, prefabNavMeshData);
            Assert.AreNotSame(initialPrefabNavMeshData, prefabNavMeshData);

            StageUtility.GoToMainStage();
            Assert.AreSame(instanceNavMeshData, instanceSurface.navMeshData);

            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenRebakedButNotSaved_RevertsToTheInitialNavMeshData()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var initialPrefabNavMeshData = prefabSurface.navMeshData;
            var initialPrefabNavMeshAssetPath = AssetDatabase.GetAssetPath(initialPrefabNavMeshData);
            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_GrayArea);
            var rebuiltPrefabNavMeshData = prefabSurface.navMeshData;
            Assert.IsTrue(rebuiltPrefabNavMeshData != null, "NavMeshSurface must have NavMeshData after baking.");
            Assert.AreNotSame(initialPrefabNavMeshData, rebuiltPrefabNavMeshData);

            prefabStage.ClearDirtiness();
            StageUtility.GoToMainStage();

            AssetDatabase.OpenAsset(prefab);
            var prefabStageReopened = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurfaceReopened = prefabStageReopened.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var prefabNavMeshData = prefabSurfaceReopened.navMeshData;
            Assert.AreSame(initialPrefabNavMeshData, prefabNavMeshData);
            Assert.AreNotSame(rebuiltPrefabNavMeshData, prefabNavMeshData);
            var prefabNavMeshAssetPath = AssetDatabase.GetAssetPath(prefabNavMeshData);
            StringAssert.AreEqualIgnoringCase(initialPrefabNavMeshAssetPath, prefabNavMeshAssetPath,
                "The NavMeshData asset referenced by the prefab should remain the same when exiting prefab mode without saving.");

            StageUtility.GoToMainStage();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenRebakedButNotSaved_TheRebakedAssetNoLongerExists()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_GrayArea);
            var rebakedAssetPath = AssetDatabase.GetAssetPath(prefabSurface.navMeshData);

            Assert.IsTrue(File.Exists(rebakedAssetPath), "NavMeshData file must exist. ({0})", rebakedAssetPath);

            prefabStage.ClearDirtiness();
            StageUtility.GoToMainStage();

            Assert.IsFalse(File.Exists(rebakedAssetPath), "NavMeshData file still exists after discarding the changes. ({0})", rebakedAssetPath);

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenRebaked_TheOldAssetExistsUntilSavingAndNotAfter()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var initialNavMeshData = prefabSurface.navMeshData;
            var initialAssetPath = AssetDatabase.GetAssetPath(prefabSurface.navMeshData);

            Assume.That(initialNavMeshData != null, "Prefab must have some NavMeshData.");
            Assume.That(File.Exists(initialAssetPath), Is.True, "NavMeshData file must exist. ({0})", initialAssetPath);

            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_GrayArea);

            Assert.IsTrue(initialNavMeshData != null, "The initial NavMeshData must still exist immediately after prefab re-bake.");
            Assert.IsTrue(File.Exists(initialAssetPath), "The initial NavMeshData file must exist after prefab re-bake. ({0})", initialAssetPath);

            Assert.IsTrue(prefabSurface.navMeshData != null, "NavMeshSurface must have NavMeshData after baking.");
            var unsavedRebakedNavMeshData = prefabSurface.navMeshData;

            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_OrangeArea);

            // Assert.IsNull would return a wrong result here (e.g. Expected: null  But was: <null>) 
            Assert.IsTrue(unsavedRebakedNavMeshData == null,"An unsaved NavMeshData should not exist after a re-bake.");
            Assert.IsTrue(prefabSurface.navMeshData != null, "NavMeshSurface must have NavMeshData after baking.");

            PrefabSavingUtil.SavePrefab(prefabStage);
            Assert.IsFalse(File.Exists(initialAssetPath), "NavMeshData file still exists after saving. ({0})", initialAssetPath);
            Assert.IsTrue(initialNavMeshData == null, "The initial NavMeshData must no longer exist after saving the prefab.");

            // This code is still reachable because initialNavMeshData has been affected by BakeNavMeshAsync()
            StageUtility.GoToMainStage();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenRebakedAndAutoSaved_InstanceHasTheNewNavMeshData()
        {
            var wasAutoSave = PrefabStageAutoSavingUtil.GetPrefabStageAutoSave();
            PrefabStageAutoSavingUtil.SetPrefabStageAutoSave(true);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var initialPrefabNavMeshData = prefabSurface.navMeshData;
            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_GrayArea);
            var rebuiltPrefabNavMeshData = prefabSurface.navMeshData;
            Assert.IsTrue(rebuiltPrefabNavMeshData != null, "NavMeshSurface must have NavMeshData after baking.");
            Assert.AreNotSame(initialPrefabNavMeshData, rebuiltPrefabNavMeshData);

            StageUtility.GoToMainStage();

            AssetDatabase.OpenAsset(prefab);
            var prefabStageReopened = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurfaceReopened = prefabStageReopened.prefabContentsRoot.GetComponent<NavMeshSurface>();
            var prefabNavMeshData = prefabSurfaceReopened.navMeshData;
            Assert.AreNotSame(initialPrefabNavMeshData, prefabNavMeshData);
            Assert.AreSame(rebuiltPrefabNavMeshData, prefabNavMeshData);

            StageUtility.GoToMainStage();

            PrefabStageAutoSavingUtil.SetPrefabStageAutoSave(wasAutoSave);

            yield return null;
        }

        [Ignore("Currently the deletion of the old asset must be done manually.")]
        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_AfterModifiedInstanceAppliedBack_TheOldAssetNoLongerExists()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstance");
            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, Vector3.zero);

            var instanceSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceSurface, Is.Not.Null);

            var initialInstanceAssetPath = AssetDatabase.GetAssetPath(instanceSurface.navMeshData);

            Assert.IsTrue(File.Exists(initialInstanceAssetPath), "Prefab's NavMeshData file must exist. ({0})", initialInstanceAssetPath);

            yield return TestUtility.BakeNavMeshAsync(instanceSurface, k_RedArea);

            Assert.IsTrue(File.Exists(initialInstanceAssetPath),
                "Prefab's NavMeshData file exists after the instance has changed. ({0})", initialInstanceAssetPath);

            PrefabUtility.ApplyPrefabInstance(m_MainInstance, InteractionMode.AutomatedAction);

            Assert.IsFalse(File.Exists(initialInstanceAssetPath),
                "Prefab's NavMeshData file still exists after the changes from the instance have been applied back to the prefab. ({0})",
                initialInstanceAssetPath);

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_AfterModifiedInstanceAppliedBack_UpdatedAccordingToInstance()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstanceOne");
            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, Vector3.zero);

            m_SecondInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstanceTwo");

            // reactivate the object to apply the change of position immediately
            m_SecondInstance.SetActive(false);
            m_SecondInstance.transform.position = new Vector3(20, 0, 0);
            m_SecondInstance.SetActive(true);

            var instanceOneSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceOneSurface, Is.Not.Null);

            yield return TestUtility.BakeNavMeshAsync(instanceOneSurface, k_RedArea);

            TestNavMeshExistsAloneAtPosition(k_RedArea, Vector3.zero);
            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, m_SecondInstance.transform.position);

            PrefabUtility.ApplyPrefabInstance(m_MainInstance, InteractionMode.AutomatedAction);

            TestNavMeshExistsAloneAtPosition(k_RedArea, m_SecondInstance.transform.position);

            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();
            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_GrayArea);
            PrefabSavingUtil.SavePrefab(prefabStage);
            StageUtility.GoToMainStage();

            TestNavMeshExistsAloneAtPosition(k_GrayArea, Vector3.zero);
            TestNavMeshExistsAloneAtPosition(k_GrayArea, m_SecondInstance.transform.position);

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_AfterClearedInstanceAppliedBack_HasEmptyData()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstance");
            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, Vector3.zero);

            var instanceSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceSurface, Is.Not.Null);

            NavMeshAssetManager.instance.ClearSurfaces(new Object[] { instanceSurface });

            const int expectedAreaMask = 1 << k_PrefabDefaultArea;
            Assert.IsFalse(HasNavMeshAtPosition(Vector3.zero, expectedAreaMask));

            PrefabUtility.ApplyPrefabInstance(m_MainInstance, InteractionMode.AutomatedAction);

            AssetDatabase.OpenAsset(prefab);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();

            Assert.IsTrue(prefabSurface.navMeshData == null,
                "Prefab should have empty NavMeshData when empty data has been applied back from the instance.");

            StageUtility.GoToMainStage();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenInstanceRevertsBack_InstanceIsLikePrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstance");
            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, Vector3.zero);

            var instanceSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceSurface, Is.Not.Null);
            yield return TestUtility.BakeNavMeshAsync(instanceSurface, k_RedArea);

            TestNavMeshExistsAloneAtPosition(k_RedArea, Vector3.zero);

            PrefabUtility.RevertPrefabInstance(m_MainInstance, InteractionMode.AutomatedAction);

            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, Vector3.zero);

            yield return null;
        }

        [Ignore("Deletion of the old asset is expected to be done manually for the time being.")]
        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenInstanceRevertsBack_TheInstanceAssetNoLongerExists()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "Surface" + m_TestCounter + "PrefabInstance");
            TestNavMeshExistsAloneAtPosition(k_PrefabDefaultArea, Vector3.zero);

            var instanceSurface = m_MainInstance.GetComponent<NavMeshSurface>();
            Assume.That(instanceSurface, Is.Not.Null);
            yield return TestUtility.BakeNavMeshAsync(instanceSurface, k_RedArea);

            var instanceAssetPath = AssetDatabase.GetAssetPath(instanceSurface.navMeshData);

            Assert.IsTrue(File.Exists(instanceAssetPath), "Instance's NavMeshData file must exist. ({0})", instanceAssetPath);

            PrefabUtility.RevertPrefabInstance(m_MainInstance, InteractionMode.AutomatedAction);

            Assert.IsFalse(File.Exists(instanceAssetPath), "Instance's NavMeshData file still exists after revert. ({0})", instanceAssetPath);

            yield return null;
        }

        [Ignore("The expected behaviour has not been decided.")]
        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenDeleted_InstancesMakeCopiesOfData(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            yield return null;
            Assert.Fail("not implemented yet");
        }

        [UnityTest]
        public IEnumerator NavMeshSurfacePrefab_WhenBakingInPrefabModeScene_CollectsOnlyPrefabModeSceneObjects(
            [Values(RunMode.EditMode, RunMode.PlayMode)]
            RunMode runMode)
        {
            m_SecondInstance = GameObject.CreatePrimitive(PrimitiveType.Plane);
            var goName = "MainScenePlane" + m_TestCounter;
            m_SecondInstance.name = goName;
            m_SecondInstance.transform.localScale = new Vector3(100, 1, 100);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            AssetDatabase.OpenAsset(prefab);

            if (runMode == RunMode.PlayMode)
                yield return new EnterPlayMode();

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var prefabSurface = prefabStage.prefabContentsRoot.GetComponent<NavMeshSurface>();

            prefabSurface.collectObjects = CollectObjects.All;

            yield return TestUtility.BakeNavMeshAsync(prefabSurface, k_RedArea);

            PrefabSavingUtil.SavePrefab(prefabStage);
            StageUtility.GoToMainStage();

            if (EditorApplication.isPlaying)
            {
                yield return new ExitPlayMode();

                m_SecondInstance = GameObject.Find(goName);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            }

            m_MainInstance = TestUtility.InstantiatePrefab(prefab, "PrefabInstance" + m_TestCounter);

            TestNavMeshExistsAloneAtPosition(k_RedArea, Vector3.zero);

            var posNearby = new Vector3(20, 0, 0);
            Assert.IsFalse(HasNavMeshAtPosition(posNearby, 1 << k_RedArea),
                "NavMesh with the prefab's area exists at position {1}, outside the prefab's plane. ({0})",
                k_RedArea, posNearby);

            yield return null;
        }

        public static bool HasNavMeshAtPosition(Vector3 pos, int areaMask = NavMesh.AllAreas, int agentTypeId = 0, float range = 0.1f)
        {
            var filter = new NavMeshQueryFilter
            {
                areaMask = areaMask,
                agentTypeID = agentTypeId
            };
            var result = NavMesh.SamplePosition(pos, out _, range, filter);
            return result;
        }
    }
}
