using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

[TestFixture]
public class RectTransformValidAfterEnable : IPrebuildSetup
{
    const string kSceneName = "DisabledCanvasScene";
    const string kGameObjectName = "DisabledCanvas";
    public void Setup()
    {
#if UNITY_EDITOR
        Action codeToExecute = delegate()
        {
            var canvasGameObject = new GameObject(kGameObjectName, typeof(Canvas));
            canvasGameObject.SetActive(false);
            canvasGameObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
            canvasGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1024, 768);
        };
        CreateSceneUtility.CreateScene(kSceneName, codeToExecute);
#endif
    }

    [UnityTest]
    public IEnumerator CheckRectTransformValidAfterEnable()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(kSceneName, LoadSceneMode.Additive);
        yield return operation;

        Scene scene = SceneManager.GetSceneByName(kSceneName);
        GameObject[] gameObjects = scene.GetRootGameObjects();
        GameObject canvasGameObject = null;
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.name == kGameObjectName)
            {
                canvasGameObject = gameObject;
                break;
            }
        }
        Assert.IsNotNull(canvasGameObject);

        RectTransform rectTransform = canvasGameObject.GetComponent<RectTransform>();
        canvasGameObject.SetActive(true);

        yield return new WaitForEndOfFrame();

        Rect rect = rectTransform.rect;
        Assert.Greater(rect.width, 0);
        Assert.Greater(rect.height, 0);

        operation = SceneManager.UnloadSceneAsync(kSceneName);
        yield return operation;
    }

    [TearDown]
    public void TearDown()
    {
        //Manually add Assets/ and .unity as CreateSceneUtility does that for you.
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset("Assets/" + kSceneName + ".unity");
#endif
    }
}
