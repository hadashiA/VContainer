using System;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEditor;

[TestFixture]
[UnityPlatform(include = new RuntimePlatform[] { RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor, RuntimePlatform.WindowsEditor })]
[Category("RegressionTest")]
[Description("CoveredBugID = 883807, CoveredBugDescription = \"Object::GetInstanceID crash when trying to switch canvas\"")]
public class NoActiveCameraInSceneDoesNotCrashEditor : IPrebuildSetup
{
    Scene m_InitScene;
    const string k_SceneName = "NoActiveCameraInSceneDoesNotCrashEditorScene";

    public void Setup()
    {
#if UNITY_EDITOR
        Action codeToExecute = delegate()
        {
            UnityEditor.EditorApplication.ExecuteMenuItem("GameObject/UI/Legacy/Button");
        };

        CreateSceneUtility.CreateScene(k_SceneName, codeToExecute);
#endif
    }

    [SetUp]
    public void TestSetup()
    {
        m_InitScene = SceneManager.GetActiveScene();
    }

    [UnityTest]
    public IEnumerator EditorShouldNotCrashWithoutActiveCamera()
    {
        AsyncOperation operationResult = SceneManager.LoadSceneAsync(k_SceneName, LoadSceneMode.Additive);
        yield return operationResult;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(k_SceneName));

        Profiler.enabled = true;
        Camera.main.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.1f);
        Assert.That(Profiler.enabled, Is.True, "Expected the profiler to be enabled. Unable to test if the profiler will crash the editor if it is not enabled.");
    }

    [TearDown]
    public void TearDown()
    {
        SceneManager.SetActiveScene(m_InitScene);
        SceneManager.UnloadSceneAsync(k_SceneName);
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset("Assets/" + k_SceneName + ".unity");
#endif
    }
}
