using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

[TestFixture]
public class LayoutGroupArrangement
{
    const float k_LayoutDefaultSize = 100f;

    GameObject m_Canvas;
    GameObject m_LayoutGameObject;
    RectTransform m_Child1;
    RectTransform m_Child2;

    [SetUp]
    public void TestSetup()
    {
        m_Canvas = new GameObject("Canvas", typeof(Canvas));
        m_LayoutGameObject = new GameObject("LayoutGroup", typeof(RectTransform));
        m_LayoutGameObject.transform.SetParent(m_Canvas.transform, false);
        m_LayoutGameObject.GetComponent<RectTransform>().sizeDelta =
            new Vector2(k_LayoutDefaultSize, k_LayoutDefaultSize);

        m_Child1 = new GameObject("Child1").AddComponent<RectTransform>();
        m_Child1.SetParent(m_LayoutGameObject.transform, false);

        m_Child2 = new GameObject("Child2").AddComponent<RectTransform>();
        m_Child2.SetParent(m_LayoutGameObject.transform, false);
    }

    [UnityTest]
    [Category("RegressionTest")]
    [Description("Nested GameObjects without a Layout Element will effect a Layout Group's arrangement (case 1026779)")]
    public IEnumerator LayoutGroup_ShouldResizeChildren_AfterDisablingAndEnablingAnyChild(
        [Values(typeof(HorizontalLayoutGroup), typeof(VerticalLayoutGroup))]
        Type layoutGroup)
    {
        var layoutComponent = (HorizontalOrVerticalLayoutGroup)m_LayoutGameObject.AddComponent(layoutGroup);
        layoutComponent.childForceExpandHeight = true;
        layoutComponent.childForceExpandWidth = true;
        layoutComponent.childControlHeight = true;
        layoutComponent.childControlWidth = true;

        Assert.That(m_LayoutGameObject.GetComponent<RectTransform>().sizeDelta,
            Is.EqualTo(Vector2.one * k_LayoutDefaultSize),
            "Layout's size should not change after adding the " + layoutGroup.Name);

        yield return null;

        // HorizontalLayoutGroup will rearrange the children to have width equal to half the layout's width.
        var expectedWidth = layoutGroup == typeof(HorizontalLayoutGroup)
            ? k_LayoutDefaultSize / 2
            : k_LayoutDefaultSize;

        // VerticalLayoutGroup will rearrange the children to have height equal to half the layout's height.
        var expectedHeight = layoutGroup == typeof(VerticalLayoutGroup) ? k_LayoutDefaultSize / 2 : k_LayoutDefaultSize;

        Assert.That(m_Child1.sizeDelta.x, Is.EqualTo(expectedWidth),
            "Adding " + layoutGroup.Name + " did not resize the first child.");
        Assert.That(m_Child1.sizeDelta.y, Is.EqualTo(expectedHeight),
            "Adding " + layoutGroup.Name + " did not resize the first child.");
        Assert.That(m_Child2.sizeDelta, Is.EqualTo(m_Child1.sizeDelta), "Both children should be of the same size.");

        // Disable the second child and verify that the first child is resized to have size equal to the layout's size.
        m_Child2.gameObject.SetActive(false);

        yield return null;

        Assert.That(m_Child1.sizeDelta.x, Is.EqualTo(k_LayoutDefaultSize),
            layoutGroup.Name + " did not resize the first child after disabling the second child.");
        Assert.That(m_Child1.sizeDelta.y, Is.EqualTo(k_LayoutDefaultSize),
            layoutGroup.Name + " did not resize the first child after disabling the second child.");

        // Enable the second child and verify that the first child is resized to the expected width/height.
        m_Child2.gameObject.SetActive(true);

        yield return null;

        Assert.That(m_Child1.sizeDelta.x, Is.EqualTo(expectedWidth),
            layoutGroup.Name + " did not resize the first child after enabling the second child.");
        Assert.That(m_Child1.sizeDelta.y, Is.EqualTo(expectedHeight),
            layoutGroup.Name + " did not resize the first child after enabling the second child.");
        Assert.That(m_Child2.sizeDelta, Is.EqualTo(m_Child1.sizeDelta), "Both children should be of the same size.");
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_Canvas);
    }
}
