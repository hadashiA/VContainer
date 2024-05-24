using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.EventSystems;
using UnityEditor;

public class ScrollBarClamp : IPrebuildSetup
{
    // This test tests that setting scrollBar.value will not be clamped (case 802330 - Scrollbar stops velocity of 'Scroll Rect' unexpectedly)
    GameObject m_PrefabRoot;
    GameObject m_CameraGO;

    const string kPrefabPath = "Assets/Resources/ScrollBarClampPrefab.prefab";

    public void Setup()
    {
#if UNITY_EDITOR
        var rootGO = new GameObject("RootGO");
        var rootCanvasGO = new GameObject("Canvas", typeof(Canvas));
        var canvas = rootCanvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvasGO.transform.SetParent(rootGO.transform);

        var scrollRectGo = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
        var scrollRectTransform = scrollRectGo.GetComponent<RectTransform>();
        scrollRectTransform.SetParent(rootCanvasGO.transform);
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.sizeDelta = Vector2.zero;
        scrollRectTransform.anchoredPosition = Vector2.zero;
        var scrollRect = scrollRectGo.GetComponent<ScrollRect>();
        scrollRect.movementType = ScrollRect.MovementType.Elastic;

        var scrollbarGo = new GameObject("Scrollbar", typeof(RectTransform), typeof(Scrollbar));
        var scrollbarTransform = scrollbarGo.GetComponent<RectTransform>();
        scrollbarTransform.SetParent(scrollRectTransform);
        scrollbarTransform.anchorMin = new Vector2(1, 0);
        scrollbarTransform.anchorMax = new Vector2(1, 1);
        scrollbarTransform.anchoredPosition = Vector2.zero;
        scrollbarTransform.pivot = new Vector2(1, 0.5f);
        scrollbarTransform.sizeDelta = new Vector2(20, 0);
        var scrollbar = scrollbarGo.GetComponent<Scrollbar>();

        scrollRect.verticalScrollbar = scrollbar;

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        var contentTransform = contentGo.GetComponent<RectTransform>();
        contentTransform.SetParent(scrollRectTransform);
        contentTransform.anchorMin = new Vector2(0, 1);
        contentTransform.anchorMax = new Vector2(1, 1);
        contentTransform.anchoredPosition = Vector2.zero;
        contentTransform.pivot = new Vector2(0.5f, 1);
        contentTransform.sizeDelta = new Vector2(0, 1135);

        scrollRect.content = contentTransform;

        var layoutGroup = contentGo.GetComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(10, 10, 0, 0);
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = false;

        for (int i = 0; i < 20; i++)
        {
            var item = new GameObject("Item" + i, typeof(RectTransform), typeof(LayoutElement));
            var itemTransform = item.GetComponent<RectTransform>();
            itemTransform.pivot = new Vector2(0.5f, 1);
            itemTransform.SetParent(contentTransform);
            var layoutElement = item.GetComponent<LayoutElement>();
            layoutElement.minWidth = 620;
            layoutElement.minHeight = 100;
            layoutElement.preferredWidth = 620;
            layoutElement.preferredHeight = 100;
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
        m_PrefabRoot = Object.Instantiate(Resources.Load("ScrollBarClampPrefab")) as GameObject;
        m_CameraGO = new GameObject("Camera", typeof(Camera));
        Canvas.ForceUpdateCanvases();
    }

    //tests that setting the scrollbar value will not be clamped, but the private scrollbar functions will be clamped
    [Test]
    public void Scrollbar_clamp()
    {
        var scrollBar = m_PrefabRoot.GetComponentInChildren<Scrollbar>();
        scrollBar.value = 1.5f;
        Assert.Greater(scrollBar.value, 1f);
        scrollBar.value = -0.5f;
        Assert.Less(scrollBar.value, 0f);
        MethodInfo method = typeof(Scrollbar).GetMethod("DoUpdateDrag", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(scrollBar, new object[] { new Vector2(1.5f, 1.5f), 1f });
        Assert.LessOrEqual(scrollBar.value, 1f);
        method.Invoke(scrollBar, new object[] { new Vector2(-0.5f, -0.5f), 1f });
        Assert.GreaterOrEqual(scrollBar.value, 0f);
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
