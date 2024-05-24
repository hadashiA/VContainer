using UnityEngine.EventSystems;

namespace UnityEngine.UIElements
{
    // This code is disabled unless the UI Toolkit package or the com.unity.modules.uielements module are present.
    // The UIElements module is always present in the Editor but it can be stripped from a project build if unused.
#if PACKAGE_UITOOLKIT
    /// <summary>
    /// Use this class to handle input and send events to UI Toolkit runtime panels.
    /// </summary>
    [AddComponentMenu("UI Toolkit/Panel Event Handler (UI Toolkit)")]
    public class PanelEventHandler : UIBehaviour, IPointerMoveHandler, IPointerUpHandler, IPointerDownHandler,
        ISubmitHandler, ICancelHandler, IMoveHandler, IScrollHandler, ISelectHandler, IDeselectHandler,
        IPointerExitHandler, IPointerEnterHandler, IRuntimePanelComponent
    {
        private BaseRuntimePanel m_Panel;

        /// <summary>
        /// The panel that this component relates to. If panel is null, this component will have no effect.
        /// Will be set to null automatically if panel is Disposed from an external source.
        /// </summary>
        public IPanel panel
        {
            get => m_Panel;
            set
            {
                var newPanel = (BaseRuntimePanel)value;
                if (m_Panel != newPanel)
                {
                    UnregisterCallbacks();
                    m_Panel = newPanel;
                    RegisterCallbacks();
                }
            }
        }

        private GameObject selectableGameObject => m_Panel?.selectableGameObject;
        private EventSystem eventSystem => UIElementsRuntimeUtility.activeEventSystem as EventSystem;

        private bool isCurrentFocusedPanel => m_Panel != null && eventSystem != null &&
                                              eventSystem.currentSelectedGameObject == selectableGameObject;

        private Focusable currentFocusedElement => m_Panel?.focusController.GetLeafFocusedElement();

        private readonly PointerEvent m_PointerEvent = new PointerEvent();

        protected override void OnEnable()
        {
            base.OnEnable();
            RegisterCallbacks();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnregisterCallbacks();
        }

        void RegisterCallbacks()
        {
            if (m_Panel != null)
            {
                m_Panel.destroyed += OnPanelDestroyed;
                m_Panel.visualTree.RegisterCallback<FocusEvent>(OnElementFocus, TrickleDown.TrickleDown);
                m_Panel.visualTree.RegisterCallback<BlurEvent>(OnElementBlur, TrickleDown.TrickleDown);
            }
        }

        void UnregisterCallbacks()
        {
            if (m_Panel != null)
            {
                m_Panel.destroyed -= OnPanelDestroyed;
                m_Panel.visualTree.UnregisterCallback<FocusEvent>(OnElementFocus, TrickleDown.TrickleDown);
                m_Panel.visualTree.UnregisterCallback<BlurEvent>(OnElementBlur, TrickleDown.TrickleDown);
            }
        }

        void OnPanelDestroyed()
        {
            panel = null;
        }

        void OnElementFocus(FocusEvent e)
        {
            if (!m_Selecting && eventSystem != null)
                eventSystem.SetSelectedGameObject(selectableGameObject);
        }

        void OnElementBlur(BlurEvent e)
        {
            // Important: if panel discards focus entirely, it doesn't discard EventSystem selection necessarily.
            // Also note that if we arrive here through eventSystem.SetSelectedGameObject -> OnDeselect,
            // eventSystem.currentSelectedGameObject will still have its old value and we can't reaffect it immediately.
        }

        private bool m_Selecting;
        public void OnSelect(BaseEventData eventData)
        {
            m_Selecting = true;
            try
            {
                // This shouldn't conflict with EditorWindow calling Panel.Focus (only on Editor-type panels).
                m_Panel?.Focus();
            }
            finally
            {
                m_Selecting = false;
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_Panel?.Blur();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData))
                return;

            using (var e = PointerMoveEvent.GetPooled(m_PointerEvent))
            {
                SendEvent(e, eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData, PointerEventType.Up))
                return;

            using (var e = PointerUpEvent.GetPooled(m_PointerEvent))
            {
                SendEvent(e, eventData);

                if (e.pressedButtons == 0)
                    PointerDeviceState.SetPlayerPanelWithSoftPointerCapture(e.pointerId, null);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData, PointerEventType.Down))
                return;

            if (eventSystem != null)
                eventSystem.SetSelectedGameObject(selectableGameObject);

