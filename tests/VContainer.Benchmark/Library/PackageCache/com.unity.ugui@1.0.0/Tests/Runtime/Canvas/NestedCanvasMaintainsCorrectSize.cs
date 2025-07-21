using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

public class NestedCanvasMaintainsCorrectSize : IPrebuildSetup
{
    BridgeScriptForRetainingObjects m_BridgeComponent;

    public void Setup()
    {
#if UNITY_EDITOR
        var canvasGO = new GameObject("Canvas", typeof(Canvas));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        var nestedCanvasGO = new GameObject("NestedCanvas", typeof(Canvas));
        nestedCanvasGO.transform.SetParent(canvasGO.transform);

        var rectTransform = (RectTransform)nestedCanvasGO.transform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(-20f, -20f);

        var bridgeObject = GameObject.Find(BridgeScriptForRetainingObjects.bridgeObjectName) ?? new GameObject(BridgeScriptForRetainingObjects.bridgeObjectName);
        var component = bridgeObject.GetComponent<BridgeScriptForRetainingObjects>() ?? bridgeObject.AddComponent<BridgeScriptForRetainingObjects>();
        component.canvasGO = canvasGO;
        component.nestedCanvasGO = nestedCanvasGO;

        canvasGO.SetActive(false);
        nestedCanvasGO.SetActive(false);
#endif
    }

    [SetUp]
    public void TestSetup()
    {
        m_BridgeComponent = GameObject.Find(BridgeScriptForRetainingObjects.bridgeObjectName).GetComponent<BridgeScriptForRetainingObjects>();
        m_BridgeComponent.canvasGO.SetActive(true);
        m_BridgeComponent.nestedCanvasGO.SetActive(true);
    }

    [Test]
    public void NestedCanvasMaintainsCorrectSizeAtGameStart()
    {
        var rectTransform = (RectTransform)m_BridgeComponent.nestedCanvasGO.transform;
        Assert.That(rectTransform.anchoredPosition, Is.EqualTo(Vector2.zero));
        Assert.That(rectTransform.sizeDelta, Is.EqualTo(new Vector2(-20f, -20f)));
        Assert.That(rectTransform.anchorMin, Is.EqualTo(Vector2.zero));
        Assert.That(rectTransform.anchorMax, Is.EqualTo(Vector2.one));
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_BridgeComponent.canvasGO);
    }
}
