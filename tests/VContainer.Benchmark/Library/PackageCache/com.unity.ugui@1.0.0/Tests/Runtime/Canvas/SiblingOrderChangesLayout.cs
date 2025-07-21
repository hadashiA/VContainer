using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;

[TestFixture]
[Category("RegressionTest")]
[Description("Case 723062")]
public class SiblingOrderChangesLayout
{
    GameObject m_CanvasGO;
    GameObject m_ParentGO;
    GameObject m_Child1GO;
    GameObject m_Child2GO;

    [SetUp]
    public void TestSetup()
    {
        m_CanvasGO = new GameObject("Canvas", typeof(Canvas));
        m_ParentGO = new GameObject("ParentRenderer");
        m_Child1GO = CreateTextObject("ChildRenderer1");
        m_Child2GO = CreateTextObject("ChildRenderer2");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExecuteMenuItem("Window/General/Game");
#endif
    }

    [UnityTest]
    public IEnumerator ReorderingSiblingChangesLayout()
    {
        m_ParentGO.transform.SetParent(m_CanvasGO.transform);
        m_Child1GO.transform.SetParent(m_ParentGO.transform);
        m_Child2GO.transform.SetParent(m_ParentGO.transform);

        m_ParentGO.AddComponent<CanvasRenderer>();
        m_ParentGO.AddComponent<RectTransform>();
        m_ParentGO.AddComponent<VerticalLayoutGroup>();
        m_ParentGO.AddComponent<ContentSizeFitter>();

        yield return new WaitForEndOfFrame();

        Vector2 child1Pos = m_Child1GO.GetComponent<RectTransform>().anchoredPosition;
        Vector2 child2Pos = m_Child2GO.GetComponent<RectTransform>().anchoredPosition;

        Assert.That(child1Pos, Is.Not.EqualTo(child2Pos));
        Assert.That(m_Child1GO.GetComponent<CanvasRenderer>().hasMoved, Is.False, "CanvasRenderer.hasMoved should be false");

        m_Child2GO.transform.SetAsFirstSibling();
        Canvas.ForceUpdateCanvases();

        Assert.That(m_Child1GO.GetComponent<CanvasRenderer>().hasMoved, Is.True, "CanvasRenderer.hasMoved should be true");
        Vector2 newChild1Pos = m_Child1GO.GetComponent<RectTransform>().anchoredPosition;
        Vector2 newChild2Pos = m_Child2GO.GetComponent<RectTransform>().anchoredPosition;

        Assert.That(newChild1Pos, Is.EqualTo(child2Pos), "Child1 should have moved to Child2's position");
        Assert.That(newChild2Pos, Is.EqualTo(child1Pos), "Child2 should have moved to Child1's position");
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasGO);
    }

    // Factory method for creating UI text objects taken from the original bug repro scene:
    private GameObject CreateTextObject(string name)
    {
        GameObject outputTextGameObject = new GameObject("OutputContent", typeof(CanvasRenderer));

        RectTransform outputTextTransform = outputTextGameObject.AddComponent<RectTransform>();
        outputTextTransform.pivot = new Vector2(0.5f, 0);
        outputTextTransform.anchorMin = Vector2.zero;
        outputTextTransform.anchorMax = new Vector2(1, 0);
        outputTextTransform.anchoredPosition = Vector2.zero;
        outputTextTransform.sizeDelta = Vector2.zero;

        Text outputText = outputTextGameObject.AddComponent<Text>();
        outputText.text = "Hello World!";
        outputTextGameObject.name = name;
        return outputTextGameObject;
    }
}
