using System;
using UnityEngine.UI;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

[TestFixture]
[Category("RegressionTest")]
public class SceneWithNestedLayoutElementsLoad : IPrebuildSetup
{
    Scene m_InitScene;
    const string aspectRatioFitterSceneName = "AspectRatioFitter";
    const string contentSizeFitterSceneName = "ContentSizeFitter";
    const string layoutGroupSceneName = "LayoutGroup";

    public void Setup()
    {
#if UNITY_EDITOR
        Action aspectRatioFitterSceneCreation = delegate()
        {
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
            var panelGO = new GameObject("Panel", typeof(UnityEngine.UI.Image), typeof(AspectRatioFitter));
            var imageGO = new GameObject("Image", typeof(UnityEngine.UI.RawImage), typeof(AspectRatioFitter));
            panelGO.transform.SetParent(canvasGO.transform);
            imageGO.transform.SetParent(panelGO.transform);

            var panelARF = panelGO.GetComponent<AspectRatioFitter>();
            panelARF.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            panelARF.aspectRatio = 1.98f;

            var iamgeARF = imageGO.GetComponent<AspectRatioFitter>();
            iamgeARF.aspectMode = AspectRatioFitter.AspectMode.None;
            iamgeARF.aspectRatio = 1f;

            new GameObject("GameObject", typeof(SceneWithNestedLayoutElementsLoadScript));
        };
        CreateSceneUtility.CreateScene(aspectRatioFitterSceneName, aspectRatioFitterSceneCreation);

        Action contentSizeFitterSceneCreation = delegate()
        {
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
            var panelGO = new GameObject("Panel", typeof(UnityEngine.UI.Image), typeof(ContentSizeFitter), typeof(LayoutElement));
            var imageGO = new GameObject("Image", typeof(UnityEngine.UI.RawImage), typeof(ContentSizeFitter), typeof(LayoutElement));
            panelGO.transform.SetParent(canvasGO.transform);
            imageGO.transform.SetParent(panelGO.transform);

            var panelCSF = panelGO.GetComponent<ContentSizeFitter>();
            panelCSF.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            panelCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var panelLE = panelGO.GetComponent<LayoutElement>();
            panelLE.preferredWidth = 200f;
            panelLE.preferredHeight = 200f;

            var imageCSF = imageGO.GetComponent<ContentSizeFitter>();
            imageCSF.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            imageCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var imageLE = imageGO.GetComponent<LayoutElement>();
            imageLE.preferredWidth = 100f;
            imageLE.preferredHeight = 100f;

            new GameObject("GameObject", typeof(SceneWithNestedLayoutElementsLoadScript));
        };
        CreateSceneUtility.CreateScene(contentSizeFitterSceneName, contentSizeFitterSceneCreation);

        Action layoutGroupSceneCreation = delegate()
        {
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GridLayoutGroup));
            var panelGO = new GameObject("Panel (0)", typeof(UnityEngine.UI.Image), typeof(GridLayoutGroup), typeof(LayoutElement));
            var imageGOOne = new GameObject("Image", typeof(UnityEngine.UI.RawImage), typeof(LayoutElement));
            var imageGOTwo = new GameObject("Image", typeof(UnityEngine.UI.RawImage), typeof(LayoutElement));
            panelGO.transform.SetParent(canvasGO.transform);
            imageGOOne.transform.SetParent(panelGO.transform);
            imageGOTwo.transform.SetParent(panelGO.transform);

            var panelLE = panelGO.GetComponent<LayoutElement>();
            panelLE.preferredWidth = 100f;
            panelLE.preferredHeight = 100f;

            var imageOneLE = imageGOOne.GetComponent<LayoutElement>();
            imageOneLE.preferredWidth = 100f;
            imageOneLE.preferredHeight = 100f;

            var imageTwoLE = imageGOOne.GetComponent<LayoutElement>();
            imageTwoLE.preferredWidth = 100f;
            imageTwoLE.preferredHeight = 100f;

            // Duplicate the first panel we created.
            GameObject.Instantiate(panelGO, canvasGO.transform);

            new GameObject("GameObject", typeof(SceneWithNestedLayoutElementsLoadScript));
        };
        CreateSceneUtility.CreateScene(layoutGroupSceneName, layoutGroupSceneCreation);
#endif
    }

    [UnityTest]
    public IEnumerator SceneWithNestedAspectRatioFitterLoads()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(aspectRatioFitterSceneName, LoadSceneMode.Additive);
        yield return operation;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(aspectRatioFitterSceneName));
        var go = GameObject.Find("GameObject");
        var component = go.GetComponent<SceneWithNestedLayoutElementsLoadScript>();

        yield return new WaitUntil(() => component.isStartCalled);

        operation = SceneManager.UnloadSceneAsync(aspectRatioFitterSceneName);
        yield return operation;
    }

    [UnityTest]
    public IEnumerator SceneWithNestedContentSizeFitterLoads()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(contentSizeFitterSceneName, LoadSceneMode.Additive);
        yield return operation;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(contentSizeFitterSceneName));
        var go = GameObject.Find("GameObject");
        var component = go.GetComponent<SceneWithNestedLayoutElementsLoadScript>();

        yield return new WaitUntil(() => component.isStartCalled);

        operation =  SceneManager.UnloadSceneAsync(contentSizeFitterSceneName);
        yield return operation;
    }

    [UnityTest]
    public IEnumerator SceneWithNestedLayoutGroupLoads()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(layoutGroupSceneName, LoadSceneMode.Additive);
        yield return operation;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(layoutGroupSceneName));
        var go = GameObject.Find("GameObject");
        var component = go.GetComponent<SceneWithNestedLayoutElementsLoadScript>();

        yield return new WaitUntil(() => component.isStartCalled);

        operation = SceneManager.UnloadSceneAsync(layoutGroupSceneName);
        yield return operation;
    }

    [SetUp]
    public void TestSetup()
    {
        m_InitScene = SceneManager.GetActiveScene();
    }

    [TearDown]
    public void TearDown()
    {
        SceneManager.SetActiveScene(m_InitScene);
    }

    [OneTimeTearDown]
    public void OnTimeTearDown()
    {
        //Manually add Assets/ and .unity as CreateSceneUtility does that for you.
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset("Assets/" + aspectRatioFitterSceneName + ".unity");
        AssetDatabase.DeleteAsset("Assets/" + contentSizeFitterSceneName + ".unity");
        AssetDatabase.DeleteAsset("Assets/" + layoutGroupSceneName + ".unity");
#endif
    }
}
