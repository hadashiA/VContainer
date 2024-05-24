using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

public class DropdownTests : IPrebuildSetup
{
    GameObject m_PrefabRoot;
    GameObject m_CameraGO;

    const string kPrefabPath = "Assets/Resources/DropdownPrefab.prefab";

    public void Setup()
    {
#if UNITY_EDITOR
        var rootGO = new GameObject("rootGo");
        var canvasGO = new GameObject("Canvas", typeof(Canvas));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.transform.SetParent(rootGO.transform);

        var dropdownGO = new GameObject("Dropdown", typeof(RectTransform), typeof(Dropdown));
        var dropdownTransform = dropdownGO.GetComponent<RectTransform>();
        dropdownTransform.SetParent(canvas.transform);
        dropdownTransform.anchoredPosition = Vector2.zero;
        var dropdown = dropdownGO.GetComponent<Dropdown>();

        var templateGO = new GameObject("Template", typeof(RectTransform));
        templateGO.SetActive(false);
        var templateTransform = templateGO.GetComponent<RectTransform>();
        templateTransform.SetParent(dropdownTransform);

        var itemGo = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
        itemGo.transform.SetParent(templateTransform);

        dropdown.template = templateTransform;

        if (!Directory.Exists("Assets/Resources/"))
            Directory.CreateDirectory("Assets/Resources/");

        PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
        GameObject.DestroyImmediate(rootGO);


        // add a custom sorting layer before test. It doesn't seem to be serialized so no need to remove it after test
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");
        sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
        var arrayElement = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
        foreach (SerializedProperty a in arrayElement)
        {
            switch (a.name)
            {
                case "name":
                    a.stringValue = "test layer";
                    break;
                case "uniqueID":
                    a.intValue = 314159265;
                    break;
                case "locked":
                    a.boolValue = false;
                    break;
            }
        }
        tagManager.ApplyModifiedProperties();
#endif
    }

    [SetUp]
    public void TestSetup()
    {
        m_PrefabRoot = Object.Instantiate(Resources.Load("DropdownPrefab")) as GameObject;
        m_CameraGO = new GameObject("Camera", typeof(Camera));
    }

    // test for case 958281 - [UI] Dropdown list does not copy the parent canvas layer when the panel is opened
    [UnityTest]
    public IEnumerator Dropdown_Canvas()
    {
        var dropdown = m_PrefabRoot.GetComponentInChildren<Dropdown>();
        var rootCanvas = m_PrefabRoot.GetComponentInChildren<Canvas>();
        rootCanvas.sortingLayerName = "test layer";
        dropdown.Show();
        yield return null;
        var dropdownList = dropdown.transform.Find("Dropdown List");
        var dropdownListCanvas = dropdownList.GetComponentInChildren<Canvas>();
        Assert.AreEqual(rootCanvas.sortingLayerID, dropdownListCanvas.sortingLayerID, "Sorting layers should match");
    }

    // test for case 1343542 - [UI] Child Canvas' Sorting Layer is changed to the same value as the parent
    [UnityTest]
    public IEnumerator Dropdown_Canvas_Already_Exists()
    {
        var dropdown = m_PrefabRoot.GetComponentInChildren<Dropdown>();
        var rootCanvas = m_PrefabRoot.GetComponentInChildren<Canvas>();
        var templateCanvas = dropdown.transform.Find("Template").gameObject.AddComponent<Canvas>();
        templateCanvas.overrideSorting = true;
        templateCanvas.sortingLayerName = "test layer";
        dropdown.Show();
        yield return null;
        var dropdownList = dropdown.transform.Find("Dropdown List");
        var dropdownListCanvas = dropdownList.GetComponentInChildren<Canvas>();
        Assert.AreNotEqual(rootCanvas.sortingLayerName, dropdownListCanvas.sortingLayerName, "Sorting layers should not match");
    }

    // test for case 935649 - open dropdown menus become unresponsive when disabled and reenabled
    [UnityTest]
    public IEnumerator Dropdown_Disable()
    {
        var dropdown = m_PrefabRoot.GetComponentInChildren<Dropdown>();
        dropdown.Show();
        dropdown.gameObject.SetActive(false);
        yield return null;
        var dropdownList = dropdown.transform.Find("Dropdown List");
        Assert.IsNull(dropdownList);
    }

    [UnityTest]
    public IEnumerator Dropdown_ResetAndClear()
    {
        var options = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        var dropdown = m_PrefabRoot.GetComponentInChildren<Dropdown>();

        // generate a first dropdown
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = 3;
        yield return null;


        // clear it and generate a new one
        dropdown.ClearOptions();
        yield return null;

        // check is the value is 0
        Assert.IsTrue(dropdown.value == 0);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_PrefabRoot);
        GameObject.DestroyImmediate(m_CameraGO);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(kPrefabPath);

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");
        sortingLayers.DeleteArrayElementAtIndex(sortingLayers.arraySize);
        tagManager.ApplyModifiedProperties();
#endif
    }
}
