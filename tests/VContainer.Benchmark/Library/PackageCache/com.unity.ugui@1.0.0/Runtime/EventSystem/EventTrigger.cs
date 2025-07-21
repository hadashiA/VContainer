using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event/Event Trigger")]
    /// <summary>
    /// Receives events from the EventSystem and calls registered functions for each event.
    /// </summary>
    /// <remarks>
    /// The EventTrigger can be used to specify functions you wish to be called for each EventSystem event.
    /// You can assign multiple functions to a single event and whenever the EventTrigger receives that event it will call those functions in the order they were provided.
    ///
    /// NOTE: Attaching this component to a GameObject will make that object intercept ALL events, and no events will propagate to parent objects.
    /// </remarks>
    /// <example>
    /// There are two ways to intercept events: You could extend EventTrigger, and override the functions for the events you are interested in intercepting; as shown in this example:
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    /// using UnityEngine.EventSystems;
    ///
    /// public class EventTriggerExample : EventTrigger
    /// {
    ///     public override void OnBeginDrag(PointerEventData data)
    ///     {
    ///         Debug.Log("OnBeginDrag called.");
    ///     }
    ///
    ///     public override void OnCancel(BaseEventData data)
    ///     {
    ///         Debug.Log("OnCancel called.");
    ///     }
    ///
    ///     public override void OnDeselect(BaseEventData data)
    ///     {
    ///         Debug.Log("OnDeselect called.");
    ///     }
    ///
    ///     public override void OnDrag(PointerEventData data)
    ///     {
    ///         Debug.Log("OnDrag called.");
    ///     }
    ///
    ///     public override void OnDrop(PointerEventData data)
    ///     {
    ///         Debug.Log("OnDrop called.");
    ///     }
    ///
    ///     public override void OnEndDrag(PointerEventData data)
    ///     {
    ///         Debug.Log("OnEndDrag called.");
    ///     }
    ///
    ///     public override void OnInitializePotentialDrag(PointerEventData data)
    ///     {
    ///         Debug.Log("OnInitializePotentialDrag called.");
    ///     }
    ///
    ///     public override void OnMove(AxisEventData data)
    ///     {
    ///         Debug.Log("OnMove called.");
    ///     }
    ///
    ///     public override void OnPointerClick(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerClick called.");
    ///     }
    ///
    ///     public override void OnPointerDown(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerDown called.");
    ///     }
    ///
    ///     public override void OnPointerEnter(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerEnter called.");
    ///     }
    ///
    ///     public override void OnPointerExit(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerExit called.");
    ///     }
    ///
    ///     public override void OnPointerUp(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerUp called.");
    ///     }
    ///
    ///     public override void OnScroll(PointerEventData data)
    ///     {
    ///         Debug.Log("OnScroll called.");
    ///     }
    ///
    ///     public override void OnSelect(BaseEventData data)
    ///     {
    ///         Debug.Log("OnSelect called.");
    ///     }
    ///
    ///     public override void OnSubmit(BaseEventData data)
    ///     {
    ///         Debug.Log("OnSubmit called.");
    ///     }
    ///
    ///     public override void OnUpdateSelected(BaseEventData data)
    ///     {
    ///         Debug.Log("OnUpdateSelected called.");
    ///     }
    /// }
    /// ]]>
    ///</code>
    /// or you can specify individual delegates:
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    /// using UnityEngine.EventSystems;
    ///
    ///
    /// public class EventTriggerDelegateExample : MonoBehaviour
    /// {
    ///     void Start()
    ///     {
    ///         EventTrigger trigger = GetComponent<EventTrigger>();
    ///         EventTrigger.Entry entry = new EventTrigger.Entry();
    ///         entry.eventID = EventTriggerType.PointerDown;
    ///         entry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
    ///         trigger.triggers.Add(entry);
    ///     }
    ///
    ///     public void OnPointerDownDelegate(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerDownDelegate called.");
    ///     }
    /// }
    /// ]]>
    ///</code>
    /// </example>
    public class EventTrigger :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IScrollHandler,
        IUpdateSelectedHandler,
        ISelectHandler,
        IDeselectHandler,
        IMoveHandler,
        ISubmitHandler,
        ICancelHandler
    {
        [Serializable]
        /// <summary>
        /// UnityEvent class for Triggers.
        /// </summary>
        public class TriggerEvent : UnityEvent<BaseEventData>
        {}

        [Serializable]
        /// <summary>
        /// An Entry in the EventSystem delegates list.
        /// </summary>
        /// <remarks>
        /// It stores the callback and which event type should this callback be fired.
        /// </remarks>
        public class Entry
        {
            /// <summary>
            /// What type of event is the associated callback listening for.
            /// </summary>
            public EventTriggerType eventID = EventTriggerType.PointerClick;

            /// <summary>
            /// The desired TriggerEvent to be Invoked.
            /// </summary>
            public TriggerEvent callback = new TriggerEvent();
        }

        [FormerlySerializedAs("delegates")]
        [SerializeField]
        private List<Entry> m_Delegates;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Please use triggers instead (UnityUpgradable) -> triggers", true)]
        public List<Entry> delegates { get { return triggers; } set { triggers = value; } }

        protected EventTrigger()
        {}

        /// <summary>
        /// All the functions registered in this EventTrigger
        /// </summary>
        public List<Entry> triggers
        {
            get
            {
                if (m_Delegates == null)
                    m_Delegates = new List<Entry>();
                return m_Delegates;
            }
            set { m_Delegates = value; }
        }

        private void Execute(EventTriggerType id, BaseEventData eventData)
        {
            for (int i = 0; i < triggers.Count; ++i)
            {
                var ent = triggers[i];
                if (ent.eventID == id && ent.callback != null)
                    ent.callback.Invoke(eventData);
            }
        }

        /// <summary>
        /// Called by the EventSystem when the pointer enters the object associated with this EventTrigger.
        /// </summary>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerEnter, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when the pointer exits the object associated with this EventTrigger.
        /// </summary>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerExit, eventData);
        }

        /// <summary>
        /// Called by the EventSystem every time the pointer is moved during dragging.
        /// </summary>
        public virtual void OnDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.Drag, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when an object accepts a drop.
        /// </summary>
        public virtual void OnDrop(PointerEventData eventData)
        {
            Execute(EventTriggerType.Drop, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a PointerDown event occurs.
        /// </summary>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerDown, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a PointerUp event occurs.
        /// </summary>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerUp, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Click event occurs.
        /// </summary>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerClick, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Select event occurs.
        /// </summary>
        public virtual void OnSelect(BaseEventData eventData)
        {
            Execute(EventTriggerType.Select, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a new object is being selected.
        /// </summary>
        public virtual void OnDeselect(BaseEventData eventData)
        {
            Execute(EventTriggerType.Deselect, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a new Scroll event occurs.
        /// </summary>
        public virtual void OnScroll(PointerEventData eventData)
        {
            Execute(EventTriggerType.Scroll, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Move event occurs.
        /// </summary>
        public virtual void OnMove(AxisEventData eventData)
        {
            Execute(EventTriggerType.Move, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when the object associated with this EventTrigger is updated.
        /// </summary>
        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            Execute(EventTriggerType.UpdateSelected, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a drag has been found, but before it is valid to begin the drag.
        /// </summary>
        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.InitializePotentialDrag, eventData);
        }

        /// <summary>
        /// Called before a drag is started.
        /// </summary>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.BeginDrag, eventData);
        }

        /// <summary>
        /// Called by the EventSystem once dragging ends.
        /// </summary>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.EndDrag, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Submit event occurs.
        /// </summary>
        public virtual void OnSubmit(BaseEventData eventData)
        {
            Execute(EventTriggerType.Submit, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Cancel event occurs.
        /// </summary>
        public virtual void OnCancel(BaseEventData eventData)
        {
            Execute(EventTriggerType.Cancel, eventData);
        }
    }
}
