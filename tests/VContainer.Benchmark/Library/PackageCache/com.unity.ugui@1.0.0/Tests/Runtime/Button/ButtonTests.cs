using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine;

public class ButtonTests : IPrebuildSetup
{
    GameObject m_PrefabRoot;
    const string kPrefabPath = "Assets/Resources/ButtonPrefab.prefab";

    public void Setup()
    {
#if UNITY_EDITOR
        var rootGO = new GameObject("rootGo");
        var canvasGO = new GameObject("Canvas", typeof(Canvas));
        canvasGO.transform.SetParent(rootGO.transform);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.referencePixelsPerUnit = 100;
        GameObject eventSystemGO = new GameObject("EventSystem", typeof(EventSystem));
        eventSystemGO.transform.SetParent(rootGO.transform);
        GameObject TestButtonGO = new GameObject("TestButton", typeof(RectTransform), typeof(TestButton));
        TestButtonGO.transform.SetParent(canvasGO.transform);

        if (!Directory.Exists("Assets/Resources/"))
            Directory.CreateDirectory("Assets/Resources/");

        PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
        GameObject.DestroyImmediate(rootGO);
#endif
    }

    [SetUp]
    public void TestSetup()
    {
        m_PrefabRoot = Object.Instantiate(Resources.Load("ButtonPrefab")) as GameObject;
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_PrefabRoot);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(kPrefabPath);
#endif
    }

    [Test]
    public void PressShouldCallClickHandler()
    {
        Button button = m_PrefabRoot.GetComponentInChildren<Button>();
        bool called = false;
        button.onClick.AddListener(() => { called = true; });
        button.OnPointerClick(new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>()) { button = PointerEventData.InputButton.Left });
        Assert.True(called);
    }

    [Test]
    public void PressInactiveShouldNotCallClickHandler()
    {
        Button button = m_PrefabRoot.GetComponentInChildren<Button>();
        bool called = false;
        button.enabled = false;
        button.onClick.AddListener(() => { called = true; });
        button.OnPointerClick(new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>()) { button = PointerEventData.InputButton.Left });
        Assert.False(called);
    }

    [Test]
    public void PressNotInteractableShouldNotCallClickHandler()
    {
        Button button = m_PrefabRoot.GetComponentInChildren<Button>();
        bool called = false;
        button.interactable = false;
        button.onClick.AddListener(() => { called = true; });
        button.OnPointerClick(new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>()) { button = PointerEventData.InputButton.Left });
        Assert.False(called);
    }

    [Test]
    public void SelectShouldHoldThePreviousStateAfterDisablingAndEnabling()
    {
        TestButton button = m_PrefabRoot.GetComponentInChildren<TestButton>();
        button.onClick.AddListener(() => {
            button.Select();
            button.enabled = false;
        });
        button.OnPointerClick(new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>()) { button = PointerEventData.InputButton.Left });
        Assert.False(button.enabled, "Expected button to not be enabled");
        button.enabled = true;
        Assert.True(button.isStateSelected, "Expected selected state to be true");
    }

    [Test]
    public void SubmitShouldCallClickHandler()
    {
        Button button = m_PrefabRoot.GetComponentInChildren<Button>();
        bool called = false;
        button.onClick.AddListener(() => { called = true; });
        button.OnSubmit(null);
        Assert.True(called);
    }

    [Test]
    public void SubmitInactiveShouldNotCallClickHandler()
    {
        Button button = m_PrefabRoot.GetComponentInChildren<Button>();
        bool called = false;
        button.enabled = false;
        button.onClick.AddListener(() => { called = true; });
        button.OnSubmit(new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>()) { button = PointerEventData.InputButton.Left });
        Assert.False(called);
    }

    [Test]
    public void SubmitNotInteractableShouldNotCallClickHandler()
    {
        Button button = m_PrefabRoot.GetComponentInChildren<Button>();
        bool called = false;
        button.interactable = false;
        button.onClick.AddListener(() => { called = true; });
        button.OnSubmit(new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>()) { button = PointerEventData.InputButton.Left });
        Assert.False(called);
    }

    [UnityTest]
    public IEnumerator SubmitShouldTransitionToPressedStateAndBackToNormal()
    {
        TestButton button = m_PrefabRoot.GetComponentInChildren<TestButton>();
        Assert.True(button.IsTransitionToNormal(0));

        button.OnSubmit(null);
        Assert.True(button.isStateNormal);
        Assert.True(button.IsTransitionToPressed(1));
        yield return new WaitWhile(() => button.StateTransitionCount == 2);

        // 3rd transition back to normal should have started
        Assert.True(button.IsTransitionToNormal(2));
        yield return null;

        Assert.True(button.isStateNormal);
    }
}
