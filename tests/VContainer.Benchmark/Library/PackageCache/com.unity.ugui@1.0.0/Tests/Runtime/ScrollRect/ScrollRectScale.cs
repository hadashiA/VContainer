using System.IO;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;

public class ScrollRectScale : IPrebuildSetup
{
    const string kPrefabPath = "Assets/Resources/ScrollRectScalePrefab.prefab";
    public void Setup()
    {
#if UNITY_EDITOR
        var rootGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
        rootGO.transform.SetParent(rootGO.transform);
        var rootGOTransform = rootGO.transform as RectTransform;
        rootGOTransform.anchoredPosition3D = new Vector3(679, -122, 783);
        rootGOTransform.sizeDelta = new Vector2(1165, 765);
        rootGOTransform.localRotation = Quaternion.Euler(20, 30, 0);


        var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        bodyGO.transform.SetParent(rootGO.transform);
        var bodyGORectTransform = bodyGO.transform as RectTransform;
        bodyGORectTransform.localRotation = Quaternion.identity;
        bodyGORectTransform.anchorMin = Vector2.zero;
        bodyGORectTransform.anchorMax = Vector2.one;
        bodyGORectTransform.localPosition = Vector3.zero;
        bodyGORectTransform.anchoredPosition = new Vector2(8, -8);
        bodyGORectTransform.sizeDelta = new Vector2(-16, -16);
        bodyGORectTransform.pivot = new Vector2(0, 1);

        var sizer = new GameObject("Sizer", typeof(RectTransform));
        var sizerTransform = sizer.transform as RectTransform;
        sizerTransform.SetParent(bodyGO.transform);
        sizerTransform.localRotation = Quaternion.identity;
        sizerTransform.sizeDelta = new Vector2(1149, 3700);
        sizerTransform.anchoredPosition3D = Vector3.zero;
        sizerTransform.anchorMin = new Vector2(0, 1);
        sizerTransform.anchorMax = new Vector2(0, 1);
        sizerTransform.pivot = new Vector2(0, 1);

        var bodyGOScrollRect = bodyGO.GetComponent<ScrollRect>();
        bodyGOScrollRect.content = sizerTransform;
        bodyGOScrollRect.movementType = ScrollRect.MovementType.Clamped;
        bodyGOScrollRect.decelerationRate = 0.135f;

        for (int i = 0; i < 19; ++i)
        {
            var row = new GameObject("Row" + i, typeof(Image));
            var rowTransform = row.transform as RectTransform;
            rowTransform.SetParent(sizer.transform);
            rowTransform.localRotation = Quaternion.identity;
            rowTransform.anchorMin = new Vector2(0, 1);
            rowTransform.anchorMax = new Vector2(0, 1);
            rowTransform.pivot = new Vector2(0, 1);
            rowTransform.sizeDelta = new Vector2(1149, 37);
            rowTransform.anchoredPosition3D = new Vector3(0, i * -37, 0);
        }

        if (!Directory.Exists("Assets/Resources/"))
            Directory.CreateDirectory("Assets/Resources/");

        UnityEditor.PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
        GameObject.DestroyImmediate(rootGO);
#endif
    }

    GameObject m_CanvasGO;

    [SetUp]
    public void TestSetup()
    {
        m_CanvasGO = Object.Instantiate(Resources.Load("ScrollRectScalePrefab")) as GameObject;
    }

    [UnityTest]
    [Description("Rect Transform position values are set to NaN, when Rect Transform is created via script and canvas is scaled to 0.01(case 893559)")]
    public IEnumerator SmallScaleDoesNotCauseInvalidContentPosition()
    {
        var canvas = m_CanvasGO.GetComponent<Canvas>();
        var scrollRect = canvas.GetComponentInChildren<ScrollRect>();
        var contentRt = scrollRect.content.GetComponent<RectTransform>();
        var contentLocalPosition = contentRt.localPosition;

        var rt = canvas.GetComponent<RectTransform>();
        rt.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        yield return null;

        Assert.AreEqual(contentLocalPosition.x, contentRt.localPosition.x, 0.001f);
        Assert.AreEqual(contentLocalPosition.y, contentRt.localPosition.y, 0.001f);
        Assert.AreEqual(contentLocalPosition.z, contentRt.localPosition.z, 0.001f);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasGO);
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(kPrefabPath);
#endif
    }
}
