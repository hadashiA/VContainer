using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event/Event System")]
    [DisallowMultipleComponent]
    /// <summary>
    /// Handles input, raycasting, and sending events.
    /// </summary>
    /// <remarks>
    /// The EventSystem is responsible for processing and handling events in a Unity scene. A scene should only contain one EventSystem. The EventSystem works in conjunction with a number of modules and mostly just holds state and delegates functionality to specific, overrideable components.
    /// When the EventSystem is started it searches for any BaseInputModules attached to the same GameObject and adds them to an internal list. On update each attached module receives an UpdateModules call, where the module can modify internal state. After each module has been Updated the active module has the Process call executed.This is where custom module processing can take place.
    /// </remarks>
    public class EventSystem : UIBehaviour
    {
        private List<BaseInputModule> m_SystemInputModules = new List<BaseInputModule>();

        private BaseInputModule m_CurrentInputModule;

        private  static List<EventSystem> m_EventSystems = new List<EventSystem>();

        /// <summary>
        /// Return the current EventSystem.
        /// </summary>
        public static EventSystem current
        {
            get { return m_EventSystems.Count > 0 ? m_EventSystems[0] : null; }
            set
            {
                int index = m_EventSystems.IndexOf(value);

                if (index > 0)
                {
                    m_EventSystems.RemoveAt(index);
                    m_EventSystems.Insert(0, value);
                }
                else if (index < 0)
                {
                    Debug.LogError("Failed setting EventSystem.current to unknown EventSystem " + value);
                }
            }
        }

        [SerializeField]
        [FormerlySerializedAs("m_Selected")]
        private GameObject m_FirstSelected;

        [SerializeField]
        private bool m_sendNavigationEvents = true;

        /// <summary>
        /// Should the EventSystem allow navigation events (move / submit / cancel).
        /// </summary>
        public bool sendNavigationEvents
        {
            get { return m_sendNavigationEvents; }
            set { m_sendNavigationEvents = value; }
        }

        [SerializeField]
        private int m_DragThreshold = 10;

        /// <summary>
        /// The soft area for dragging in pixels.
        /// </summary>
        public int pixelDragThreshold
        {
            get { return m_DragThreshold; }
            set { m_DragThreshold = value; }
        }

        private GameObject m_CurrentSelected;

        /// <summary>
        /// The currently active EventSystems.BaseInputModule.
        /// </summary>
        public BaseInputModule currentInputModule
        {
            get { return m_CurrentInputModule; }
        }

        /// <summary>
        /// Only one object can be selected at a time. Think: controller-selected button.
        /// </summary>
        public GameObject firstSelectedGameObject
        {
            get { return m_FirstSelected; }
            set { m_FirstSelected = value; }
        }

        /// <summary>
        /// The GameObject currently considered active by the EventSystem.
        /// </summary>
        public GameObject currentSelectedGameObject
        {
            get { return m_CurrentSelected; }
        }

        [Obsolete("lastSelectedGameObject is no longer supported")]
        public GameObject lastSelectedGameObject
        {
            get { return null; }
        }

        private bool m_HasFocus = true;

        /// <summary>
        /// Flag to say whether the EventSystem thinks it should be paused or not based upon focused state.
        /// </summary>
        /// <remarks>
        /// Used to determine inside the individual InputModules if the module should be ticked while the application doesnt have focus.
        /// </remarks>
        public bool isFocused
        {
            get { return m_HasFocus; }
        }

        protected EventSystem()
        {}

        /// <summary>
        /// Recalculate the internal list of BaseInputModules.
        /// </summary>
        public void UpdateModules()
        {
            GetComponents(m_SystemInputModules);
            var systemInputModulesCount = m_SystemInputModules.Count;
            for (int i = systemInputModulesCount - 1; i >= 0; i--)
            {
                if (m_SystemInputModules[i] && m_SystemInputModules[i].IsActive())
                    continue;

                m_SystemInputModules.RemoveAt(i);
            }
        }

        private bool m_SelectionGuard;

        /// <summary>
        /// Returns true if the EventSystem is already in a SetSelectedGameObject.
        /// </summary>
        public bool alreadySelecting
        {
            get { return m_SelectionGuard; }
        }

        /// <summary>
        /// Set the object as selected. Will send an OnDeselect the the old selected object and OnSelect to the new selected object.
        /// </summary>
        /// <param name="selected">GameObject to select.</param>
        /// <param name="pointer">Associated EventData.</param>
        public void SetSelectedGameObject(GameObject selected, BaseEventData pointer)
        {
            if (m_SelectionGuard)
            {
                Debug.LogError("Attempting to select " + selected +  "while already selecting an object.");
                return;
            }

            m_SelectionGuard = true;
            if (selected == m_CurrentSelected)
            {
                m_SelectionGuard = false;
                return;
            }

            // Debug.Log("Selection: new (" + selected + ") old (" + m_CurrentSelected + ")");
            ExecuteEvents.Execute(m_CurrentSelected, pointer, ExecuteEvents.deselectHandler);
            m_CurrentSelected = selected;
            ExecuteEvents.Execute(m_CurrentSelected, pointer, ExecuteEvents.selectHandler);
            m_SelectionGuard = false;
        }

        private BaseEventData m_DummyData;
        private BaseEventData baseEventDataCache
        {
            get
            {
                if (m_DummyData == null)
                    m_DummyData = new BaseEventData(this);

                return m_DummyData;
            }
        }

        /// <summary>
        /// Set the object as selected. Will send an OnDeselect the the old selected object and OnSelect to the new selected object.
        /// </summary>
        /// <param name="selected">GameObject to select.</param>
        public void SetSelectedGameObject(GameObject selected)
        {
            SetSelectedGameObject(selected, baseEventDataCache);
        }

        private static int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
        {
            if (lhs.module != rhs.module)
            {
                var lhsEventCamera = lhs.module.eventCamera;
                var rhsEventCamera = rhs.module.eventCamera;
                if (lhsEventCamera != null && rhsEventCamera != null && lhsEventCamera.depth != rhsEventCamera.depth)
                {
                    // need to reverse the standard compareTo
                    if (lhsEventCamera.depth < rhsEventCamera.depth)
                        return 1;
                    if (lhsEventCamera.depth == rhsEventCamera.depth)
                        return 0;

                    return -1;
                }

                if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
                    return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);

                if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
                    return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
            }

            // Renderer sorting
            if (lhs.sortingLayer != rhs.sortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
                var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
                return rid.CompareTo(lid);
            }

            if (lhs.sortingOrder != rhs.sortingOrder)
                return rhs.sortingOrder.CompareTo(lhs.sortingOrder);

            // comparing depth only makes sense if the two raycast results have the same root canvas (case 912396)
            if (lhs.depth != rhs.depth && lhs.module.rootRaycaster == rhs.module.rootRaycaster)
                return rhs.depth.CompareTo(lhs.depth);

            if (lhs.distance != rhs.distance)
                return lhs.distance.CompareTo(rhs.distance);

            #if PACKAGE_PHYSICS2D
			// Sorting group
            if (lhs.sortingGroupID != SortingGroup.invalidSortingGroupID && rhs.sortingGroupID != SortingGroup.invalidSortingGroupID)
            {
                if (lhs.sortingGroupID != rhs.sortingGroupID)
                    return lhs.sortingGroupID.CompareTo(rhs.sortingGroupID);
                if (lhs.sortingGroupOrder != rhs.sortingGroupOrder)
                    return rhs.sortingGroupOrder.CompareTo(lhs.sortingGroupOrder);
            }
            #endif

            return lhs.index.CompareTo(rhs.index);
        }

        private static readonly Comparison<RaycastResult> s_RaycastComparer = RaycastComparer;

        /// <summary>
        /// Raycast into the scene using all configured BaseRaycasters.
        /// </summary>
        /// <param name="eventData">Current pointer data.</param>
        /// <param name="raycastResults">List of 'hits' to populate.</param>
        public void RaycastAll(PointerEventData eventData, List<RaycastResult> raycastResults)
        {
            raycastResults.Clear();
            var modules = RaycasterManager.GetRaycasters();
            var modulesCount = modules.Count;
            for (int i = 0; i < modulesCount; ++i)
            {
                var module = modules[i];
                if (module == null || !module.IsActive())
                    continue;

                module.Raycast(eventData, raycastResults);
            }

            raycastResults.Sort(s_RaycastComparer);
        }

        /// <summary>
        /// Is the pointer with the given ID over an EventSystem object?
        /// </summary>
        public bool IsPointerOverGameObject()
        {
            return IsPointerOverGameObject(PointerInputModule.kMouseLeftId);
        }

        /// <summary>
        /// Is the pointer with the given ID over an EventSystem object?
        /// </summary>
        /// <remarks>
        /// If you use IsPointerOverGameObject() without a parameter, it points to the "left mouse button" (pointerId = -1); therefore when you use IsPointerOverGameObject for touch, you should consider passing a pointerId to it
        /// Note that for touch, IsPointerOverGameObject should be used with ''OnMouseDown()'' or ''Input.GetMouseButtonDown(0)'' or ''Input.GetTouch(0).phase == TouchPhase.Began''.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.EventSystems;
        ///
        /// public class MouseExample : MonoBehaviour
        /// {
        ///     void Update()
        ///     {
        ///         // Check if the left mouse button was clicked
        ///         if (Input.GetMouseButtonDown(0))
        ///         {
        ///             // Check if the mouse was clicked over a UI element
        ///             if (EventSystem.current.IsPointerOverGameObject())
        ///             {
        ///                 Debug.Log("Clicked on the UI");
        ///             }
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public bool IsPointerOverGameObject(int pointerId)
        {
            return m_CurrentInputModule != null && m_CurrentInputModule.IsPointerOverGameObject(pointerId);
        }

        // This code is disabled unless the UI Toolkit package or the com.unity.modules.uielements module are present.
        // The UIElements module is always present in the Editor but it can be stripped from a project build if unused.
