using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

[Category("Canvas")]
public class RootCanvasTests : TestBehaviourBase<UnityEngine.Canvas>
{
    // A simple nested canvas hierarchy
    // m_TestObject
    //     └ rootCanvasChild
    //              └ emptyChildGameObject
    //                          └ baseCanvas
    private UnityEngine.Canvas rootCanvasChild;
    private GameObject emptyChildGameObject;
    private UnityEngine.Canvas baseCanvas;

    [SetUp]
    public override void TestSetup()
    {
        base.TestSetup();

        var rootChildGO = new GameObject("root child");
        rootCanvasChild = rootChildGO.AddComponent<Canvas>();

        emptyChildGameObject = new GameObject("empty");

        var baseGO = new GameObject("base");
        baseCanvas = baseGO.AddComponent<Canvas>();

        baseCanvas.transform.SetParent(emptyChildGameObject.transform);
        emptyChildGameObject.transform.SetParent(rootChildGO.transform);
        rootChildGO.transform.SetParent(m_TestObject.transform);
    }

    [Test]
    public void IsRootCanvasTest()
    {
        Assert.IsFalse(baseCanvas.isRootCanvas);
        Assert.IsFalse(rootCanvasChild.isRootCanvas);
        Assert.IsTrue(m_TestObject.isRootCanvas);
    }

    [Test]
    public void CorrectRootCanvasReturned()
    {
        Assert.AreEqual(m_TestObject, m_TestObject.rootCanvas);
        Assert.AreEqual(m_TestObject, rootCanvasChild.rootCanvas);
        Assert.AreEqual(m_TestObject, baseCanvas.rootCanvas);
    }

    [Test]
    public void NotRootCanvasAnchorsDontGetReset()
    {
        var rect = rootCanvasChild.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        Assert.IsTrue(rect.anchorMin == Vector2.zero);
        Assert.IsTrue(rect.anchorMax == Vector2.one);

        m_TestObject.gameObject.SetActive(false);

        Assert.IsTrue(rect.anchorMin == Vector2.zero);
        Assert.IsTrue(rect.anchorMax == Vector2.one);
    }

    [Test]
    public void ChildOfDisabledCanvasCantReceiveClicks()
    {
        rootCanvasChild.gameObject.AddComponent<Image>();
        var raycasts = GraphicRegistry.GetRaycastableGraphicsForCanvas(rootCanvasChild);

        Assert.IsTrue(raycasts.Count == 1);

        m_TestObject.gameObject.SetActive(false);
        raycasts = GraphicRegistry.GetRaycastableGraphicsForCanvas(rootCanvasChild);

        Assert.IsTrue(raycasts.Count == 0);
    }
}
