using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEditor;

public class NestedCanvas : IPrebuildSetup
{
    Object m_GO1;
    Object m_GO2;

    const string kPrefabPath = "Assets/Resources/NestedCanvasPrefab.prefab";

    public void Setup()
    {
#if UNITY_EDITOR

        var rootGO = new GameObject("RootGO");

        var rootCanvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasGroup));
        rootCanvasGO.transform.SetParent(rootGO.transform);

        var nestedCanvas = new GameObject("Nested Canvas", typeof(Canvas), typeof(Image));
        nestedCanvas.transform.SetParent(rootCanvasGO.transform);

        var nestedCanvasCamera = new GameObject("Nested Canvas Camera", typeof(Camera));
        nestedCanvasCamera.transform.SetParent(rootCanvasGO.transform);

        var rootCanvas = rootCanvasGO.GetComponent<Canvas>();
        rootCanvas.renderMode = RenderMode.WorldSpace;
        rootCanvas.worldCamera = nestedCanvasCamera.GetComponent<Camera>();

        if (!Directory.Exists("Assets/Resources/"))
            Directory.CreateDirectory("Assets/Resources/");

        UnityEditor.PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
        GameObject.DestroyImmediate(rootGO);
#endif
    }

    [UnityTest]
    [Description("[UI] Button does not interact after nested canvas is used(case 892913)")]
    public IEnumerator WorldCanvas_CanFindCameraAfterDisablingAndEnablingRootCanvas()
    {
        m_GO1 = Object.Instantiate(Resources.Load("NestedCanvasPrefab"));
        yield return null;

        var nestedCanvasGo = GameObject.Find("Nested Canvas");
        var nestedCanvas = nestedCanvasGo.GetComponent<Canvas>();
        Assert.IsNotNull(nestedCanvas.worldCamera, "Expected the nested canvas worldCamera to NOT be null after loading the scene.");

        nestedCanvasGo.transform.parent.gameObject.SetActive(false);
        nestedCanvasGo.transform.parent.gameObject.SetActive(true);
        Assert.IsNotNull(nestedCanvas.worldCamera, "Expected the nested canvas worldCamera to NOT be null after the parent canvas has been re-enabled.");
    }

    [UnityTest]
    public IEnumerator WorldCanvas_CanFindTheSameCameraAfterDisablingAndEnablingRootCanvas()
    {
        m_GO2 = Object.Instantiate(Resources.Load("NestedCanvasPrefab"));
        yield return null;

        var nestedCanvasGo = GameObject.Find("Nested Canvas");
        var nestedCanvas = nestedCanvasGo.GetComponent<Canvas>();
        var worldCamera = nestedCanvas.worldCamera;
        nestedCanvasGo.transform.parent.gameObject.SetActive(false);
        nestedCanvasGo.transform.parent.gameObject.SetActive(true);
        Assert.AreEqual(worldCamera, nestedCanvas.worldCamera, "Expected the same world camera to be returned after the root canvas was disabled and then re-enabled.");
    }

    [UnityTest]
    public IEnumerator NestedCanvasHasProperInheritedAlpha()
    {
        GameObject root = (GameObject)Object.Instantiate(Resources.Load("NestedCanvasPrefab"));
        CanvasGroup group = root.GetComponentInChildren<CanvasGroup>();
        Image image = root.GetComponentInChildren<Image>();

        group.alpha = 0.5f;

        yield return null;

        Assert.True(image.canvasRenderer.GetInheritedAlpha() == 0.5f);
        GameObject.DestroyImmediate(root);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_GO1);
        GameObject.DestroyImmediate(m_GO2);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(kPrefabPath);
#endif
    }
}