#if PACKAGE_UITOOLKIT
        private struct UIToolkitOverrideConfig
        {
            public EventSystem activeEventSystem;
            public bool sendEvents;
            public bool createPanelGameObjectsOnStart;
        }

        private static UIToolkitOverrideConfig s_UIToolkitOverride = new UIToolkitOverrideConfig
        {
            activeEventSystem = null,
            sendEvents = true,
            createPanelGameObjectsOnStart = true
        };

        private bool isUIToolkitActiveEventSystem =>
            s_UIToolkitOverride.activeEventSystem == this || s_UIToolkitOverride.activeEventSystem == null;

        private bool sendUIToolkitEvents =>
            s_UIToolkitOverride.sendEvents && isUIToolkitActiveEventSystem;

        private bool createUIToolkitPanelGameObjectsOnStart =>
            s_UIToolkitOverride.createPanelGameObjectsOnStart && isUIToolkitActiveEventSystem;
#endif

        /// <summary>
        /// Sets how UI Toolkit runtime panels receive events and handle selection
        /// when interacting with other objects that use the EventSystem, such as components from the Unity UI package.
        /// </summary>
        /// <param name="activeEventSystem">
        /// The EventSystem used to override UI Toolkit panel events and selection.
        /// If activeEventSystem is null, UI Toolkit panels will use current enabled EventSystem
        /// or, if there is none, the default InputManager-based event system will be used.
        /// </param>
        /// <param name="sendEvents">
        /// If true, UI Toolkit events will come from this EventSystem
        /// instead of the default InputManager-based event system.
        /// </param>
        /// <param name="createPanelGameObjectsOnStart">
        /// If true, UI Toolkit panels' unassigned selectableGameObject will be automatically initialized
        /// with children GameObjects of this EventSystem on Start.
        /// </param>
        public static void SetUITookitEventSystemOverride(EventSystem activeEventSystem, bool sendEvents = true, bool createPanelGameObjectsOnStart = true)
        {
#if PACKAGE_UITOOLKIT
            UIElementsRuntimeUtility.UnregisterEventSystem(UIElementsRuntimeUtility.activeEventSystem);

            s_UIToolkitOverride = new UIToolkitOverrideConfig
            {
                activeEventSystem = activeEventSystem,
                sendEvents = sendEvents,
                createPanelGameObjectsOnStart = createPanelGameObjectsOnStart,
            };

            if (sendEvents)
            {
                var eventSystem = activeEventSystem != null ? activeEventSystem : EventSystem.current;
                if (eventSystem.isActiveAndEnabled)
                    UIElementsRuntimeUtility.RegisterEventSystem(activeEventSystem);
            }
#endif
        }

