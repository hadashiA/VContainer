using UnityEngine;
using NUnit.Framework;
using UnityEngine.UI;

[TestFixture]
[Category("RegressionTest")]
[Description("CoveredBugID = 913932")]
public class CanvasWidthAssertionErrorWithRectTransform
{
    GameObject m_CanvasMaster;
    GameObject m_CanvasChild;

    [SetUp]
    public void TestSetup()
    {
        m_CanvasMaster = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        m_CanvasChild = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
    }

    [Test]
    public void CanvasWidthAssertionErrorCheckOnModifyingRectTransform()
    {
        // Creating canvas and child canvas
        m_CanvasChild.transform.SetParent(m_CanvasMaster.transform);

        // Getting the rect Transform and modifying it
        RectTransform rt = m_CanvasChild.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);

        rt.offsetMin = new Vector2(rt.offsetMin.x, 1000);
        rt.offsetMax = new Vector2(rt.offsetMax.x, 200);

        rt.offsetMin = new Vector2(rt.offsetMin.y, 1);
        rt.offsetMax = new Vector2(rt.offsetMax.y, 0);

        //Assertion failed: Assertion failed on expression: 'width >= 0 should not happen
        Assert.Pass();
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasMaster);
    }
}