            using (var e = PointerDownEvent.GetPooled(m_PointerEvent))
            {
                SendEvent(e, eventData);

                PointerDeviceState.SetPlayerPanelWithSoftPointerCapture(e.pointerId, m_Panel);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData))
                return;

            // If a pointer exit is called while the pointer is still on top of this object, it means
            // there's something else removing the pointer, so we might need to send a PointerCancelEvent.
            // This is necessary for touch pointers that are being released, because in UGUI the object
            // that was last hovered will not always be the one receiving the pointer up.
            if (eventData.pointerCurrentRaycast.gameObject == gameObject &&
                eventData.pointerPressRaycast.gameObject != gameObject &&
                m_PointerEvent.pointerId != PointerId.mousePointerId)
            {
                using (var e = PointerCancelEvent.GetPooled(m_PointerEvent))
                {
                    SendEvent(e, eventData);
                }
            }

            m_Panel.PointerLeavesPanel(m_PointerEvent.pointerId, m_PointerEvent.position);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData))
                return;

            m_Panel.PointerEntersPanel(m_PointerEvent.pointerId, m_PointerEvent.position);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (m_Panel == null)
                return;

            // Allow KeyDown/KeyUp events to be processed before navigation events.
            var target = currentFocusedElement ?? m_Panel.visualTree;
            ProcessImguiEvents(target);

            using (var e = NavigationSubmitEvent.GetPooled(s_Modifiers))
            {
                e.target = target;
                SendEvent(e, eventData);
            }
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (m_Panel == null)
                return;

            // Allow KeyDown/KeyUp events to be processed before navigation events.
            var target = currentFocusedElement ?? m_Panel.visualTree;
            ProcessImguiEvents(target);

            using (var e = NavigationCancelEvent.GetPooled(s_Modifiers))
            {
                e.target = target;
                SendEvent(e, eventData);
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            if (m_Panel == null)
                return;

            // Allow KeyDown/KeyUp events to be processed before navigation events.
            var target = currentFocusedElement ?? m_Panel.visualTree;
            ProcessImguiEvents(target);

            using (var e = NavigationMoveEvent.GetPooled(eventData.moveVector, s_Modifiers))
            {
                e.target = target;
                SendEvent(e, eventData);
            }

            // TODO: if runtime panel has no internal navigation, switch to the next UGUI selectable element.
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (m_Panel == null || !ReadPointerData(m_PointerEvent, eventData))
                return;

            var scrollDelta = eventData.scrollDelta;
            scrollDelta.y = -scrollDelta.y;

            const float kPixelPerLine = 20;
            // The old input system reported scroll deltas in lines, we report pixels.
            // Need to scale as the UI system expects lines.
            scrollDelta /= kPixelPerLine;

            using (var e = WheelEvent.GetPooled(scrollDelta, m_PointerEvent))
            {
                SendEvent(e, eventData);
            }
        }

        private void SendEvent(EventBase e, BaseEventData sourceEventData)
        {
            //e.runtimeEventData = sourceEventData;
            m_Panel.SendEvent(e);
            if (e.isPropagationStopped)
                sourceEventData.Use();
        }

        private void SendEvent(EventBase e, Event sourceEvent)
        {
            m_Panel.SendEvent(e);
            if (e.isPropagationStopped)
                sourceEvent.Use();
        }

        void Update()
        {
            if (isCurrentFocusedPanel)
                ProcessImguiEvents(currentFocusedElement ?? m_Panel.visualTree);
        }

        void LateUpdate()
        {
            // Empty the Event queue, look for EventModifiers.
            ProcessImguiEvents(null);
        }

        private Event m_Event = new Event();
        private static EventModifiers s_Modifiers = EventModifiers.None;

        // Send IMGUI events to given focus-based target, if any, or simply flush the event queue if not.
        // For uniformity of composite events (keyDown vs navigation), target should remain the same
        // throughout the entire processing cycle.
        void ProcessImguiEvents(Focusable target)
        {
            bool first = true;

            while (Event.PopEvent(m_Event))
            {
                if (m_Event.type == EventType.Ignore || m_Event.type == EventType.Repaint ||
                    m_Event.type == EventType.Layout)
                    continue;

                s_Modifiers = first ? m_Event.modifiers : (s_Modifiers | m_Event.modifiers);
                first = false;

                if (target != null)
                {
                    ProcessKeyboardEvent(m_Event, target);
                    if (eventSystem.sendNavigationEvents)
                        ProcessTabEvent(m_Event, target);
                }
            }
        }

        void ProcessKeyboardEvent(Event e, Focusable target)
        {
            if (e.type == EventType.KeyUp)
            {
                SendKeyUpEvent(e, target);
            }
            else if (e.type == EventType.KeyDown)
            {
                SendKeyDownEvent(e, target);
            }
        }

        // TODO: add an ITabHandler interface
        void ProcessTabEvent(Event e, Focusable target)
        {
            if (e.ShouldSendNavigationMoveEventRuntime())
            {
                SendTabEvent(e, e.shift ? NavigationMoveEvent.Direction.Previous : NavigationMoveEvent.Direction.Next, target);
            }
        }

        private void SendTabEvent(Event e, NavigationMoveEvent.Direction direction, Focusable target)
        {
            using (var ev = NavigationMoveEvent.GetPooled(direction, s_Modifiers))
            {
                ev.target = target;
                SendEvent(ev, e);
            }
        }

        private void SendKeyUpEvent(Event e, Focusable target)
        {
            using (var ev = KeyUpEvent.GetPooled('\0', e.keyCode, e.modifiers))
            {
                ev.target = target;
                SendEvent(ev, e);
            }
        }

        private void SendKeyDownEvent(Event e, Focusable target)
        {
            using (var ev = KeyDownEvent.GetPooled(e.character, e.keyCode, e.modifiers))
            {
                ev.target = target;
                SendEvent(ev, e);
            }
        }

        private bool ReadPointerData(PointerEvent pe, PointerEventData eventData, PointerEventType eventType = PointerEventType.Default)
        {
            if (eventSystem == null || eventSystem.currentInputModule == null)
                return false;

            pe.Read(this, eventData, eventType);

            // PointerEvents making it this far have been validated by PanelRaycaster already
            m_Panel.ScreenToPanel(pe.position, pe.deltaPosition,
                out var panelPosition, out var panelDelta, allowOutside:true);

            pe.SetPosition(panelPosition, panelDelta);
            return true;
        }

        enum PointerEventType
        {
            Default, Down, Up
        }

        class PointerEvent : IPointerEvent
        {
            public int pointerId { get; private set; }
            public string pointerType { get; private set; }
            public bool isPrimary { get; private set; }
            public int button { get; private set; }
            public int pressedButtons { get; private set; }
            public Vector3 position { get; private set; }
            public Vector3 localPosition { get; private set; }
            public Vector3 deltaPosition { get; private set; }
            public float deltaTime { get; private set; }
            public int clickCount { get; private set; }
            public float pressure { get; private set; }
            public float tangentialPressure { get; private set; }
            public float altitudeAngle { get; private set; }
            public float azimuthAngle { get; private set; }
            public float twist { get; private set; }
            public Vector2 tilt { get; private set; }
            public PenStatus penStatus { get; private set; }
            public Vector2 radius { get; private set; }
            public Vector2 radiusVariance { get; private set; }
            public EventModifiers modifiers { get; private set; }

            public bool shiftKey => (modifiers & EventModifiers.Shift) != 0;
            public bool ctrlKey => (modifiers & EventModifiers.Control) != 0;
            public bool commandKey => (modifiers & EventModifiers.Command) != 0;
            public bool altKey => (modifiers & EventModifiers.Alt) != 0;

            public bool actionKey =>
                Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer
                ? commandKey
                : ctrlKey;

            public void Read(PanelEventHandler self, PointerEventData eventData, PointerEventType eventType)
            {
                pointerId = self.eventSystem.currentInputModule.ConvertUIToolkitPointerId(eventData);

                bool InRange(int i, int start, int count) => i >= start && i < start + count;

                pointerType =
                    InRange(pointerId, PointerId.touchPointerIdBase, PointerId.touchPointerCount) ? PointerType.touch :
                    InRange(pointerId, PointerId.penPointerIdBase, PointerId.penPointerCount) ? PointerType.pen :
                    PointerType.mouse;

                isPrimary = pointerId == PointerId.mousePointerId ||
                    pointerId == PointerId.touchPointerIdBase ||
                    pointerId == PointerId.penPointerIdBase;

                // Flip Y axis between input and UITK
                var h = Screen.height;

                var eventPosition = Display.RelativeMouseAt(eventData.position);
                if (eventPosition != Vector3.zero)
                {
                    // We support multiple display and display identification based on event position.

                    int eventDisplayIndex = (int)eventPosition.z;
                    if (eventDisplayIndex > 0 && eventDisplayIndex < Display.displays.Length)
                        h = Display.displays[eventDisplayIndex].systemHeight;
                }
                else
                {
                    eventPosition = eventData.position;
                }

                var delta = eventData.delta;
                eventPosition.y = h - eventPosition.y;
                delta.y = -delta.y;

                localPosition = position = eventPosition;
                deltaPosition = delta;

                deltaTime = 0; //TODO: find out what's expected here. Time since last frame? Since last sent event?
                pressure = eventData.pressure;
                tangentialPressure = eventData.tangentialPressure;
                altitudeAngle = eventData.altitudeAngle;
                azimuthAngle = eventData.azimuthAngle;
                twist = eventData.twist;
                tilt = eventData.tilt;
                penStatus = eventData.penStatus;
                radius = eventData.radius;
                radiusVariance = eventData.radiusVariance;

                modifiers = s_Modifiers;

                if (eventType == PointerEventType.Default)
                {
                    button = -1;
                    clickCount = 0;
                }
                else
                {
                    button = Mathf.Max(0, (int)eventData.button);
                    clickCount = eventData.clickCount;

                    if (eventType == PointerEventType.Down)
                    {
                        // Case 1379054: UIToolkit assumes clickCount is increased before PointerDown, but UGUI does it after.
                        clickCount++;

                        PointerDeviceState.PressButton(pointerId, button);
                    }
                    else if (eventType == PointerEventType.Up)
                    {
                        PointerDeviceState.ReleaseButton(pointerId, button);
                    }

                    clickCount = Mathf.Max(1, clickCount);
                }

                pressedButtons = PointerDeviceState.GetPressedButtons(pointerId);
            }

            public void SetPosition(Vector3 positionOverride, Vector3 deltaOverride)
            {
                localPosition = position = positionOverride;
                deltaPosition = deltaOverride;
            }
        }
    }
#endif
}