#if PACKAGE_UITOOLKIT
        private bool m_Started;
        private bool m_IsTrackingUIToolkitPanels;

        private void StartTrackingUIToolkitPanels()
        {
            if (createUIToolkitPanelGameObjectsOnStart)
            {
                foreach (BaseRuntimePanel panel in UIElementsRuntimeUtility.GetSortedPlayerPanels())
                {
                    CreateUIToolkitPanelGameObject(panel);
                }
                UIElementsRuntimeUtility.onCreatePanel += CreateUIToolkitPanelGameObject;
                m_IsTrackingUIToolkitPanels = true;
            }
        }

        private void StopTrackingUIToolkitPanels()
        {
            if (m_IsTrackingUIToolkitPanels)
            {
                UIElementsRuntimeUtility.onCreatePanel -= CreateUIToolkitPanelGameObject;
                m_IsTrackingUIToolkitPanels = false;
            }
        }

        private void CreateUIToolkitPanelGameObject(BaseRuntimePanel panel)
        {
            if (panel.selectableGameObject == null)
            {
                var go = new GameObject(panel.name, typeof(PanelEventHandler), typeof(PanelRaycaster));
                go.transform.SetParent(transform);
                panel.selectableGameObject = go;
                panel.destroyed += () => DestroyImmediate(go);
            }
        }
