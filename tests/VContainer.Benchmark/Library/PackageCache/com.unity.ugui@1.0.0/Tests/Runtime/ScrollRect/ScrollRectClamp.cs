using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class ScrollRectClamp
{
    // Prefab has the following hierarchy:
    // - PrefabRoot
    //   - Canvas
    //     - Root
    //       - Scroll View
    //         - Content
    //         - Scrollbar
    GameObject m_PrefabRoot;
    RectTransform Root { get; set; }
    ScrollRect Scroll { get; set; }
    RectTransform ScrollTransform { get; set; }
    RectTransform Content { get; set; }

    float ScrollSizeY { get { return ScrollTransform.rect.size.y; } }
    float ContentSizeY { get { return Content.rect.size.y; } }

    [SetUp]
    public void Setup()
    {
        // We setup the ScrollRect so that it will vertically resize with the Root object, to simulate
        // a change in screen size
        m_PrefabRoot = new GameObject("ScrollRectClamp");

        GameObject CanvasGO = new GameObject("Canvas");
        CanvasGO.transform.SetParent(m_PrefabRoot.transform);

        GameObject RootGO = new GameObject("Root", typeof(RectTransform));
        RootGO.transform.SetParent(CanvasGO.transform);
        Root = RootGO.GetComponent<RectTransform>();
        Root.pivot = Root.anchorMin = Root.anchorMax = new Vector2(0.5f, 0.5f);
        Root.sizeDelta = new Vector2(150.0f, 200.0f);

        GameObject ScrollViewGO = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        ScrollViewGO.transform.SetParent(Root);
        Scroll = ScrollViewGO.GetComponent<ScrollRect>();
        ScrollTransform = ScrollViewGO.GetComponent<RectTransform>();
        Scroll.viewport = ScrollTransform;
        Scroll.movementType = ScrollRect.MovementType.Clamped;
        Scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

        ScrollTransform.pivot = ScrollTransform.anchorMax = new Vector2(0.0f, 1.0f);
        ScrollTransform.anchorMin = Vector2.zero;
        ScrollTransform.sizeDelta = new Vector2(150.0f, 0.0f);
        ScrollTransform.localPosition = Vector3.zero;

        GameObject ContentGO = new GameObject("Content", typeof(RectTransform));
        Content = ContentGO.GetComponent<RectTransform>();
        Content.SetParent(ScrollTransform);
        Scroll.content = Content;
        Content.pivot = Content.anchorMin = new Vector2(0.0f, 1.0f);
        Content.anchorMax = new Vector2(1.0f, 1.0f);
        Content.sizeDelta = new Vector2(0.0f, 300.0f);
        Content.anchoredPosition = Vector2.zero;

        GameObject ScrollbarGO = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        ScrollbarGO.transform.SetParent(ScrollTransform);
        Scroll.verticalScrollbar = ScrollbarGO.GetComponent<Scrollbar>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_PrefabRoot);
    }

    [UnityTest]
    public IEnumerator ScrollRect_CorrectClampOnResize()
    {
        Assert.IsNotNull(Scroll.verticalScrollbar);

        Scroll.verticalNormalizedPosition = 1.0f;
        yield return null;
        Assert.IsTrue(Mathf.Approximately(0.0f, Content.anchoredPosition.y));

        Scroll.verticalNormalizedPosition = 0.0f;
        yield return null;
        // The content is vertically bigger than the viewport.
        Assert.IsTrue(Mathf.Approximately(Content.anchoredPosition.y, ContentSizeY - ScrollSizeY));

        // Resizing the root will resize the viewport accordingly.
        Root.sizeDelta = new Vector2(150.0f, 300.0f);
        yield return null;
        // The content is vertically the same size as the viewport
        Assert.IsTrue(Mathf.Approximately(ContentSizeY, ScrollSizeY));
        Assert.False(Scroll.verticalScrollbar.gameObject.activeSelf);
        Assert.IsTrue(Mathf.Approximately(0.0f, Scroll.verticalNormalizedPosition));
        Assert.IsTrue(Mathf.Approximately(0.0f, Content.anchoredPosition.y));

        Root.sizeDelta = new Vector2(150.0f, 200.0f);
        yield return null;
        Assert.IsTrue(Mathf.Approximately(1.0f, Scroll.verticalNormalizedPosition));
    }
}
