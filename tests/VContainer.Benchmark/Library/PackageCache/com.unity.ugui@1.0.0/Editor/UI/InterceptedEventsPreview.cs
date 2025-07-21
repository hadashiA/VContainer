using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.EventSystems;

namespace UnityEditor.Events
{
    [CustomPreview(typeof(GameObject))]
    /// <summary>
    ///   Custom preview drawing that will draw the intercepted events of a given object.
    /// </summary>
    class InterceptedEventsPreview : ObjectPreview
    {
        protected class ComponentInterceptedEvents
        {
            public GUIContent componentName;
            public int[] interceptedEvents;
        }

        class Styles
        {
            public GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            public GUIStyle componentName = new GUIStyle(EditorStyles.boldLabel);

            public Styles()
            {
                Color fontColor = new Color(0.7f, 0.7f, 0.7f);
                labelStyle.padding.right += 20;
                labelStyle.normal.textColor    = fontColor;
                labelStyle.active.textColor    = fontColor;
                labelStyle.focused.textColor   = fontColor;
                labelStyle.hover.textColor     = fontColor;
                labelStyle.onNormal.textColor  = fontColor;
                labelStyle.onActive.textColor  = fontColor;
                labelStyle.onFocused.textColor = fontColor;
                labelStyle.onHover.textColor   = fontColor;

                componentName.normal.textColor = fontColor;
                componentName.active.textColor = fontColor;
                componentName.focused.textColor = fontColor;
                componentName.hover.textColor = fontColor;
                componentName.onNormal.textColor = fontColor;
                componentName.onActive.textColor = fontColor;
                componentName.onFocused.textColor = fontColor;
                componentName.onHover.textColor = fontColor;
            }
        }

        private Dictionary<GameObject, List<ComponentInterceptedEvents>> m_TargetEvents;
        private bool m_InterceptsAnyEvent = false;
        private GUIContent m_Title;
        private Styles m_Styles;

        public override void Initialize(UnityEngine.Object[] targets)
        {
            Profiler.BeginSample("ComponentInterceptedEvents.Initialize");

            base.Initialize(targets);
            m_TargetEvents = new Dictionary<GameObject, List<ComponentInterceptedEvents>>(targets.Length);
            m_InterceptsAnyEvent = false;
            for (int i = 0; i < targets.Length; ++i)
            {
                GameObject go = targets[i] as GameObject;
                List<ComponentInterceptedEvents> interceptedEvents = GetEventsInfo(go);
                m_TargetEvents.Add(go, interceptedEvents);
                if (interceptedEvents.Any())
                    m_InterceptsAnyEvent = true;
            }
            Profiler.EndSample();
        }

        public override GUIContent GetPreviewTitle()
        {
            if (m_Title == null)
            {
                m_Title = EditorGUIUtility.TrTextContent("Intercepted Events");
            }
            return m_Title;
        }

        public override bool HasPreviewGUI()
        {
            return m_TargetEvents != null && m_InterceptsAnyEvent;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            Profiler.BeginSample("InterceptedEventsPreview.OnPreviewGUI");


            if (m_Styles == null)
                m_Styles = new Styles();

            Vector2 maxEventLabelSize = Vector2.zero;
            int totalInterceptedEvents = 0;

            List<ComponentInterceptedEvents> componentIncerceptedEvents = m_TargetEvents[target as GameObject];

            // Find out the maximum size needed for any given label.
            foreach (ComponentInterceptedEvents componentInterceptedEvents in componentIncerceptedEvents)
            {
                foreach (int eventIndex in componentInterceptedEvents.interceptedEvents)
                {
                    GUIContent eventContent = s_PossibleEvents[eventIndex];
                    ++totalInterceptedEvents;
                    Vector2 labelSize = m_Styles.labelStyle.CalcSize(eventContent);
                    if (maxEventLabelSize.x < labelSize.x)
                    {
                        maxEventLabelSize.x = labelSize.x;
                    }
                    if (maxEventLabelSize.y < labelSize.y)
                    {
                        maxEventLabelSize.y = labelSize.y;
                    }
                }
            }

            // Apply padding
            RectOffset previewPadding = new RectOffset(-5, -5, -5, -5);
            r = previewPadding.Add(r);

            // Figure out how many rows and columns we can/should have
            int columns = Mathf.Max(Mathf.FloorToInt(r.width / maxEventLabelSize.x), 1);
            int rows = Mathf.Max(totalInterceptedEvents / columns, 1) + componentIncerceptedEvents.Count;

            // Centering
            float initialX = r.x + Mathf.Max(0, (r.width - (maxEventLabelSize.x * columns)) / 2);
            float initialY = r.y + Mathf.Max(0, (r.height - (maxEventLabelSize.y * rows)) / 2);

            Rect labelRect = new Rect(initialX, initialY, maxEventLabelSize.x, maxEventLabelSize.y);
            int currentColumn = 0;
            foreach (ComponentInterceptedEvents componentInterceptedEvents in componentIncerceptedEvents)
            {
                GUI.Label(labelRect, componentInterceptedEvents.componentName, m_Styles.componentName);
                labelRect.y += labelRect.height;
                labelRect.x = initialX;
                foreach (int eventIndex in componentInterceptedEvents.interceptedEvents)
                {
                    GUIContent eventContent = s_PossibleEvents[eventIndex];
                    GUI.Label(labelRect, eventContent, m_Styles.labelStyle);
                    if (currentColumn < columns - 1)
                    {
                        labelRect.x += labelRect.width;
                    }
                    else
                    {
                        labelRect.y += labelRect.height;
                        labelRect.x = initialX;
                    }

                    currentColumn = (currentColumn + 1) % columns;
                }

                if (labelRect.x != initialX)
                {
                    labelRect.y += labelRect.height;
                    labelRect.x = initialX;
                }
            }
            Profiler.EndSample();
        }

