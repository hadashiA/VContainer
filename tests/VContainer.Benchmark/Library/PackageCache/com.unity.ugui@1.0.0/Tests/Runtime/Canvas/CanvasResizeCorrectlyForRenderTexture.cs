using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEditor;

[TestFixture]
public class CanvasResizeCorrectlyForRenderTexture
{
    Canvas m_Canvas;
    Camera m_Camera;
    RenderTexture m_RenderTexture;

    [Test]
    public void CanvasResizeCorrectly()
    {
        var cameraGameObject = new GameObject("PreviewCamera", typeof(Camera));
        var canvasGameObject = new GameObject("Canvas", typeof(Canvas));
        m_Camera = cameraGameObject.GetComponent<Camera>();
        m_Canvas = canvasGameObject.GetComponent<Canvas>();

        m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        m_Canvas.worldCamera = m_Camera;
        var rectTransform = canvasGameObject.transform as RectTransform;
        m_Canvas.updateRectTransformForStandalone = StandaloneRenderResize.Disabled;
        m_RenderTexture = new RenderTexture(100, 100, 0);
        m_Camera.targetTexture = m_RenderTexture;

        m_Camera.Render();

        Assert.AreNotEqual(new Vector2(100, 100), rectTransform.sizeDelta);

        m_Canvas.updateRectTransformForStandalone = StandaloneRenderResize.Enabled;
        m_Camera.Render();
        Assert.AreEqual(new Vector2(100, 100), rectTransform.sizeDelta);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_Canvas.gameObject);
        GameObject.DestroyImmediate(m_Camera.gameObject);
        UnityEngine.Object.DestroyImmediate(m_RenderTexture);
    }
}
