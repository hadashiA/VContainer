using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class InputModuleTests
{
    EventSystem m_EventSystem;
    FakeBaseInput m_FakeBaseInput;
    StandaloneInputModule m_StandaloneInputModule;
    Canvas m_Canvas;
    Image m_Image;
    Image m_NestedImage;

    [SetUp]
    public void TestSetup()
    {
        // Camera | Canvas (Image) | Event System

        m_Canvas = new GameObject("Canvas").AddComponent<Canvas>();
        m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        m_Canvas.gameObject.AddComponent<GraphicRaycaster>();

        m_Image = new GameObject("Image").AddComponent<Image>();
        m_Image.gameObject.transform.SetParent(m_Canvas.transform);
        RectTransform imageRectTransform = m_Image.GetComponent<RectTransform>();
        imageRectTransform.sizeDelta = new Vector2(400f, 400f);
        imageRectTransform.localPosition = Vector3.zero;

        m_NestedImage = new GameObject("NestedImage").AddComponent<Image>();
        m_NestedImage.gameObject.transform.SetParent(m_Image.transform);
        RectTransform nestedImageRectTransform = m_NestedImage.GetComponent<RectTransform>();
        nestedImageRectTransform.sizeDelta = new Vector2(200f, 200f);
        nestedImageRectTransform.localPosition = Vector3.zero;

        GameObject go = new GameObject("Event System");
        m_EventSystem = go.AddComponent<EventSystem>();
        m_EventSystem.pixelDragThreshold = 1;

        m_StandaloneInputModule = go.AddComponent<StandaloneInputModule>();
        m_FakeBaseInput = go.AddComponent<FakeBaseInput>();

        // Override input with FakeBaseInput so we can send fake mouse/keyboards button presses and touches
        m_StandaloneInputModule.inputOverride = m_FakeBaseInput;

        Cursor.lockState = CursorLockMode.None;
    }

    [UnityTest]
    public IEnumerator DragCallbacksDoGetCalled()
    {
        // While left mouse button is pressed and the mouse is moving, OnBeginDrag and OnDrag callbacks should be called
        // Then when the left mouse button is released, OnEndDrag callback should be called

        // Add script to EventSystem to update the mouse position
        m_EventSystem.gameObject.AddComponent<MouseUpdate>();

        // Add script to Image which implements OnBeginDrag, OnDrag & OnEndDrag callbacks
        DragCallbackCheck callbackCheck = m_Image.gameObject.AddComponent<DragCallbackCheck>();

        // Setting required input.mousePresent to fake mouse presence
        m_FakeBaseInput.MousePresent = true;

        var canvasRT = m_Canvas.gameObject.transform as RectTransform;
        m_FakeBaseInput.MousePosition = new Vector2(Screen.width / 2, Screen.height / 2);

        yield return null;

        // Left mouse button down simulation
        m_FakeBaseInput.MouseButtonDown[0] = true;

        yield return null;

        // Left mouse button down flag needs to reset in the next frame
        m_FakeBaseInput.MouseButtonDown[0] = false;

        yield return null;

        // Left mouse button up simulation
        m_FakeBaseInput.MouseButtonUp[0] = true;

        yield return null;

        // Left mouse button up flag needs to reset in the next frame
        m_FakeBaseInput.MouseButtonUp[0] = false;

        yield return null;

        Assert.IsTrue(callbackCheck.onBeginDragCalled, "OnBeginDrag not called");
        Assert.IsTrue(callbackCheck.onDragCalled, "OnDragCalled not called");
        Assert.IsTrue(callbackCheck.onEndDragCalled, "OnEndDragCalled not called");
        Assert.IsTrue(callbackCheck.onDropCalled, "OnDrop not called");
    }

    [UnityTest]
    public IEnumerator MouseOutsideMaskRectTransform_WhileInsidePaddedArea_PerformsClick()
    {
        var mask = new GameObject("Panel").AddComponent<RectMask2D>();
        mask.gameObject.transform.SetParent(m_Canvas.transform);
        RectTransform panelRectTransform = mask.GetComponent<RectTransform>();
        panelRectTransform.sizeDelta = new Vector2(100, 100f);
        panelRectTransform.localPosition = Vector3.zero;

        m_Image.gameObject.transform.SetParent(mask.transform, true);
        mask.padding = new Vector4(-30, -30, -30, -30);


        PointerClickCallbackCheck callbackCheck = m_Image.gameObject.AddComponent<PointerClickCallbackCheck>();

        var canvasRT = m_Canvas.gameObject.transform as RectTransform;
        var screenMiddle = new Vector2(Screen.width / 2, Screen.height / 2);
        m_FakeBaseInput.MousePresent = true;
        m_FakeBaseInput.MousePosition = screenMiddle;

        yield return null;
        // Click the center of the screen should hit the middle of the image.
        m_FakeBaseInput.MouseButtonDown[0] = true;
        yield return null;
        m_FakeBaseInput.MouseButtonDown[0] = false;
        yield return null;
        m_FakeBaseInput.MouseButtonUp[0] = true;
        yield return null;
        m_FakeBaseInput.MouseButtonUp[0] = false;
        yield return null;
        Assert.IsTrue(callbackCheck.pointerDown);

        //Reset the callbackcheck and click outside the mask but still in the image.
        callbackCheck.pointerDown = false;
        m_FakeBaseInput.MousePosition = new Vector2(screenMiddle.x - 60, screenMiddle.y);
        yield return null;
        m_FakeBaseInput.MouseButtonDown[0] = true;
        yield return null;
        m_FakeBaseInput.MouseButtonDown[0] = false;
        yield return null;
        m_FakeBaseInput.MouseButtonUp[0] = true;
        yield return null;
        m_FakeBaseInput.MouseButtonUp[0] = false;
        yield return null;
        Assert.IsTrue(callbackCheck.pointerDown);

        //Reset the callbackcheck and click outside the mask and outside in the image.
        callbackCheck.pointerDown = false;
        m_FakeBaseInput.MousePosition = new Vector2(screenMiddle.x - 100, screenMiddle.y);
        yield return null;
        m_FakeBaseInput.MouseButtonDown[0] = true;
        yield return null;
        m_FakeBaseInput.MouseButtonDown[0] = false;
        yield return null;
        m_FakeBaseInput.MouseButtonUp[0] = true;
        yield return null;
        m_FakeBaseInput.MouseButtonUp[0] = false;
        yield return null;
        Assert.IsFalse(callbackCheck.pointerDown);
    }

    [UnityTest]
    public IEnumerator PointerEnterChildShouldNotFullyExit_NotSendPointerEventToParent()
    {
        m_StandaloneInputModule.sendPointerHoverToParent = false;
        PointerExitCallbackCheck callbackCheck = m_Image.gameObject.AddComponent<PointerExitCallbackCheck>();
        m_NestedImage.gameObject.AddComponent<PointerExitCallbackCheck>();
        var screenMiddle = new Vector2(Screen.width / 2, Screen.height / 2);

        m_FakeBaseInput.MousePresent = true;
        m_FakeBaseInput.MousePosition = screenMiddle - new Vector2(150, 150);
        yield return null;
        m_FakeBaseInput.MousePosition = screenMiddle;
        yield return null;
        Assert.IsTrue(callbackCheck.pointerData.fullyExited == false);
    }

    [UnityTest]
    public IEnumerator PointerEnterChildShouldNotExit_SendPointerEventToParent()
    {
        m_StandaloneInputModule.sendPointerHoverToParent = true;
        PointerExitCallbackCheck callbackCheck = m_Image.gameObject.AddComponent<PointerExitCallbackCheck>();
        m_NestedImage.gameObject.AddComponent<PointerExitCallbackCheck>();
        var screenMiddle = new Vector2(Screen.width / 2, Screen.height / 2);

        m_FakeBaseInput.MousePresent = true;
        m_FakeBaseInput.MousePosition = screenMiddle - new Vector2(150, 150);
        yield return null;
        m_FakeBaseInput.MousePosition = screenMiddle;
        yield return null;
        Assert.IsTrue(callbackCheck.pointerData == null);
    }

    [UnityTest]
    public IEnumerator PointerEnterChildShouldNotReenter()
    {
        PointerEnterCallbackCheck callbackCheck = m_NestedImage.gameObject.AddComponent<PointerEnterCallbackCheck>();
        var screenMiddle = new Vector2(Screen.width / 2, Screen.height / 2);

        m_FakeBaseInput.MousePresent = true;
        m_FakeBaseInput.MousePosition = screenMiddle - new Vector2(150, 150);
        yield return null;
        m_FakeBaseInput.MousePosition = screenMiddle;
        yield return null;
        Assert.IsTrue(callbackCheck.pointerData.reentered == false);
    }

    [UnityTest]
    public IEnumerator PointerExitChildShouldReenter_NotSendPointerEventToParent()
    {
        m_StandaloneInputModule.sendPointerHoverToParent = false;
        PointerEnterCallbackCheck callbackCheck = m_Image.gameObject.AddComponent<PointerEnterCallbackCheck>();
        var screenMiddle = new Vector2(Screen.width / 2, Screen.height / 2);

        m_FakeBaseInput.MousePresent = true;
        m_FakeBaseInput.MousePosition = screenMiddle - new Vector2(150, 150);
        yield return null;
        m_FakeBaseInput.MousePosition = screenMiddle;
        yield return null;
        m_FakeBaseInput.MousePosition = screenMiddle - new Vector2(150, 150);
        yield return null;
        Assert.IsTrue(callbackCheck.pointerData.reentered == true);
    }

    [UnityTest]
    public IEnumerator PointerExitChildShouldNotSendEnter_SendPointerEventToParent()
    {
        m_StandaloneInputModule.sendPointerHoverToParent = true;
        m_NestedImage.gameObject.AddComponent<PointerEnterCallbackCheck>();
        var screenMiddle = new Vector2(Screen.width / 2, Screen.height / 2);

        m_FakeBaseInput.MousePresent = true;
        m_FakeBaseInput.MousePosition = screenMiddle;
        yield return null;
        PointerEnterCallbackCheck callbackCheck = m_Image.gameObject.AddComponent<PointerEnterCallbackCheck>();
        m_FakeBaseInput.MousePosition = screenMiddle - new Vector2(150, 150);
        yield return null;
        Assert.IsTrue(callbackCheck.pointerData == null);
    }

    [UnityTest]
    public IEnumerator PointerExitChildShouldFullyExit()
    {
        PointerExitCallbackCheck callbackCheck = m_NestedImage.gameObject.AddComponent<PointerExitCallbackCheck>();
        var screenMiddle = new Vector2(Screen.width / 2, Screen.height / 2);

        m_FakeBaseInput.MousePresent = true;
        m_FakeBaseInput.MousePosition = screenMiddle - new Vector2(150, 150);
        yield return null;
        m_FakeBaseInput.MousePosition = screenMiddle;
        yield return null;
        m_FakeBaseInput.MousePosition = screenMiddle - new Vector2(150, 150);
        yield return null;
        Assert.IsTrue(callbackCheck.pointerData.fullyExited == true);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_EventSystem.gameObject);
        GameObject.DestroyImmediate(m_Canvas.gameObject);
    }
}
