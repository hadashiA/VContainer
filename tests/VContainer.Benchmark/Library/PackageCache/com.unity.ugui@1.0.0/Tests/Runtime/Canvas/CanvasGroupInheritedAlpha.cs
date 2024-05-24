using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;

[TestFixture]
public class CanvasGroupTests
{
    GameObject m_CanvasObject;
    CanvasGroup m_CanvasGroup;

    CanvasRenderer m_ChildCanvasRenderer;
    CanvasGroup m_ChildCanvasGroup;

    CanvasRenderer m_GrandChildCanvasRenderer;
    CanvasGroup m_GrandChildCanvasGroup;

    GameObject m_CanvasTwoObject;
    CanvasGroup m_CanvasTwoGroup;

    CanvasRenderer m_ChildCanvasTwoRenderer;

    const float m_CanvasAlpha = 0.25f;
    const float m_ChildAlpha = 0.5f;
    const float m_GrandChildAlpha = 0.8f;

    [SetUp]
    public void TestSetup()
    {
        m_CanvasObject = new GameObject("Canvas", typeof(Canvas));
        m_CanvasGroup = m_CanvasObject.AddComponent<CanvasGroup>();
        m_CanvasGroup.alpha = m_CanvasAlpha;

        var childObject = new GameObject("Child Object", typeof(Image));
        childObject.transform.SetParent(m_CanvasObject.transform);
        m_ChildCanvasGroup = childObject.AddComponent<CanvasGroup>();
        m_ChildCanvasGroup.alpha = m_ChildAlpha;
        m_ChildCanvasRenderer = childObject.GetComponent<CanvasRenderer>();

        var grandChildObject = new GameObject("Grand Child Object", typeof(Image));
        grandChildObject.transform.SetParent(childObject.transform);
        m_GrandChildCanvasGroup = grandChildObject.AddComponent<CanvasGroup>();
        m_GrandChildCanvasGroup.alpha = m_GrandChildAlpha;
        m_GrandChildCanvasRenderer = grandChildObject.GetComponent<CanvasRenderer>();

        m_CanvasTwoObject = new GameObject("CanvasTwo", typeof(Canvas));
        m_CanvasTwoObject.transform.SetParent(m_CanvasObject.transform);
        m_CanvasTwoGroup = m_CanvasTwoObject.AddComponent<CanvasGroup>();
        m_CanvasTwoGroup.alpha = m_CanvasAlpha;

        var childTwoObject = new GameObject("Child Two Object", typeof(Image));
        childTwoObject.transform.SetParent(m_CanvasTwoObject.transform);
        m_ChildCanvasTwoRenderer = childTwoObject.GetComponent<CanvasRenderer>();
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasObject);
    }

    private void SetUpCanvasGroupState()
    {
        m_CanvasGroup.enabled = false;
        m_CanvasGroup.ignoreParentGroups = false;
        m_ChildCanvasGroup.enabled = false;
        m_ChildCanvasGroup.ignoreParentGroups = false;
        m_GrandChildCanvasGroup.enabled = false;
        m_GrandChildCanvasGroup.ignoreParentGroups = false;

        m_CanvasTwoGroup.enabled = false;
        m_CanvasTwoGroup.ignoreParentGroups = false;
    }

    [Test]
    public void EnabledCanvasGroupEffectSelfAndChildrenAlpha()
    {
        // Set up the states of the canvas groups for the tests.
        SetUpCanvasGroupState();

        // With no enabled CanvasGroup the Alphas should be 1
        Assert.AreEqual(1.0f, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(1.0f, m_GrandChildCanvasRenderer.GetInheritedAlpha());

        // Enable the child CanvasGroup. It and its children should now have the same alpha value.
        m_ChildCanvasGroup.enabled = true;

        Assert.AreEqual(m_ChildAlpha, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(m_ChildAlpha, m_GrandChildCanvasRenderer.GetInheritedAlpha());
    }

    [Test]
    public void EnabledCanvasGroupOnACanvasEffectAllChildrenAlpha()
    {
        // Set up the states of the canvas groups for the tests.
        SetUpCanvasGroupState();

        // With no enabled CanvasGroup the Alphas should be 1
        Assert.AreEqual(1.0f, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(1.0f, m_GrandChildCanvasRenderer.GetInheritedAlpha());

        // Children under a different nest canvas should also obey the alpha
        Assert.AreEqual(1.0f, m_ChildCanvasTwoRenderer.GetInheritedAlpha());

        // Enable the Canvas CanvasGroup. All of the Canvas children should now have the same alpha value.
        m_CanvasGroup.enabled = true;

        Assert.AreEqual(m_CanvasAlpha, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(m_CanvasAlpha, m_GrandChildCanvasRenderer.GetInheritedAlpha());

        // Children under a different nest canvas should also obey the alpha
        Assert.AreEqual(m_CanvasAlpha, m_ChildCanvasTwoRenderer.GetInheritedAlpha());
    }

    [Test]
    public void EnabledCanvasGroupOnLeafChildEffectOnlyThatChild()
    {
        // Set up the states of the canvas groups for the tests.
        SetUpCanvasGroupState();

        // With no enabled CanvasGroup the Alphas should be 1
        Assert.AreEqual(1.0f, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(1.0f, m_GrandChildCanvasRenderer.GetInheritedAlpha());

        // Enable the Leaf child CanvasGroup. Only it should have a modified alpha
        m_GrandChildCanvasGroup.enabled = true;

        Assert.AreEqual(1.0f, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(m_GrandChildAlpha, m_GrandChildCanvasRenderer.GetInheritedAlpha());
    }

    [Test]
    public void EnabledCanvasGroupOnCanvasAndChildMultipleAlphaValuesCorrectly()
    {
        // Set up the states of the canvas groups for the tests.
        SetUpCanvasGroupState();

        // With no enabled CanvasGroup the Alphas should be 1
        Assert.AreEqual(1.0f, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(1.0f, m_GrandChildCanvasRenderer.GetInheritedAlpha());

        // Enable the Canvas CanvasGroup. All of the Canvas children should now have the same alpha value.
        m_CanvasGroup.enabled = true;
        m_ChildCanvasGroup.enabled = true;
        Assert.AreEqual(m_CanvasAlpha * m_ChildAlpha, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(m_CanvasAlpha * m_ChildAlpha, m_GrandChildCanvasRenderer.GetInheritedAlpha());
    }

    [Test]
    public void EnabledCanvasGroupOnCanvasAndChildWithChildIgnoringParentGroupMultipleAlphaValuesCorrectly()
    {
        // Set up the states of the canvas groups for the tests.
        SetUpCanvasGroupState();

        // With no enabled CanvasGroup the Alphas should be 1
        Assert.AreEqual(1.0f, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(1.0f, m_GrandChildCanvasRenderer.GetInheritedAlpha());

        // Enable the Canvas CanvasGroup. All of the Canvas children should now have the same alpha value.
        m_CanvasGroup.enabled = true;
        m_ChildCanvasGroup.enabled = true;
        m_ChildCanvasGroup.ignoreParentGroups = true;
        Assert.AreEqual(m_ChildAlpha, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(m_ChildAlpha, m_GrandChildCanvasRenderer.GetInheritedAlpha());
    }

    [Test]
    public void EnabledCanvasGroupOnCanvasAndChildrenWithAllChildrenIgnoringParentGroupMultipleAlphaValuesCorrectly()
    {
        // Set up the states of the canvas groups for the tests.
        SetUpCanvasGroupState();

        // With no enabled CanvasGroup the Alphas should be 1
        Assert.AreEqual(1.0f, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(1.0f, m_GrandChildCanvasRenderer.GetInheritedAlpha());

        // Enable the Canvas CanvasGroup. All of the Canvas children should now have the same alpha value.
        m_CanvasGroup.enabled = true;
        m_ChildCanvasGroup.enabled = true;
        m_GrandChildCanvasGroup.enabled = true;
        m_ChildCanvasGroup.ignoreParentGroups = true;
        m_GrandChildCanvasGroup.ignoreParentGroups = true;
        Assert.AreEqual(m_ChildAlpha, m_ChildCanvasRenderer.GetInheritedAlpha());
        Assert.AreEqual(m_GrandChildAlpha, m_GrandChildCanvasRenderer.GetInheritedAlpha());
    }

    [Test]
    public void EnabledCanvasGroupOnNestedCanvasIgnoringParentGroupMultipleAlphaValuesCorrectly()
    {
        // Set up the states of the canvas groups for the tests.
        SetUpCanvasGroupState();

        // With no enabled CanvasGroup the Alphas should be 1
        Assert.AreEqual(1.0f, m_ChildCanvasTwoRenderer.GetInheritedAlpha());

        // Enable the Canvas CanvasGroup. All of the Canvas children should now have the same alpha value.
        m_CanvasGroup.enabled = true;
        m_CanvasTwoGroup.enabled = true;
        m_CanvasTwoGroup.ignoreParentGroups = true;
        Assert.AreEqual(m_CanvasAlpha, m_ChildCanvasTwoRenderer.GetInheritedAlpha());
    }
}
