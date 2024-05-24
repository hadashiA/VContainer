using NUnit.Framework;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.IO;
using UnityEditor;

class GridLayoutGroupTests : IPrebuildSetup
{
    const string kPrefabPath = "Assets/Resources/GridLayoutGroupTests.prefab";
    private GameObject m_PrefabRoot;
    private GridLayoutGroup m_LayoutGroup;

    public void Setup()
    {
#if UNITY_EDITOR
        var rootGO = new GameObject("rootGo");

        var canvasGO = new GameObject("Canvas");
        canvasGO.transform.SetParent(rootGO.transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.referencePixelsPerUnit = 100;

        var groupGO = new GameObject("Group", typeof(RectTransform), typeof(GridLayoutGroup));
        groupGO.transform.SetParent(canvas.transform);

        var rectTransform = groupGO.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400, 500);
        rectTransform.anchoredPosition = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0, 0);

        var layoutGroup = groupGO.GetComponent<GridLayoutGroup>();
        layoutGroup.spacing = new Vector2(10, 0);
        layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layoutGroup.cellSize = new Vector2(90, 50);
        layoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
        layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.enabled = false;
        layoutGroup.enabled = true;

        var el1 = new GameObject("Element1");
        el1.transform.SetParent(rectTransform);
        var element1 = el1.AddComponent<LayoutElement>();
        (el1.transform as RectTransform).pivot = Vector2.zero;
        element1.minWidth = 5;
        element1.minHeight = 10;
        element1.preferredWidth = 100;
        element1.preferredHeight = 50;
        element1.flexibleWidth = 0;
        element1.flexibleHeight = 0;
        element1.enabled = true;

        var el2 = new GameObject("Element2");
        el2.transform.SetParent(rectTransform);
        var element2 = el2.AddComponent<LayoutElement>();
        (el2.transform as RectTransform).pivot = Vector2.zero;
        element2.minWidth = 10;
        element2.minHeight = 5;
        element2.preferredWidth = -1;
        element2.preferredHeight = -1;
        element2.flexibleWidth = 0;
        element2.flexibleHeight = 0;
        element2.enabled = true;

        var el3 = new GameObject("Element3");
        el3.transform.SetParent(rectTransform);
        var element3 = el3.AddComponent<LayoutElement>();
        (el3.transform as RectTransform).pivot = Vector2.zero;
        element3.minWidth = 60;
        element3.minHeight = 25;
        element3.preferredWidth = 120;
        element3.preferredHeight = 40;
        element3.flexibleWidth = 1;
        element3.flexibleHeight = 1;
        element3.enabled = true;

        var el4 = new GameObject("Element4");
        el4.transform.SetParent(rectTransform);
        var element4 = el4.AddComponent<LayoutElement>();
        (el4.transform as RectTransform).pivot = Vector2.zero;
        element4.minWidth = 60;
        element4.minHeight = 25;
        element4.preferredWidth = 120;
        element4.preferredHeight = 40;
        element4.flexibleWidth = 1;
        element4.flexibleHeight = 1;
        element4.enabled = true;

        var el5 = new GameObject("Element5");
        el5.transform.SetParent(rectTransform);
        var element5 = el5.AddComponent<LayoutElement>();
        (el5.transform as RectTransform).pivot = Vector2.zero;
        element5.minWidth = 60;
        element5.minHeight = 25;
        element5.preferredWidth = 120;
        element5.preferredHeight = 40;
        element5.flexibleWidth = 1;
        element5.flexibleHeight = 1;
        element5.enabled = true;

        var el6 = new GameObject("Element6");
        el6.transform.SetParent(rectTransform);
        var element6 = el6.AddComponent<LayoutElement>();
        (el6.transform as RectTransform).pivot = Vector2.zero;
        element6.minWidth = 60;
        element6.minHeight = 25;
        element6.preferredWidth = 120;
        element6.preferredHeight = 40;
        element6.flexibleWidth = 1;
        element6.flexibleHeight = 1;
        element6.enabled = true;

