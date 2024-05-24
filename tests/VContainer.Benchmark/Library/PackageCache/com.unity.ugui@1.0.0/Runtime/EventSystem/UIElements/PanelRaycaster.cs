using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;

namespace UnityEngine.UIElements
{
    // This code is disabled unless the UI Toolkit package or the com.unity.modules.uielements module are present.
    // The UIElements module is always present in the Editor but it can be stripped from a project build if unused.
#if PACKAGE_UITOOLKIT
    /// <summary>
    /// A derived BaseRaycaster to raycast against UI Toolkit panel instances at runtime.
    /// </summary>
    [AddComponentMenu("UI Toolkit/Panel Raycaster (UI Toolkit)")]
    public class PanelRaycaster : BaseRaycaster, IRuntimePanelComponent
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

        void RegisterCallbacks()
        {
            if (m_Panel != null)
            {
                m_Panel.destroyed += OnPanelDestroyed;
            }
        }

        void UnregisterCallbacks()
        {
            if (m_Panel != null)
            {
                m_Panel.destroyed -= OnPanelDestroyed;
            }
        }

        void OnPanelDestroyed()
        {
            panel = null;
        }

        private GameObject selectableGameObject => m_Panel?.selectableGameObject;

        public override int sortOrderPriority => Mathf.FloorToInt(m_Panel?.sortingPriority ?? 0f);
        public override int renderOrderPriority => int.MaxValue - (UIElementsRuntimeUtility.s_ResolvedSortingIndexMax - (m_Panel?.resolvedSortingIndex ?? 0));

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (m_Panel == null)
                return;

            var eventPosition = Display.RelativeMouseAt(eventData.position);
            var displayIndex = m_Panel.targetDisplay;

            var originalEventPosition = eventPosition;
            if (eventPosition != Vector3.zero)
            {
                // We support multiple display and display identification based on event position.

                int eventDisplayIndex = (int)eventPosition.z;

                // Discard events that are not part of this display so the user does not interact with multiple displays at once.
                if (eventDisplayIndex != displayIndex)
                    return;
            }
            else
            {
                // The multiple display system is not supported on all platforms, when it is not supported the returned position
                // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
                eventPosition = eventData.position;
#if UNITY_EDITOR
                if (Display.activeEditorGameViewTarget != displayIndex)
                    return;
                eventPosition.z = Display.activeEditorGameViewTarget;
#endif

                // We don't really know in which display the event occurred. We will process the event assuming it occurred in our display.
            }

            var position = eventPosition;
            var delta = eventData.delta;

            float h = Screen.height;
            if (displayIndex > 0 && displayIndex < Display.displays.Length)
            {
                h = Display.displays[displayIndex].systemHeight;
            }

            position.y = h - position.y;
            delta.y = -delta.y;

            var eventSystem = UIElementsRuntimeUtility.activeEventSystem as EventSystem;
            if (eventSystem == null || eventSystem.currentInputModule == null)
                return;
            var pointerId = eventSystem.currentInputModule.ConvertUIToolkitPointerId(eventData);

            var capturingElement = m_Panel.GetCapturingElement(pointerId);
            if (capturingElement is VisualElement ve && ve.panel != m_Panel)
                return;

            var capturingPanel = PointerDeviceState.GetPressedButtons(pointerId) != 0 ?
                                 PointerDeviceState.GetPlayerPanelWithSoftPointerCapture(pointerId) :
                                 null;
            if (capturingPanel != null && capturingPanel != m_Panel)
                return;

            if (capturingElement == null && capturingPanel == null)
            {
                if (!m_Panel.ScreenToPanel(position, delta, out var panelPosition, out _))
                    return;

                var pick = m_Panel.Pick(panelPosition);
                if (pick == null)
                    return;
            }

            resultAppendList.Add(new RaycastResult
            {
                gameObject = selectableGameObject,
                module = this,
                screenPosition = eventPosition,
                displayIndex = m_Panel.targetDisplay,
            });
        }

        public override Camera eventCamera => null;
    }
#endif
}
