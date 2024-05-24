using System;
using System.Collections;
using System.Reflection;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using System.Runtime.CompilerServices;

public class ScrollRectTests : IPrebuildSetup
{
    const int ScrollSensitivity = 3;
    GameObject m_PrefabRoot;
    const string kPrefabPath = "Assets/Resources/ScrollRectPrefab.prefab";

    public void Setup()
    {
#if UNITY_EDITOR
        var rootGO = new GameObject("rootGo");
        GameObject eventSystemGO = new GameObject("EventSystem", typeof(EventSystem));
        eventSystemGO.transform.SetParent(rootGO.transform);

        var canvasGO = new GameObject("Canvas", typeof(Canvas));
        canvasGO.transform.SetParent(rootGO.transform);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.referencePixelsPerUnit = 100;

        GameObject scrollRectGO = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect));
        scrollRectGO.transform.SetParent(canvasGO.transform);

        GameObject contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(scrollRectGO.transform);

        GameObject horizontalScrollBarGO = new GameObject("HorizontalScrollBar", typeof(Scrollbar));
        horizontalScrollBarGO.transform.SetParent(scrollRectGO.transform);

        GameObject verticalScrollBarGO = new GameObject("VerticalScrollBar", typeof(Scrollbar));
        verticalScrollBarGO.transform.SetParent(scrollRectGO.transform);

        ScrollRect scrollRect = scrollRectGO.GetComponent<ScrollRect>();
        scrollRect.transform.position = Vector3.zero;
        scrollRect.transform.rotation = Quaternion.identity;
        scrollRect.transform.localScale = Vector3.one;
        (scrollRect.transform as RectTransform).sizeDelta = new Vector3(0.5f, 0.5f);
        scrollRect.horizontalScrollbar = horizontalScrollBarGO.GetComponent<Scrollbar>();
        scrollRect.verticalScrollbar = verticalScrollBarGO.GetComponent<Scrollbar>();
        scrollRect.content = contentGO.GetComponent<RectTransform>();
        scrollRect.content.anchoredPosition = Vector2.zero;
        scrollRect.content.sizeDelta = new Vector2(3, 3);
        scrollRect.scrollSensitivity = ScrollSensitivity;
        scrollRect.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);

        if (!Directory.Exists("Assets/Resources/"))
            Directory.CreateDirectory("Assets/Resources/");

        PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
        GameObject.DestroyImmediate(rootGO);
