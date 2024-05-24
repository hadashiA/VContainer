using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ScrollBarTests : IPrebuildSetup
{
    GameObject m_PrefabRoot;
    const string kPrefabPath = "Assets/Resources/ScrollBarPrefab.prefab";

    public void Setup()
    {
#if UNITY_EDITOR
        var rootGO = new GameObject("rootGo");

        var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        canvasGO.transform.SetParent(rootGO.transform);

        var scrollBarGo = new GameObject("ScrollBar", typeof(Scrollbar));
        scrollBarGo.transform.SetParent(canvasGO.transform);

        if (!Directory.Exists("Assets/Resources/"))
            Directory.CreateDirectory("Assets/Resources/");

        PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
        GameObject.DestroyImmediate(rootGO);
#endif
    }

    [SetUp]
    public void TestSetup()
    {
        m_PrefabRoot = UnityEngine.Object.Instantiate(Resources.Load("ScrollBarPrefab")) as GameObject;
    }

    [Test]
    public void ScrollBarSetValueWithoutNotifyWillNotNotify()
    {
        Scrollbar s = m_PrefabRoot.GetComponentInChildren<Scrollbar>();
        s.value = 0;

        bool calledOnValueChanged = false;

        s.onValueChanged.AddListener(b => { calledOnValueChanged = true; });

        s.SetValueWithoutNotify(1);

        Assert.IsTrue(s.value == 1);
        Assert.IsFalse(calledOnValueChanged);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_PrefabRoot);
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(kPrefabPath);
#endif
    }
}