#endif

        protected override void Start()
        {
            base.Start();

#if PACKAGE_UITOOLKIT
            m_Started = true;
            StartTrackingUIToolkitPanels();
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_EventSystems.Add(this);

#if PACKAGE_UITOOLKIT
            if (m_Started && !m_IsTrackingUIToolkitPanels)
            {
                StartTrackingUIToolkitPanels();
            }
            if (sendUIToolkitEvents)
            {
                UIElementsRuntimeUtility.RegisterEventSystem(this);
            }
#endif
        }

        protected override void OnDisable()
        {
#if PACKAGE_UITOOLKIT
            StopTrackingUIToolkitPanels();
            UIElementsRuntimeUtility.UnregisterEventSystem(this);
#endif

            if (m_CurrentInputModule != null)
            {
                m_CurrentInputModule.DeactivateModule();
                m_CurrentInputModule = null;
            }

            m_EventSystems.Remove(this);

            base.OnDisable();
        }

        private void TickModules()
        {
            var systemInputModulesCount = m_SystemInputModules.Count;
            for (var i = 0; i < systemInputModulesCount; i++)
            {
                if (m_SystemInputModules[i] != null)
                    m_SystemInputModules[i].UpdateModule();
            }
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            m_HasFocus = hasFocus;
            if (!m_HasFocus)
                TickModules();
        }

        protected virtual void Update()
        {
            if (current != this)
                return;
            TickModules();

            bool changedModule = false;
            var systemInputModulesCount = m_SystemInputModules.Count;
            for (var i = 0; i < systemInputModulesCount; i++)
            {
                var module = m_SystemInputModules[i];
                if (module.IsModuleSupported() && module.ShouldActivateModule())
                {
                    if (m_CurrentInputModule != module)
                    {
                        ChangeEventModule(module);
                        changedModule = true;
                    }
                    break;
                }
            }

            // no event module set... set the first valid one...
            if (m_CurrentInputModule == null)
            {
                for (var i = 0; i < systemInputModulesCount; i++)
                {
                    var module = m_SystemInputModules[i];
                    if (module.IsModuleSupported())
                    {
                        ChangeEventModule(module);
                        changedModule = true;
                        break;
                    }
                }
            }

            if (!changedModule && m_CurrentInputModule != null)
                m_CurrentInputModule.Process();

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                int eventSystemCount = 0;
                for (int i = 0; i < m_EventSystems.Count; i++)
                {
                    if (m_EventSystems[i].GetType() == typeof(EventSystem))
                        eventSystemCount++;
                }

                if (eventSystemCount > 1)
                    Debug.LogWarning("There are " + eventSystemCount + " event systems in the scene. Please ensure there is always exactly one event system in the scene");
            }
#endif
        }

        private void ChangeEventModule(BaseInputModule module)
        {
            if (m_CurrentInputModule == module)
                return;

            if (m_CurrentInputModule != null)
                m_CurrentInputModule.DeactivateModule();

            if (module != null)
                module.ActivateModule();
            m_CurrentInputModule = module;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>Selected:</b>" + currentSelectedGameObject);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(m_CurrentInputModule != null ? m_CurrentInputModule.ToString() : "No module");
            return sb.ToString();
        }
    }
}
