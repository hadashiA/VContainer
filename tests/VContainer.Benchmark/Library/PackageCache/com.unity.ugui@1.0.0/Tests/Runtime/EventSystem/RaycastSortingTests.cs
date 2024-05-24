using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class RaycastSortingTests : IPrebuildSetup
{
    // Test to check that a a raycast over two canvases will not use hierarchal depth to compare two results
    // from different canvases (case 912396 - Raycast hits ignores 2nd Canvas which is drawn in front)
    GameObject m_PrefabRoot;

    const string kPrefabPath = "Assets/Resources/RaycastSortingPrefab.prefab";

    public void Setup()
    {
#if UNITY_EDITOR
        var rootGO = new GameObject("RootGO");
        var cameraGO = new GameObject("Camera", typeof(Camera));
        var camera = cameraGO.GetComponent<Camera>();
        cameraGO.transform.SetParent(rootGO.transform);
        var eventSystemGO = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemGO.transform.SetParent(rootGO.transform);

        var backCanvasGO = new GameObject("BackCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        backCanvasGO.transform.SetParent(rootGO.transform);
        var backCanvas = backCanvasGO.GetComponent<Canvas>();
        backCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        backCanvas.planeDistance = 100;
        backCanvas.worldCamera = camera;

        var backCanvasBackground = new GameObject("BackCanvasBackground", typeof(RectTransform), typeof(Image));
        var backCanvasBackgroundTransform = backCanvasBackground.GetComponent<RectTransform>();
        backCanvasBackgroundTransform.SetParent(backCanvasGO.transform);
        backCanvasBackgroundTransform.anchorMin = Vector2.zero;
        backCanvasBackgroundTransform.anchorMax = Vector2.one;
        backCanvasBackgroundTransform.sizeDelta = Vector2.zero;
        backCanvasBackgroundTransform.anchoredPosition3D = Vector3.zero;
        backCanvasBackgroundTransform.localScale = Vector3.one;

        var backCanvasDeeper = new GameObject("BackCanvasDeeperHierarchy", typeof(RectTransform), typeof(Image));
        var backCanvasDeeperTransform = backCanvasDeeper.GetComponent<RectTransform>();
        backCanvasDeeperTransform.SetParent(backCanvasBackgroundTransform);
        backCanvasDeeperTransform.anchorMin = new Vector2(0.5f, 0);
        backCanvasDeeperTransform.anchorMax = Vector2.one;
        backCanvasDeeperTransform.sizeDelta = Vector2.zero;
        backCanvasDeeperTransform.anchoredPosition3D = Vector3.zero;
        backCanvasDeeperTransform.localScale = Vector3.one;
        backCanvasDeeper.GetComponent<Image>().color = new Color(0.6985294f, 0.7754564f, 1f);

        var frontCanvasGO = new GameObject("FrontCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        frontCanvasGO.transform.SetParent(rootGO.transform);
        var frontCanvas = frontCanvasGO.GetComponent<Canvas>();
        frontCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        frontCanvas.planeDistance = 50;
        frontCanvas.worldCamera = camera;

        var frontCanvasTopLevel = new GameObject("FrontCanvasTopLevel", typeof(RectTransform), typeof(Text));
        var frontCanvasTopLevelTransform = frontCanvasTopLevel.GetComponent<RectTransform>();
        frontCanvasTopLevelTransform.SetParent(frontCanvasGO.transform);
        frontCanvasTopLevelTransform.anchorMin = Vector2.zero;
        frontCanvasTopLevelTransform.anchorMax = new Vector2(1, 0.5f);
        frontCanvasTopLevelTransform.sizeDelta = Vector2.zero;
        frontCanvasTopLevelTransform.anchoredPosition3D = Vector3.zero;
        frontCanvasTopLevelTransform.localScale = Vector3.one;
        var text = frontCanvasTopLevel.GetComponent<Text>();
        text.text = "FrontCanvasTopLevel";
        text.color = Color.black;
        text.fontSize = 97;

        if (!Directory.Exists("Assets/Resources/"))
            Directory.CreateDirectory("Assets/Resources/");

        UnityEditor.PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
        GameObject.DestroyImmediate(rootGO);
#endif
    }

    [SetUp]
    public void TestSetup()
    {
        m_PrefabRoot = Object.Instantiate(Resources.Load("RaycastSortingPrefab")) as GameObject;
    }

    [UnityTest]
    public IEnumerator RaycastResult_Sorting()
    {
        Camera cam = m_PrefabRoot.GetComponentInChildren<Camera>();
        EventSystem eventSystem = m_PrefabRoot.GetComponentInChildren<EventSystem>();
        GameObject shouldHit = m_PrefabRoot.GetComponentInChildren<Text>().gameObject;

        PointerEventData eventData = new PointerEventData(eventSystem);
        //bottom left quadrant
        eventData.position = cam.ViewportToScreenPoint(new Vector3(0.75f, 0.25f));

        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(eventData, results);

        Assert.IsTrue(results[0].gameObject.name == shouldHit.name);

        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_PrefabRoot);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(kPrefabPath);
#endif
    }
}
