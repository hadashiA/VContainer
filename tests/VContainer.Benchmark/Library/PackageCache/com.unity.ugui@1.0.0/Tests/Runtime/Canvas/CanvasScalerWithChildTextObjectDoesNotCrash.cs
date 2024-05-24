using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;

[TestFixture]
[Category("RegressionTest")]
[Description("CoveredBugID = 734299")]
public class CanvasScalerWithChildTextObjectDoesNotCrash
{
    GameObject m_CanvasObject;

    [SetUp]
    public void TestSetup()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExecuteMenuItem("Window/General/Game");
#endif
    }

    [UnityTest]
    public IEnumerator CanvasScalerWithChildTextObjectWithTextFontDoesNotCrash()
    {
        //This adds a Canvas component as well
        m_CanvasObject = new GameObject("Canvas");
        var canvasScaler = m_CanvasObject.AddComponent<CanvasScaler>();
        m_CanvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;

        //the crash only reproduces if the text component is a child of the game object
        //that has the CanvasScaler component and if it has an actual font and text set
        var textObject = new GameObject("Text").AddComponent<UnityEngine.UI.Text>();
        textObject.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        textObject.text = "New Text";
        textObject.transform.SetParent(m_CanvasObject.transform);
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1020);

        //The crash happens when setting the referenceResolution to a small value
        canvasScaler.referenceResolution = new Vector2(9, 9);

        //We need to wait a few milliseconds for the crash to happen, otherwise Debug.Log will
        //get executed and the test will pass
        yield return new WaitForSeconds(0.1f);

        Assert.That(true);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasObject);
    }
}
