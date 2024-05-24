using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class RectMask2DCulling : TestBehaviourBase<UnityEngine.Canvas>
{
    [Test]
    public void CullFlagNotResetWhenReparented740604()
    {
        var noMaskGameObject = new GameObject("noMaskGO");
        noMaskGameObject.AddComponent<RectTransform>();

        var maskGameObject = new GameObject("MaskGO");
        var rectMask2D = maskGameObject.AddComponent<RectMask2D>();

        noMaskGameObject.transform.SetParent(m_TestObject.transform);
        maskGameObject.transform.SetParent(m_TestObject.transform);

        noMaskGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 800);
        maskGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 400);

        var imageGameObject = new GameObject("ImageGO");
        var image = imageGameObject.AddComponent<Image>();
        imageGameObject.transform.SetParent(maskGameObject.transform);

        imageGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

        // Start with image inside RectMask2D area so that it's no culled
        rectMask2D.PerformClipping();
        Assert.IsFalse(image.canvasRenderer.cull);

        // Move image outside of RectMask2D so that it is culled
        imageGameObject.GetComponent<RectTransform>().position = new Vector2(275, 275);
        rectMask2D.PerformClipping();
        Assert.IsTrue(image.canvasRenderer.cull);

        // Change parent to noMask so that it's unaffected by RectMask2D and isn't culled
        imageGameObject.transform.SetParent(noMaskGameObject.transform);
        rectMask2D.PerformClipping();
        Assert.IsFalse(image.canvasRenderer.cull);
    }
}
