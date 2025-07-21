using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Graphics
{
    [Category("RegressionTest")]
    [Description("CoveredBugID = 1395695, CoveredBugDescription = \"RectMask2D hides all content when parented from other display to first dislpay in the Game view window\"")]
    public class RectMask2DReparentedToDifferentCanvas
    {
        GameObject m_GameObjectA;
        GameObject m_GameObjectB;
        Canvas m_CanvasA;
        Canvas m_CanvasB;
        RectMask2D m_Mask;

        [SetUp]
        public void TestSetup()
        {
            m_GameObjectA = new GameObject("Canvas A");
            m_GameObjectB = new GameObject("Canvas B");
            m_CanvasA = m_GameObjectA.AddComponent<Canvas>();
            m_CanvasB = m_GameObjectB.AddComponent<Canvas>();

            var rectMaskGameObject = new GameObject("RectMask2D");
            m_Mask = rectMaskGameObject.AddComponent<RectMask2D>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Mask.gameObject);
            Object.DestroyImmediate(m_GameObjectA);
            Object.DestroyImmediate(m_GameObjectB);
        }

        [Test]
        public void ReparentingRectMask2D_UpdatesCanvas()
        {
            m_Mask.transform.SetParent(m_GameObjectA.transform);
            Assert.AreSame(m_CanvasA, m_Mask.Canvas);

            m_Mask.transform.SetParent(m_GameObjectB.transform);
            Assert.AreSame(m_CanvasB, m_Mask.Canvas, "Expected Canvas to be updated after parent changed.");
        }
    }
}
