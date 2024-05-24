using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;

namespace Graphics
{
    [TestFixture]
    [Category("RegressionTest")]
    [Description(
        "CoveredBugID = 782957, CoveredBugDescription = \"Some element from scroll view are invisible when they're masked with RectMask2D and sub-canvases\"")]
    public class RectMask2DWithNestedCanvasCullsUsingCorrectCanvasRect
    {
        GameObject m_RootCanvasGO;
        GameObject m_MaskGO;
        GameObject m_ImageGO;

        [SetUp]
        public void TestSetup()
        {
            m_RootCanvasGO = new GameObject("Canvas");
            m_MaskGO = new GameObject("Mask", typeof(RectMask2D));
            m_ImageGO = new GameObject("Image");
        }

        [UnityTest]
        public IEnumerator RectMask2DShouldNotCullImagesWithCanvas()
        {
            //Root Canvas
            var canvas = m_RootCanvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Rectmaskk2D
            var maskRect = m_MaskGO.GetComponent<RectTransform>();
            maskRect.sizeDelta = new Vector2(200, 200);

            // Our image that will be in the RectMask2D
            var image = m_ImageGO.AddComponent<Image>();
            var imageRenderer = m_ImageGO.GetComponent<CanvasRenderer>();
            var imageRect = m_ImageGO.GetComponent<RectTransform>();
            m_ImageGO.AddComponent<Canvas>();
            imageRect.sizeDelta = new Vector2(10, 10);

            m_MaskGO.transform.SetParent(canvas.transform);
            image.transform.SetParent(m_MaskGO.transform);
            imageRect.position = maskRect.position = Vector3.zero;

            yield return new WaitForSeconds(0.1f);

            Assert.That(imageRenderer.cull, Is.False,
                "Expected image(with canvas) to not be culled by the RectMask2D but it was.");
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_RootCanvasGO);
            GameObject.DestroyImmediate(m_MaskGO);
            GameObject.DestroyImmediate(m_ImageGO);
        }
    }
}