#endif
    }

    [SetUp]
    public void TestSetup()
    {
        m_PrefabRoot = UnityEngine.Object.Instantiate(Resources.Load("ScrollRectPrefab")) as GameObject;
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

    #region Enable disable scrollbars

    [UnityTest]
    [TestCase(true, ExpectedResult = null)]
    [TestCase(false, ExpectedResult = null)]
    public IEnumerator OnEnableShouldAddListeners(bool isHorizontal)
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        Scrollbar scrollbar = isHorizontal ? scrollRect.horizontalScrollbar : scrollRect.verticalScrollbar;

        scrollRect.enabled = false;

        yield return null;

        FieldInfo field = scrollbar.onValueChanged.GetType().BaseType.BaseType.GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
        object invokeableCallList = field.GetValue(scrollbar.onValueChanged);
        PropertyInfo property = invokeableCallList.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);

        int callCount = (int)property.GetValue(invokeableCallList, null);

        scrollRect.enabled = true;

        yield return null;
        Assert.AreEqual(callCount + 1, (int)property.GetValue(invokeableCallList, null));
    }

    [UnityTest]
    [TestCase(true, ExpectedResult = null)]
    [TestCase(false, ExpectedResult = null)]
    public IEnumerator OnDisableShouldRemoveListeners(bool isHorizontal)
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        Scrollbar scrollbar = isHorizontal ? scrollRect.horizontalScrollbar : scrollRect.verticalScrollbar;

        scrollRect.enabled = true;
        yield return null;

        FieldInfo field = scrollbar.onValueChanged.GetType().BaseType.BaseType.GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
        object invokeableCallList = field.GetValue(scrollbar.onValueChanged);
        PropertyInfo property = invokeableCallList.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);

        Assert.AreNotEqual(0, (int)property.GetValue(invokeableCallList, null));

        scrollRect.enabled = false;
        yield return null;
        Assert.AreEqual(0, (int)property.GetValue(invokeableCallList, null));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SettingScrollbarShouldRemoveThenAddListeners(bool testHorizontal)
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        Scrollbar scrollbar = testHorizontal ? scrollRect.horizontalScrollbar : scrollRect.verticalScrollbar;

        GameObject scrollBarGO = new GameObject("scrollBar", typeof(RectTransform), typeof(Scrollbar));
        scrollBarGO.transform.SetParent(scrollRect.transform);
        Scrollbar newScrollbar = scrollBarGO.GetComponent<Scrollbar>();

        FieldInfo field = newScrollbar.onValueChanged.GetType().BaseType.BaseType.GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
        PropertyInfo property = field.GetValue(newScrollbar.onValueChanged).GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);

        int newCallCount = (int)property.GetValue(field.GetValue(newScrollbar.onValueChanged), null);
        if (testHorizontal)
            scrollRect.horizontalScrollbar = newScrollbar;
        else
            scrollRect.verticalScrollbar = newScrollbar;

        Assert.AreEqual(0, (int)property.GetValue(field.GetValue(scrollbar.onValueChanged), null), "The previous scrollbar should not have listeners anymore");
        Assert.AreEqual(newCallCount + 1, (int)property.GetValue(field.GetValue(newScrollbar.onValueChanged), null), "The new scrollbar should have listeners now");
    }

    #endregion

    #region Drag

    [Test]
    [TestCase(PointerEventData.InputButton.Left, true)]
    [TestCase(PointerEventData.InputButton.Right, false)]
    [TestCase(PointerEventData.InputButton.Middle, false)]
    public void PotentialDragNeedsLeftClick(PointerEventData.InputButton button, bool expectedEqualsZero)
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        scrollRect.velocity = Vector2.one;
        Assert.AreNotEqual(Vector2.zero, scrollRect.velocity);

        scrollRect.OnInitializePotentialDrag(new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>()) { button = button });
        if (expectedEqualsZero)
            Assert.AreEqual(Vector2.zero, scrollRect.velocity);
        else
            Assert.AreNotEqual(Vector2.zero, scrollRect.velocity);
    }

    [Test]
    [TestCase(PointerEventData.InputButton.Left, true, true)]
    [TestCase(PointerEventData.InputButton.Left, false, false)]
    [TestCase(PointerEventData.InputButton.Right, true, false)]
    [TestCase(PointerEventData.InputButton.Middle, true, false)]
    public void LeftClickShouldStartDrag(PointerEventData.InputButton button, bool active, bool expectedIsDragging)
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        scrollRect.enabled = active;

        scrollRect.velocity = Vector2.one;
        Assert.AreNotEqual(Vector2.zero, scrollRect.velocity);

        var pointerEventData = new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>()) { button = button };
        scrollRect.OnInitializePotentialDrag(pointerEventData);

        FieldInfo field = typeof(ScrollRect).GetField("m_Dragging", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.IsFalse((bool)field.GetValue(scrollRect));
        scrollRect.OnBeginDrag(pointerEventData);
        Assert.AreEqual(expectedIsDragging, (bool)field.GetValue(scrollRect));
    }

    [Test]
    [TestCase(PointerEventData.InputButton.Left, true, false)]
    [TestCase(PointerEventData.InputButton.Left, false, false)]
    [TestCase(PointerEventData.InputButton.Right, false, false)]
    [TestCase(PointerEventData.InputButton.Right, true, true)]
    [TestCase(PointerEventData.InputButton.Middle, true, true)]
    [TestCase(PointerEventData.InputButton.Middle, false, false)]
    public void LeftClickUpShouldEndDrag(PointerEventData.InputButton button, bool active, bool expectedIsDragging)
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        scrollRect.velocity = Vector2.one;
        Assert.AreNotEqual(Vector2.zero, scrollRect.velocity);

        var startDragEventData = new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>()) { button = PointerEventData.InputButton.Left };

        scrollRect.OnInitializePotentialDrag(startDragEventData);

        FieldInfo field = typeof(ScrollRect).GetField("m_Dragging", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.IsFalse((bool)field.GetValue(scrollRect));
        scrollRect.OnBeginDrag(startDragEventData);
        Assert.IsTrue((bool)field.GetValue(scrollRect), "Prerequisite: dragging should be true to test if it is set to false later");

        scrollRect.enabled = active;

        var endDragEventData = new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>()) { button = button };
        scrollRect.OnEndDrag(endDragEventData);
        Assert.AreEqual(expectedIsDragging, (bool)field.GetValue(scrollRect));
    }

    #endregion

    #region LateUpdate

    [UnityTest]
    public IEnumerator LateUpdateWithoutInertiaOrElasticShouldZeroVelocity()
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        scrollRect.velocity = Vector2.one;
        scrollRect.inertia = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        yield return null;

        Assert.AreEqual(Vector2.zero, scrollRect.velocity);
    }

    [UnityTest]
    public IEnumerator LateUpdateWithInertiaShouldDecelerate()
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        scrollRect.velocity = Vector2.one;
        scrollRect.inertia = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        yield return null;
        Assert.Less(scrollRect.velocity.magnitude, 1);
    }

    [UnityTest][Ignore("Fails")]
    public IEnumerator LateUpdateWithElasticShouldDecelerate()
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        scrollRect.velocity = Vector2.one;
        scrollRect.inertia = false;
        scrollRect.content.anchoredPosition = Vector2.one * 2;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;

        yield return null;
        Assert.AreNotEqual(1, scrollRect.velocity.magnitude);
        var newMagnitude = scrollRect.velocity.magnitude;

        yield return null;
        Assert.AreNotEqual(newMagnitude, scrollRect.velocity.magnitude);
    }

    [UnityTest]
    public IEnumerator LateUpdateWithElasticNoOffsetShouldZeroVelocity()
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        scrollRect.velocity = Vector2.one;
        scrollRect.inertia = false;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        yield return null;
        Assert.AreEqual(Vector2.zero, scrollRect.velocity);
    }

    #endregion

    [Test]
    public void SetNormalizedPositionShouldSetContentLocalPosition()
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        scrollRect.normalizedPosition = Vector2.one;
        Assert.AreEqual(new Vector3(-1f, -1f, 0), scrollRect.content.localPosition);
    }

    #region Scroll, offset, ...

    [Test]
    [TestCase(1, 1, true, true, 1, -1, TestName = "Horizontal and vertical scroll")]
    [TestCase(1, 1, false, true, 0, -1, TestName = "Vertical scroll")]
    [TestCase(1, 1, true, false, 1, 0, TestName = "Horizontal scroll")]
    public void OnScrollClampedShouldMoveContentAnchoredPosition(int scrollDeltaX, int scrollDeltaY, bool horizontal,
        bool vertical, int expectedPosX, int expectedPosY)
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        Vector2 scrollDelta = new Vector2(scrollDeltaX, scrollDeltaY);
        var expected = new Vector2(expectedPosX, expectedPosY) * ScrollSensitivity;

        scrollRect.horizontal = horizontal;
        scrollRect.vertical = vertical;

        scrollRect.OnScroll(new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>())
        {
            scrollDelta = scrollDelta
        });

        Assert.AreEqual(expected, scrollRect.content.anchoredPosition);
    }

    [Test][Ignore("Tests fail without mocking")]
    [TestCase(ScrollRect.MovementType.Clamped, 1f, 1f)]
    [TestCase(ScrollRect.MovementType.Unrestricted, 150, 150)]
    [TestCase(ScrollRect.MovementType.Elastic, 150, 150)]
    public void OnScrollClampedShouldClampContentAnchoredPosition(ScrollRect.MovementType movementType, float anchoredPosX,
        float anchoredPosY)
    {
        ScrollRect scrollRect = m_PrefabRoot.GetComponentInChildren<ScrollRect>();
        Vector2 scrollDelta = new Vector2(50, -50);
        scrollRect.movementType = movementType;
        scrollRect.content.anchoredPosition = new Vector2(2.5f, 2.5f);

        scrollRect.OnScroll(new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>())
        {
            scrollDelta = scrollDelta
        });
        Assert.AreEqual(new Vector2(anchoredPosX, anchoredPosY), scrollRect.content.anchoredPosition);
    }

    [Test]
    public void GetBoundsShouldEncapsulateAllCorners()
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        object[] arguments = new object[2]
        {
            new Vector3[] { Vector3.zero, Vector3.one, Vector3.one * 2, Vector3.one },
            matrix
        };
        MethodInfo method = typeof(ScrollRect).GetMethod("InternalGetBounds", BindingFlags.NonPublic | BindingFlags.Static);
        var bounds = (Bounds)method.Invoke(null, arguments);

        Assert.AreEqual(Vector3.zero, bounds.min);
        Assert.AreEqual(Vector3.one * 2, bounds.max);
    }

    [Test]
    public void UpdateBoundsShouldPad()
    {
        Bounds viewBounds = new Bounds(Vector3.zero, Vector3.one * 2);
        Vector3 contentSize = Vector3.one;
        Vector3 contentPos = Vector3.one;
        var contentPivot = new Vector2(0.5f, 0.5f);

        object[] arguments = new object[] { viewBounds, contentPivot, contentSize, contentPos };
        MethodInfo method = typeof(ScrollRect).GetMethod("AdjustBounds", BindingFlags.NonPublic | BindingFlags.Static);
        method.Invoke(null, arguments);

        //ScrollRect.AdjustBounds(ref viewBounds, ref contentPivot, ref contentSize, ref contentPos);
        Assert.AreEqual(new Vector3(2, 2, 1), arguments[2]);
    }

    [Test]
    [TestCase(true, true, 2, 4, -2, -2, TestName = "Should clamp offset")]
    [TestCase(false, true, 2, 4, 0, -2, TestName = "Vertical should clamp offset on one axis")]
    [TestCase(true, false, 2, 4, -2, 0, TestName = "Horizontal should clamp offset on one axis")]
    [TestCase(false, false, 2, 4, 0, 0, TestName = "No axis should not clamp offset")]
    [TestCase(true, true, 8, 10, 2, 2, TestName = "Should clamp negative offset")]
    [TestCase(false, true, 8, 10, 0, 2, TestName = "Vertical should clamp negative offset on one axis")]
    [TestCase(true, false, 8, 10, 2, 0, TestName = "Horizontal should clamp negative offset on one axis")]
    [TestCase(false, false, 8, 10, 0, 0, TestName = "No axis should not clamp negative offset")]
    public void CalculateOffsetShouldClamp(bool horizontal, bool vertical, int viewX, float viewY, float resX, float resY)
    {
        TestCalculateOffset(ScrollRect.MovementType.Clamped, horizontal, vertical, viewX, viewY, resX, resY, new Bounds(new Vector3(5, 7), new Vector3(4, 4)));
    }

    [Test]
    [TestCase(true, true, 2, 4, -2, -2, TestName = "Should clamp offset")]
    [TestCase(false, true, 2, 4, 0, -2, TestName = "Vertical should clamp offset on one axis")]
    [TestCase(true, false, 2, 4, -2, 0, TestName = "Horizontal should clamp offset on one axis")]
    [TestCase(false, false, 2, 4, 0, 0, TestName = "No axis should not clamp offset")]
    [TestCase(true, true, 8, 10, 2, 2, TestName = "Should clamp negative offset")]
    [TestCase(false, true, 8, 10, 0, 2, TestName = "Vertical should clamp negative offset on one axis")]
    [TestCase(true, false, 8, 10, 2, 0, TestName = "Horizontal should clamp negative offset on one axis")]
    [TestCase(false, false, 8, 10, 0, 0, TestName = "No axis should not clamp negative offset")]
    public void CalculateOffsetUnrestrictedShouldNotClamp(bool horizontal, bool vertical, int viewX, float viewY, float resX, float resY)
    {
        TestCalculateOffset(ScrollRect.MovementType.Unrestricted, horizontal, vertical, viewX, viewY, 0, 0, new Bounds(new Vector3(5, 7), new Vector3(4, 4)));
    }

    private static void TestCalculateOffset(ScrollRect.MovementType movementType, bool horizontal, bool vertical, int viewX, float viewY, float resX, float resY, Bounds contentBounds)
    {
        Bounds viewBounds = new Bounds(new Vector3(viewX, viewY), new Vector3(2, 2));
        // content is south east of view
        Vector2 delta = Vector2.zero;

        object[] arguments = new object[] { viewBounds, contentBounds, horizontal, vertical, movementType, delta };
        MethodInfo method = typeof(ScrollRect).GetMethod("InternalCalculateOffset", BindingFlags.NonPublic | BindingFlags.Static);
        var result = (Vector2)method.Invoke(null, arguments);

        Console.WriteLine(result);
        Assert.AreEqual(new Vector2(resX, resY), result);
    }

    #endregion
}
