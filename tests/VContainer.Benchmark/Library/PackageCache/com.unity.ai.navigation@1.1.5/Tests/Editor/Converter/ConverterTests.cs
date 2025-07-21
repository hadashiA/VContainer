#if UNITY_2022_2_OR_NEWER

using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation.Updater;
using NavMeshBuilder = UnityEditor.AI.NavMeshBuilder;

namespace Unity.AI.Navigation.Editor.Tests
{
    [Description("Tests suite related to the systems used to convert editor data from the legacy NavMesh systems to the modern component-based navigation extension")]
    class ConverterTests
    {
        const string k_RootFolder = "Assets";
        const string k_TestFolder = "ConverterTests";
        const string k_TestFolderPath = k_RootFolder + "/" + k_TestFolder;
        const string k_TestScenePath = k_TestFolderPath + "/ConverterTestsScene.unity";
        const string k_BuildHeightMeshPropertyName = "m_BuildSettings.buildHeightMesh";

        bool m_BuildHeightMeshPreviousValue;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!AssetDatabase.IsValidFolder(k_TestFolderPath))
                AssetDatabase.CreateFolder(k_RootFolder, k_TestFolder);
            Assume.That(AssetDatabase.IsValidFolder(k_TestFolderPath));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var planeGameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
#pragma warning disable 618
            GameObjectUtility.SetStaticEditorFlags(planeGameObject, StaticEditorFlags.NavigationStatic);
#pragma warning restore 618
            EditorSceneManager.SaveScene(scene, k_TestScenePath);

            // Enable desired build settings (build HeightMesh)
            var settingsObject = new SerializedObject(NavMeshBuilder.navMeshSettingsObject);
            Assume.That(settingsObject, Is.Not.Null, "Unable to get the build settings object");
            var buildHeightMeshProperty = settingsObject.FindProperty(k_BuildHeightMeshPropertyName);
            Assume.That(buildHeightMeshProperty, Is.Not.Null, "Unable to get the buildHeightMesh property from the build settings object");
            m_BuildHeightMeshPreviousValue = buildHeightMeshProperty.boolValue;
            buildHeightMeshProperty.boolValue = true;
            settingsObject.ApplyModifiedProperties();
            Assume.That(buildHeightMeshProperty.boolValue, Is.True, "buildHeightMesh property from the build settings object should be true");

            NavMeshBuilder.BuildNavMesh();
            EditorSceneManager.SaveScene(scene, k_TestScenePath);

            Assume.That(NavMeshUpdaterUtility.IsSceneReferencingLegacyNavMesh(k_TestScenePath));

            NavMeshUpdaterUtility.ConvertScene(k_TestScenePath);
        }

        [Test]
        public void Converter_AfterConversion_SceneNavMeshAssetIsGone()
        {
            var navMeshOwnedByScene = NavMeshUpdaterUtility.IsSceneReferencingLegacyNavMesh(k_TestScenePath);
            Assert.IsFalse(navMeshOwnedByScene, "Converted scene should not own a NavMesh after conversion");
        }

        [Test]
        public void Converter_AfterConversion_NavMeshSurfaceIsPresent()
        {
            var surface = Object.FindAnyObjectByType<NavMeshSurface>();
            Assert.IsNotNull(surface, "Unable to find a NavMesh surface, it should have been created by the conversion");
        }

        [Test]
        public void Converter_AfterConversion_NavMeshIsPresent()
        {
            var sampleSuccess = NavMesh.SamplePosition(Vector3.zero, out var hit, 1.0f, NavMesh.AllAreas);
            Assert.IsTrue(sampleSuccess && hit.hit, "NavMesh should still be present after conversion");
        }

        [Test]
        public void Converter_AfterConversion_NoNavigationStaticGameObjects()
        {
            var gameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var gameObject in gameObjects)
            {
#pragma warning disable 618
                Assert.IsFalse(GameObjectUtility.AreStaticEditorFlagsSet(gameObject, StaticEditorFlags.NavigationStatic), "Objects should not be flagged as NavigationStatic after conversion");
#pragma warning restore 618
            }
        }

        [Test]
        public void Converter_AfterConversion_HeightMeshIsPresent()
        {
            var surface = Object.FindAnyObjectByType<NavMeshSurface>();
            Assume.That(surface, Is.Not.Null, "Unable to find a NavMesh surface, it should have been created by the conversion");
            Assert.IsTrue(surface.buildHeightMesh, "A scene NavMesh built with HeightMesh should be converted to a surface with the buildHeightMesh option enabled");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (AssetDatabase.IsValidFolder(k_TestFolderPath))
                AssetDatabase.DeleteAsset(k_TestFolderPath);

            // Restore build settings value
            var settingsObject = new SerializedObject(NavMeshBuilder.navMeshSettingsObject);
            Assume.That(settingsObject, Is.Not.Null, "Unable to get the build settings object");
            var buildHeightMeshProperty = settingsObject.FindProperty(k_BuildHeightMeshPropertyName);
            Assume.That(buildHeightMeshProperty, Is.Not.Null, "Unable to get the buildHeightMesh property from the build settings object");
            buildHeightMeshProperty.boolValue = m_BuildHeightMeshPreviousValue;
            settingsObject.ApplyModifiedProperties();
        }
    }
}
#endif
