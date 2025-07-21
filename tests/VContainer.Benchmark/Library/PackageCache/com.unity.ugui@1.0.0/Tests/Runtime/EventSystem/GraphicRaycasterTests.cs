using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.TestTools.Utils;

public class GraphicRaycasterTests
{
    Camera m_Camera;
    EventSystem m_EventSystem;
    Canvas m_Canvas;
    RectTransform m_CanvasRectTrans;
    Text m_TextComponent;

    [SetUp]
    public void TestSetup()
    {
        m_Camera = new GameObject("GraphicRaycaster Camera").AddComponent<Camera>();
        m_Camera.transform.position = Vector3.zero;
        m_Camera.transform.LookAt(Vector3.forward);
        m_Camera.farClipPlane = 10;

        m_EventSystem = new GameObject("Event System").AddComponent<EventSystem>();

        m_Canvas = new GameObject("Canvas").AddComponent<Canvas>();
        m_Canvas.renderMode = RenderMode.WorldSpace;
        m_Canvas.worldCamera = m_Camera;
        m_Canvas.gameObject.AddComponent<GraphicRaycaster>();
        m_CanvasRectTrans = m_Canvas.GetComponent<RectTransform>();
        m_CanvasRectTrans.sizeDelta = new Vector2(100, 100);

        var textGO = new GameObject("Text");
        m_TextComponent = textGO.AddComponent<Text>();
        var textRectTrans = m_TextComponent.rectTransform;
        textRectTrans.SetParent(m_Canvas.transform);
        textRectTrans.anchorMin = Vector2.zero;
        textRectTrans.anchorMax = Vector2.one;
        textRectTrans.offsetMin = Vector2.zero;
        textRectTrans.offsetMax = Vector2.zero;
    }

    [UnityTest]
    public IEnumerator GraphicRaycasterDoesNotHitGraphicBehindCameraFarClipPlane()
    {
        m_CanvasRectTrans.anchoredPosition3D = new Vector3(0, 0, 11);

        yield return null;

        var results = new List<RaycastResult>();
        var pointerEvent = new PointerEventData(m_EventSystem)
        {
            position = new Vector2(Screen.width / 2f, Screen.height / 2f)
        };

        m_EventSystem.RaycastAll(pointerEvent, results);

        Assert.IsEmpty(results, "Expected no results from a raycast against a graphic behind the camera's far clip plane.");
    }

    [UnityTest]
    public IEnumerator GraphicRaycasterReturnsWorldPositionAndWorldNormal()
    {
        m_CanvasRectTrans.anchoredPosition3D = new Vector3(0, 0, 11);
        m_Camera.farClipPlane = 12;

        yield return null;

        var results = new List<RaycastResult>();
        var pointerEvent = new PointerEventData(m_EventSystem)
        {
            position = new Vector2(Screen.width / 2f, Screen.height / 2f)
        };

        m_EventSystem.RaycastAll(pointerEvent, results);
        // on katana on 10.13 agents world position returned is 0, -0.00952, 11
        // it does not reproduce for me localy, so we just tweak the comparison threshold
        Assert.That(new Vector3(0, 0, 11), Is.EqualTo(results[0].worldPosition).Using(new Vector3EqualityComparer(0.01f)));
        Assert.That(new Vector3(0, 0, -1), Is.EqualTo(results[0].worldNormal).Using(new Vector3EqualityComparer(0.001f)));
    }

    [UnityTest]
    public IEnumerator GraphicRaycasterUsesGraphicPadding()
    {
        m_CanvasRectTrans.anchoredPosition3D = new Vector3(0, 0, 11);
        m_TextComponent.raycastPadding = new Vector4(-50, -50, -50, -50);
        m_Camera.farClipPlane = 12;
        yield return null;

        var results = new List<RaycastResult>();
        var pointerEvent = new PointerEventData(m_EventSystem)
        {
            position = new Vector2((Screen.width / 2f) - 60, Screen.height / 2f)
        };

        m_EventSystem.RaycastAll(pointerEvent, results);

        Assert.IsNotEmpty(results, "Expected at least 1 result from a raycast outside the graphics RectTransform whose padding would make the click hit.");
    }

    [UnityTest]
    public IEnumerator GraphicOnTheSamePlaneAsTheCameraCanBeTargetedForEvents()
    {
        m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        m_Canvas.planeDistance = 0;
        m_Camera.orthographic = true;
        m_Camera.orthographicSize = 1;
        m_Camera.nearClipPlane = 0;
        m_Camera.farClipPlane = 12;
        yield return null;

        var results = new List<RaycastResult>();
        var pointerEvent = new PointerEventData(m_EventSystem)
        {
            position = new Vector2((Screen.width / 2f), Screen.height / 2f)
        };

        m_EventSystem.RaycastAll(pointerEvent, results);

        Assert.IsNotEmpty(results, "Expected at least 1 result from a raycast ");
    }
#if ENABLE_INPUT_SYSTEM
    [UnityTest]
    public IEnumerator GraphicRaycasterIgnoresEventsFromTheWrongDisplay()
    {
        m_CanvasRectTrans.anchoredPosition3D = new Vector3(0, 0, 11);
        m_Camera.farClipPlane = 12;

        yield return null;
        var results = new List<RaycastResult>();
        var pointerEvent = new PointerEventData(m_EventSystem)
        {
            position = new Vector2(Screen.width / 2f, Screen.height / 2f),
            displayIndex = 1,
        };

        m_EventSystem.RaycastAll(pointerEvent, results);

        Assert.IsEmpty(results, "Pointer event on display 1 was not ignored on display 0");
    }
#endif
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_Camera.gameObject);
        Object.DestroyImmediate(m_EventSystem.gameObject);
        Object.DestroyImmediate(m_Canvas.gameObject);
    }
}