        //Lookup cache to avoid recalculating which types uses which events:
        //Caches all interfaces that inherit from IEventSystemHandler
        static List<Type> s_EventSystemInterfaces = null;
        //Caches all GUIContents in a single list to avoid creating too much GUIContent and strings.
        private static List<GUIContent> s_PossibleEvents = null;
        //Caches all events used by each interface
        static Dictionary<Type, List<int>> s_InterfaceEventSystemEvents = null;
        //Caches each concrete type and it's events
        static readonly Dictionary<Type, ComponentInterceptedEvents> s_ComponentEvents2 = new Dictionary<Type, ComponentInterceptedEvents>();


        protected static List<ComponentInterceptedEvents> GetEventsInfo(GameObject gameObject)
        {
            InitializeEvetnsInterfaceCacheIfNeeded();

            List<ComponentInterceptedEvents> componentEvents = new List<ComponentInterceptedEvents>();

            MonoBehaviour[] mbs = gameObject.GetComponents<MonoBehaviour>();

            for (int i = 0, imax = mbs.Length; i < imax; ++i)
            {
                ComponentInterceptedEvents componentEvent = null;

                MonoBehaviour mb = mbs[i];
                if (mb == null)
                    continue;

                Type type = mb.GetType();

                if (!s_ComponentEvents2.ContainsKey(type))
                {
                    List<int> events = null;
                    Profiler.BeginSample("ComponentInterceptedEvents.GetEventsInfo.NewType");
                    if (typeof(IEventSystemHandler).IsAssignableFrom(type))
                    {
                        for (int index = 0; index < s_EventSystemInterfaces.Count; index++)
                        {
                            var eventInterface = s_EventSystemInterfaces[index];
                            if (!eventInterface.IsAssignableFrom(type))
                                continue;

                            if (events == null)
                                events = new List<int>();

                            events.AddRange(s_InterfaceEventSystemEvents[eventInterface]);
                        }
                    }

                    if (events != null)
                    {
                        componentEvent = new ComponentInterceptedEvents();
                        componentEvent.componentName = new GUIContent(type.Name);
                        componentEvent.interceptedEvents = events.OrderBy(index => s_PossibleEvents[index].text).ToArray();
                    }
                    s_ComponentEvents2.Add(type, componentEvent);

                    Profiler.EndSample();
                }
                else
                {
                    componentEvent = s_ComponentEvents2[type];
                }


                if (componentEvent != null)
                {
                    componentEvents.Add(componentEvent);
                }
            }

            return componentEvents;
        }

        private static void InitializeEvetnsInterfaceCacheIfNeeded()
        {
            if (s_EventSystemInterfaces != null)
                return;

            s_EventSystemInterfaces = new List<Type>();
            s_PossibleEvents = new List<GUIContent>();
            s_InterfaceEventSystemEvents = new Dictionary<Type, List<int>>();

            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<IEventSystemHandler>();
            foreach (var type in types)
            {
                if (!type.IsInterface)
                    continue;

                s_EventSystemInterfaces.Add(type);
                List<int> eventIndexList = new List<int>();

                MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int mi = 0; mi < methodInfos.Length; mi++)
                {
                    MethodInfo methodInfo = methodInfos[mi];
                    eventIndexList.Add(s_PossibleEvents.Count);
                    s_PossibleEvents.Add(new GUIContent(methodInfo.Name));
                }
                s_InterfaceEventSystemEvents.Add(type, eventIndexList);
            }
        }
    }
}