        var el7 = new GameObject("Element7");
        el7.transform.SetParent(rectTransform);
        var element7 = el7.AddComponent<LayoutElement>();
        (el7.transform as RectTransform).pivot = Vector2.zero;
        element7.minWidth = 60;
        element7.minHeight = 25;
        element7.preferredWidth = 120;
        element7.preferredHeight = 40;
        element7.flexibleWidth = 1;
        element7.flexibleHeight = 1;
        element7.enabled = true;

        var el8 = new GameObject("Element8");
        el8.transform.SetParent(rectTransform);
        var element8 = el8.AddComponent<LayoutElement>();
        (el8.transform as RectTransform).pivot = Vector2.zero;
        element8.minWidth = 60;
        element8.minHeight = 25;
        element8.preferredWidth = 120;
        element8.preferredHeight = 40;
        element8.flexibleWidth = 1;
        element8.flexibleHeight = 1;
        element8.enabled = true;

        var el9 = new GameObject("Element9");
        el9.transform.SetParent(rectTransform);
        var element9 = el9.AddComponent<LayoutElement>();
        (el9.transform as RectTransform).pivot = Vector2.zero;
        element9.minWidth = 500;
        element9.minHeight = 300;
        element9.preferredWidth = 1000;
        element9.preferredHeight = 600;
        element9.flexibleWidth = 1;
        element9.flexibleHeight = 1;
        element9.enabled = true;
        element9.ignoreLayout = true;

        if (!Directory.Exists("Assets/Resources/"))
            Directory.CreateDirectory("Assets/Resources/");

        PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
        GameObject.DestroyImmediate(rootGO);
#endif
    }

    [SetUp]
    public void TestSetup()
    {
        m_PrefabRoot = GameObject.Instantiate(Resources.Load("GridLayoutGroupTests")) as GameObject;
        m_LayoutGroup = m_PrefabRoot.GetComponentInChildren<GridLayoutGroup>();
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_PrefabRoot);
        m_LayoutGroup = null;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(kPrefabPath);
