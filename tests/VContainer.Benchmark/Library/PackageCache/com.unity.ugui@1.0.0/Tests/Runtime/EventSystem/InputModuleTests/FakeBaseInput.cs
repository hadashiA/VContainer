using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FakeBaseInput : BaseInput
{
    [NonSerialized]
    public String CompositionString = "";

    private  IMECompositionMode m_ImeCompositionMode = IMECompositionMode.Auto;
    private Vector2 m_CompositionCursorPos = Vector2.zero;

    [NonSerialized]
    public bool MousePresent = false;

    [NonSerialized]
    public bool[] MouseButtonDown = new bool[3];

    [NonSerialized]
    public bool[] MouseButtonUp = new bool[3];

    [NonSerialized]
    public bool[] MouseButton = new bool[3];

    [NonSerialized]
    public Vector2 MousePosition = Vector2.zero;

    [NonSerialized]
    public Vector2 MouseScrollDelta = Vector2.zero;

    [NonSerialized]
    public bool TouchSupported = false;

    [NonSerialized]
    public int TouchCount = 0;

    [NonSerialized]
    public Touch TouchData;

    [NonSerialized]
    public float AxisRaw = 0f;

    [NonSerialized]
    public bool ButtonDown = false;

    public override string compositionString
    {
        get { return CompositionString; }
    }

    public override IMECompositionMode imeCompositionMode
    {
        get { return m_ImeCompositionMode; }
        set { m_ImeCompositionMode = value; }
    }

    public override Vector2 compositionCursorPos
    {
        get { return m_CompositionCursorPos; }
        set { m_CompositionCursorPos = value; }
    }

    public override bool mousePresent
    {
        get { return MousePresent; }
    }

    public override bool GetMouseButtonDown(int button)
    {
        return MouseButtonDown[button];
    }

    public override bool GetMouseButtonUp(int button)
    {
        return MouseButtonUp[button];
    }

    public override bool GetMouseButton(int button)
    {
        return MouseButton[button];
    }

    public override Vector2 mousePosition
    {
        get { return MousePosition; }
    }

    public override Vector2 mouseScrollDelta
    {
        get { return MouseScrollDelta; }
    }

    public override bool touchSupported
    {
        get { return TouchSupported; }
    }

    public override int touchCount
    {
        get { return TouchCount; }
    }

    public override Touch GetTouch(int index)
    {
        return TouchData;
    }

    public override float GetAxisRaw(string axisName)
    {
        return AxisRaw;
    }

    public override bool GetButtonDown(string buttonName)
    {
        return ButtonDown;
    }
}
