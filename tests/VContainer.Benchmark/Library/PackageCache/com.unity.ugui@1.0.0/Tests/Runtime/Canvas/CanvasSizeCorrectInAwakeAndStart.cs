using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEditor;

[TestFixture]
public class CanvasSizeCorrectInAwakeAndStart : IPrebuildSetup
{
    const string k_SceneName = "CanvasSizeCorrectInAwakeAndStartScene";
    GameObject m_CanvasGameObject;
    Scene m_InitScene;

    public void Setup()
    {
#if UNITY_EDITOR
        Action codeToExecute = delegate()
        {
            var canvasGO = new GameObject("CanvasToAddImage", typeof(Canvas));
            var imageGO = new GameObject("ImageOnCanvas", typeof(UnityEngine.UI.Image));
            imageGO.transform.localPosition = Vector3.one;
            imageGO.transform.SetParent(canvasGO.transform);
            imageGO.AddComponent<CanvasSizeCorrectInAwakeAndStartScript>();
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            imageGO.SetActive(false);
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
    public IEnumerator CanvasSizeIsCorrectInAwakeAndStart()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(k_SceneName, LoadSceneMode.Additive);
        yield return operation;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(k_SceneName));
        m_CanvasGameObject = GameObject.Find("CanvasToAddImage");
        var imageGO = m_CanvasGameObject.transform.Find("ImageOnCanvas");
        imageGO.gameObject.SetActive(true);
        var component = imageGO.GetComponent<CanvasSizeCorrectInAwakeAndStartScript>();

        yield return new WaitUntil(() => component.isAwakeCalled && component.isStartCalled);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasGameObject);
        SceneManager.SetActiveScene(m_InitScene);
        SceneManager.UnloadSceneAsync(k_SceneName);
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset("Assets/" + k_SceneName + ".unity");
#endif
    }
}
