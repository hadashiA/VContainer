using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEditor;

/*
 This test checks if a clamped scrollRect with a content rect that has the same width and a canvas with scaling will stay stable for 5 frames.
 Test for case (1010178-Clamped ScrollRect with scalling cause a large spike in performance)
*/

public class ScrollRectStableLayout : IPrebuildSetup
{
    GameObject m_PrefabRoot;
    GameObject m_CameraGO;

    const string kPrefabPath = "Assets/Resources/ScrollRectStableLayoutPrefab.prefab";

    public void Setup()
    {
#if UNITY_EDITOR
        var rootGO = new GameObject("RootGO");
        var rootCanvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
        rootCanvasGO.transform.SetParent(rootGO.transform);
        var canvas = rootCanvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var canvasScaler = rootCanvasGO.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(3840, 2160);
        canvasScaler.referencePixelsPerUnit = 128;

        var scrollRectGo = new GameObject("Scroll View", typeof(ScrollRect));
        var scrollRectTransform = scrollRectGo.GetComponent<RectTransform>();
        scrollRectTransform.SetParent(rootCanvasGO.transform);
        scrollRectTransform.localPosition = Vector3.zero;
        scrollRectTransform.sizeDelta = new Vector2(1000, 1000);
        scrollRectTransform.localScale = Vector3.one * 1.000005f;
        var scrollRect = scrollRectGo.GetComponent<ScrollRect>();
        scrollRect.horizontal = true;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;

        var viewportTransform = new GameObject("Viewport", typeof(RectTransform)).GetComponent<RectTransform>();
        viewportTransform.SetParent(scrollRectTransform);
        viewportTransform.localPosition = Vector3.zero;
        viewportTransform.localScale = Vector3.one;
        viewportTransform.sizeDelta = new Vector2(-17, 0);
        viewportTransform.anchorMin = new Vector2(0, 0);
        viewportTransform.anchorMax = new Vector2(1, 1);
        viewportTransform.pivot = new Vector2(0, 1);
        scrollRect.viewport = viewportTransform;

        var contentGo = new GameObject("Content", typeof(GridLayoutGroup));
        var contentTransform = contentGo.GetComponent<RectTransform>();
        contentTransform.SetParent(viewportTransform);
        contentTransform.localPosition = Vector3.zero;
        contentTransform.localScale = Vector3.one;
        contentTransform.sizeDelta = new Vector2(0, 300);
        contentTransform.anchorMin = new Vector2(0, 1);
        contentTransform.anchorMax = new Vector2(1, 1);
        contentTransform.pivot = new Vector2(0, 1);
        var gridLayout = contentGo.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(100, 100);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;
        scrollRect.content = contentTransform;

        for (int i = 0; i < 20; i++)
        {
            var go = new GameObject("GameObject (" + i + ")", typeof(RectTransform));
            var t = go.GetComponent<RectTransform>();
            t.SetParent(contentTransform);
            t.localScale = Vector3.one;
            t.pivot = new Vector2(0.5f, 0.5f);
        }

        if (!Directory.Exists("Assets/Resources/"))
            Directory.CreateDirectory("Assets/Resources/");

        UnityEditor.PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
        GameObject.DestroyImmediate(rootGO);
#endif
    }

    [SetUp]
    public void TestSetup()
    {
        m_PrefabRoot = Object.Instantiate(Resources.Load("ScrollRectStableLayoutPrefab")) as GameObject;
        m_CameraGO = new GameObject("Camera", typeof(Camera));
    }

    [UnityTest]
    public IEnumerator ScrollRect_StableWhenStatic()
    {
        //root->canvas->scroll view->viewport->content
        RectTransform t = m_PrefabRoot.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0) as RectTransform;
        float[] values = new float[5];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = t.localPosition.y;
            yield return null;
        }
        Assert.AreEqual(values[0], values[1]);
        Assert.AreEqual(values[1], values[2]);
        Assert.AreEqual(values[2], values[3]);
        Assert.AreEqual(values[3], values[4]);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_PrefabRoot);
        GameObject.DestroyImmediate(m_CameraGO);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(kPrefabPath);
#endif
    }
}