#endif
    }

    [Test]
    public void TestFlexibleCalculateLayout()
    {
        m_LayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
        Assert.AreEqual(GridLayoutGroup.Constraint.Flexible, m_LayoutGroup.constraint);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_LayoutGroup.GetComponent<RectTransform>());

        Assert.AreEqual(90, m_LayoutGroup.minWidth, "Expected layout group min width to match but it did not");
        Assert.AreEqual(100, m_LayoutGroup.minHeight, "Expected layout group min height to match but it did not");
        Assert.AreEqual(290, m_LayoutGroup.preferredWidth, "Expected layout group preferred width to match but it did not");
        Assert.AreEqual(100, m_LayoutGroup.preferredHeight, "Expected layout group preferred height to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleWidth, "Expected layout group flexiblle width to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleHeight, "Expected layout group flexiblle height to match but it did not");

        Vector2[] expectedPositions =
        {
            new Vector2(0, -50),
            new Vector2(100, -50),
            new Vector2(200, -50),
            new Vector2(300, -50),
            new Vector2(0, -100),
            new Vector2(100, -100),
            new Vector2(200, -100),
            new Vector2(300, -100),
        };

        Vector2 expectedSize = new Vector2(90, 50);

        for (int i = 0; i < expectedPositions.Length; ++i)
        {
            var element = m_LayoutGroup.transform.Find("Element" + (i + 1));
            var rectTransform = element.GetComponent<RectTransform>();

            Assert.AreEqual(expectedPositions[i], rectTransform.anchoredPosition, $"Expected Element { i + 1 } position to match but it did not");
            Assert.AreEqual(expectedSize, rectTransform.sizeDelta, $"Expected Element { i + 1 } size to match but it did not");
        }
    }

    [Test]
    public void TestHorizontallyContrainedCalculateLayoutHorizontal()
    {
        m_LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        m_LayoutGroup.constraintCount = 2;
        Assert.AreEqual(GridLayoutGroup.Constraint.FixedColumnCount, m_LayoutGroup.constraint, "Expected layout group constraint mode to match but it did not");
        Assert.AreEqual(2, m_LayoutGroup.constraintCount, "Expected layout group constraint count mode to match but it did not");
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_LayoutGroup.GetComponent<RectTransform>());

        Assert.AreEqual(190, m_LayoutGroup.minWidth, "Expected layout group min width to match but it did not");
        Assert.AreEqual(200, m_LayoutGroup.minHeight, "Expected layout group min height to match but it did not");
        Assert.AreEqual(190, m_LayoutGroup.preferredWidth, "Expected layout group preferred width to match but it did not");
        Assert.AreEqual(200, m_LayoutGroup.preferredHeight, "Expected layout group preferred height to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleWidth, "Expected layout group flexiblle width to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleHeight, "Expected layout group flexiblle height to match but it did not");

        Vector2[] expectedPositions =
        {
            new Vector2(0, -50),
            new Vector2(100, -50),
            new Vector2(0, -100),
            new Vector2(100, -100),
            new Vector2(0, -150),
            new Vector2(100, -150),
            new Vector2(0, -200),
            new Vector2(100, -200),
        };

        Vector2 expectedSize = new Vector2(90, 50);

        for (int i = 0; i < expectedPositions.Length; ++i)
        {
            var element = m_LayoutGroup.transform.Find("Element" + (i + 1));
            var rectTransform = element.GetComponent<RectTransform>();

            Assert.AreEqual(expectedPositions[i], rectTransform.anchoredPosition, $"Expected Element { i + 1 } position to match but it did not");
            Assert.AreEqual(expectedSize, rectTransform.sizeDelta, $"Expected Element { i + 1 } size to match but it did not");
        }
    }

    [Test]
    public void TestVerticallyContrainedCalculateLayoutHorizontal()
    {
        m_LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        m_LayoutGroup.constraintCount = 2;
        Assert.AreEqual(GridLayoutGroup.Constraint.FixedRowCount, m_LayoutGroup.constraint, "Expected layout group constraint mode to match but it did not");
        Assert.AreEqual(2, m_LayoutGroup.constraintCount, "Expected layout group constraint count mode to match but it did not");
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_LayoutGroup.GetComponent<RectTransform>());

        Assert.AreEqual(390, m_LayoutGroup.minWidth, "Expected layout group min width to match but it did not");
        Assert.AreEqual(100, m_LayoutGroup.minHeight, "Expected layout group min height to match but it did not");
        Assert.AreEqual(390, m_LayoutGroup.preferredWidth, "Expected layout group preferred width to match but it did not");
        Assert.AreEqual(100, m_LayoutGroup.preferredHeight, "Expected layout group preferred height to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleWidth, "Expected layout group flexiblle width to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleHeight, "Expected layout group flexiblle height to match but it did not");

        Vector2[] expectedPositions =
        {
            new Vector2(0, -50),
            new Vector2(100, -50),
            new Vector2(200, -50),
            new Vector2(300, -50),
            new Vector2(0, -100),
            new Vector2(100, -100),
            new Vector2(200, -100),
            new Vector2(300, -100),
        };

        Vector2 expectedSize = new Vector2(90, 50);

        for (int i = 0; i < expectedPositions.Length; ++i)
        {
            var element = m_LayoutGroup.transform.Find("Element" + (i + 1));
            var rectTransform = element.GetComponent<RectTransform>();

            Assert.AreEqual(expectedPositions[i], rectTransform.anchoredPosition, $"Expected Element { i + 1 } position to match but it did not");
            Assert.AreEqual(expectedSize, rectTransform.sizeDelta, $"Expected Element { i + 1 } size to match but it did not");
        }
    }

    [Test]
    public void TestHorizontallyContrainedCalculateLayoutVertical()
    {
        m_LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        m_LayoutGroup.constraintCount = 2;
        m_LayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
        Assert.AreEqual(GridLayoutGroup.Constraint.FixedColumnCount, m_LayoutGroup.constraint, "Expected layout group constraint mode to match but it did not");
        Assert.AreEqual(2, m_LayoutGroup.constraintCount, "Expected layout group constraint count mode to match but it did not");
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_LayoutGroup.GetComponent<RectTransform>());

        Assert.AreEqual(190, m_LayoutGroup.minWidth, "Expected layout group min width to match but it did not");
        Assert.AreEqual(200, m_LayoutGroup.minHeight, "Expected layout group min height to match but it did not");
        Assert.AreEqual(190, m_LayoutGroup.preferredWidth, "Expected layout group preferred width to match but it did not");
        Assert.AreEqual(200, m_LayoutGroup.preferredHeight, "Expected layout group preferred height to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleWidth, "Expected layout group flexiblle width to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleHeight, "Expected layout group flexiblle height to match but it did not");

        Vector2[] expectedPositions =
        {
            new Vector2(0, -50),
            new Vector2(0, -100),
            new Vector2(0, -150),
            new Vector2(0, -200),
            new Vector2(100, -50),
            new Vector2(100, -100),
            new Vector2(100, -150),
            new Vector2(100, -200),
        };

        Vector2 expectedSize = new Vector2(90, 50);

        for (int i = 0; i < expectedPositions.Length; ++i)
        {
            var element = m_LayoutGroup.transform.Find("Element" + (i + 1));
            var rectTransform = element.GetComponent<RectTransform>();

            Assert.AreEqual(expectedPositions[i], rectTransform.anchoredPosition, $"Expected Element { i + 1 } position to match but it did not");
            Assert.AreEqual(expectedSize, rectTransform.sizeDelta, $"Expected Element { i + 1 } size to match but it did not");
        }
    }

    [Test]
    public void TestVerticallyContrainedCalculateLayoutVertical()
    {
        m_LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        m_LayoutGroup.constraintCount = 2;
        m_LayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
        m_LayoutGroup.startCorner = GridLayoutGroup.Corner.LowerRight;
        Assert.AreEqual(GridLayoutGroup.Constraint.FixedRowCount, m_LayoutGroup.constraint, "Expected layout group constraint mode to match but it did not");
        Assert.AreEqual(2, m_LayoutGroup.constraintCount, "Expected layout group constraint count mode to match but it did not");
        m_LayoutGroup.CalculateLayoutInputHorizontal();
        m_LayoutGroup.SetLayoutHorizontal();
        m_LayoutGroup.CalculateLayoutInputVertical();
        m_LayoutGroup.SetLayoutVertical();

        Assert.AreEqual(390, m_LayoutGroup.minWidth, "Expected layout group min width to match but it did not");
        Assert.AreEqual(100, m_LayoutGroup.minHeight, "Expected layout group min height to match but it did not");
        Assert.AreEqual(390, m_LayoutGroup.preferredWidth, "Expected layout group preferred width to match but it did not");
        Assert.AreEqual(100, m_LayoutGroup.preferredHeight, "Expected layout group preferred height to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleWidth, "Expected layout group flexiblle width to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleHeight, "Expected layout group flexiblle height to match but it did not");

        Vector2[] expectedPositions =
        {
            new Vector2(300, -100),
            new Vector2(300, -50),
            new Vector2(200, -100),
            new Vector2(200, -50),
            new Vector2(100, -100),
            new Vector2(100, -50),
            new Vector2(0, -100),
            new Vector2(0, -50),
        };

        Vector2 expectedSize = new Vector2(90, 50);

        for (int i = 0; i < expectedPositions.Length; ++i)
        {
            var element = m_LayoutGroup.transform.Find("Element" + (i + 1));
            var rectTransform = element.GetComponent<RectTransform>();

            Assert.AreEqual(expectedPositions[i], rectTransform.anchoredPosition, $"Expected Element { i + 1 } position to match but it did not");
            Assert.AreEqual(expectedSize, rectTransform.sizeDelta, $"Expected Element { i + 1 } size to match but it did not");
        }
    }

    [Test]
    public void TestHorizontallyContrainedCalculateLayoutHorizontal_WithChildrenToMove()
    {
        m_LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        m_LayoutGroup.constraintCount = 5;
        m_LayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        m_LayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        Assert.AreEqual(GridLayoutGroup.Constraint.FixedRowCount, m_LayoutGroup.constraint, "Expected layout group constraint mode to match but it did not");
        Assert.AreEqual(5, m_LayoutGroup.constraintCount, "Expected layout group constraint count mode to match but it did not");
        m_LayoutGroup.CalculateLayoutInputHorizontal();
        m_LayoutGroup.SetLayoutHorizontal();
        m_LayoutGroup.CalculateLayoutInputVertical();
        m_LayoutGroup.SetLayoutVertical();

        Assert.AreEqual(190, m_LayoutGroup.minWidth, "Expected layout group min width to match but it did not");
        Assert.AreEqual(250, m_LayoutGroup.minHeight, "Expected layout group min height to match but it did not");
        Assert.AreEqual(190, m_LayoutGroup.preferredWidth, "Expected layout group preferred width to match but it did not");
        Assert.AreEqual(250, m_LayoutGroup.preferredHeight, "Expected layout group preferred height to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleWidth, "Expected layout group flexiblle width to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleHeight, "Expected layout group flexiblle height to match but it did not");

        Vector2[] expectedPositions =
        {
            new Vector2(0, -50),
            new Vector2(100, -50),
            new Vector2(0, -100),
            new Vector2(100, -100),
            new Vector2(0, -150),
            new Vector2(100, -150),
            new Vector2(0, -200),
            new Vector2(0, -250)
        };

        Vector2 expectedSize = new Vector2(90, 50);

        for (int i = 0; i < expectedPositions.Length; ++i)
        {
            var element = m_LayoutGroup.transform.Find("Element" + (i + 1));
            var rectTransform = element.GetComponent<RectTransform>();

            Assert.AreEqual(expectedPositions[i], rectTransform.anchoredPosition, $"Expected Element { i + 1 } position to match but it did not");
            Assert.AreEqual(expectedSize, rectTransform.sizeDelta, $"Expected Element { i + 1 } size to match but it did not");
        }
    }

    [Test]
    public void TestVerticallyContrainedCalculateLayoutVertical_WithChildrenToMove()
    {
        m_LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        m_LayoutGroup.constraintCount = 5;
        m_LayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
        m_LayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        Assert.AreEqual(GridLayoutGroup.Constraint.FixedColumnCount, m_LayoutGroup.constraint, "Expected layout group constraint mode to match but it did not");
        Assert.AreEqual(5, m_LayoutGroup.constraintCount, "Expected layout group constraint count mode to match but it did not");
        m_LayoutGroup.CalculateLayoutInputHorizontal();
        m_LayoutGroup.SetLayoutHorizontal();
        m_LayoutGroup.CalculateLayoutInputVertical();
        m_LayoutGroup.SetLayoutVertical();

        Assert.AreEqual(490, m_LayoutGroup.minWidth, "Expected layout group min width to match but it did not");
        Assert.AreEqual(100, m_LayoutGroup.minHeight, "Expected layout group min height to match but it did not");
        Assert.AreEqual(490, m_LayoutGroup.preferredWidth, "Expected layout group preferred width to match but it did not");
        Assert.AreEqual(100, m_LayoutGroup.preferredHeight, "Expected layout group preferred height to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleWidth, "Expected layout group flexiblle width to match but it did not");
        Assert.AreEqual(-1, m_LayoutGroup.flexibleHeight, "Expected layout group flexiblle height to match but it did not");

        Vector2[] expectedPositions =
        {
            new Vector2(0, -50),
            new Vector2(0, -100),
            new Vector2(100, -50),
            new Vector2(100, -100),
            new Vector2(200, -50),
            new Vector2(200, -100),
            new Vector2(300, -50),
            new Vector2(400, -50)
        };

        Vector2 expectedSize = new Vector2(90, 50);

        for (int i = 0; i < expectedPositions.Length; ++i)
        {
            var element = m_LayoutGroup.transform.Find("Element" + (i + 1));
            var rectTransform = element.GetComponent<RectTransform>();

            Assert.AreEqual(expectedPositions[i], rectTransform.anchoredPosition, $"Expected Element { i + 1 } position to match but it did not");
            Assert.AreEqual(expectedSize, rectTransform.sizeDelta, $"Expected Element { i + 1 } size to match but it did not");
        }
    }
}
