using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#pragma warning disable IDE1006 // Unity-specific lower case public property names

namespace Unity.AI.Navigation
{
    /// <summary> Component used to create a navigable link between two NavMesh locations. </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-101)]
    [AddComponentMenu("Navigation/NavMeshLink", 33)]
    [HelpURL(HelpUrls.Manual + "NavMeshLink.html")]
    public class NavMeshLink : MonoBehaviour
    {
        [SerializeField]
        int m_AgentTypeID;

        [SerializeField]
        Vector3 m_StartPoint = new Vector3(0.0f, 0.0f, -2.5f);

        [SerializeField]
        Vector3 m_EndPoint = new Vector3(0.0f, 0.0f, 2.5f);

        [SerializeField]
        float m_Width;

        [SerializeField]
        int m_CostModifier = -1;

        [SerializeField]
        bool m_Bidirectional = true;

        [SerializeField]
        bool m_AutoUpdatePosition;

        [SerializeField]
        int m_Area;

        /// <summary> Gets or sets the type of agent that can use the link. </summary>
        public int agentTypeID { get { return m_AgentTypeID; } set { m_AgentTypeID = value; UpdateLink(); } }

        /// <summary> Gets or sets the position at the middle of the link's start edge. </summary>
        /// <remarks> The position is relative to the GameObject transform. </remarks>
        public Vector3 startPoint { get { return m_StartPoint; } set { m_StartPoint = value; UpdateLink(); } }

        /// <summary> Gets or sets the position at the middle of the link's end edge. </summary>
        /// <remarks> The position is relative to the GameObject transform. </remarks>
        public Vector3 endPoint { get { return m_EndPoint; } set { m_EndPoint = value; UpdateLink(); } }

        /// <summary> The width of the segments making up the ends of the link. </summary>
        /// <remarks> The segments are created perpendicular to the line from start to end,in the XZ plane of the GameObject. </remarks>
        public float width { get { return m_Width; } set { m_Width = value; UpdateLink(); } }

        /// <summary> Gets or sets a value that determines the cost of traversing the link.</summary>
        /// <remarks> A negative value implies that the traversal cost is obtained based on the area type.
        /// A positive or zero value applies immediately, overriding the cost associated with the area type.</remarks>
        public int costModifier { get { return m_CostModifier; } set { m_CostModifier = value; UpdateLink(); } }

        /// <summary> Gets or sets whether the link can be traversed in both directions. </summary>
        /// <remarks> A link that connects to NavMeshes at both ends can always be traversed from the start position to the end position. When this property is set to `true` it allows the agents to traverse the link also in the direction from end to start. When the value is `false` the agents will never move over the link from the end position to the start position.</remarks>
        public bool bidirectional { get { return m_Bidirectional; } set { m_Bidirectional = value; UpdateLink(); } }

        /// <summary> Gets or sets whether the world positions of the link's edges update whenever
        /// the GameObject transform changes at runtime. </summary>
        public bool autoUpdate { get { return m_AutoUpdatePosition; } set { SetAutoUpdate(value); } }

        /// <summary> The area type of the link. </summary>
        public int area { get { return m_Area; } set { m_Area = value; UpdateLink(); } }

        NavMeshLinkInstance m_LinkInstance = new NavMeshLinkInstance();

        Vector3 m_LastPosition = Vector3.zero;
        Quaternion m_LastRotation = Quaternion.identity;

        static readonly List<NavMeshLink> s_Tracked = new List<NavMeshLink>();

        void OnEnable()
        {
            AddLink();
            if (m_AutoUpdatePosition && m_LinkInstance.valid)
                AddTracking(this);
        }

        void OnDisable()
        {
            RemoveTracking(this);
            m_LinkInstance.Remove();
        }

        /// <summary> Replaces the link with a new one using the current settings. </summary>
        public void UpdateLink()
        {
            m_LinkInstance.Remove();
            AddLink();
        }

        static void AddTracking(NavMeshLink link)
        {
#if UNITY_EDITOR
            if (s_Tracked.Contains(link))
            {
                Debug.LogError("Link is already tracked: " + link);
                return;
            }
#endif
            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate += UpdateTrackedInstances;

            s_Tracked.Add(link);
        }

        static void RemoveTracking(NavMeshLink link)
        {
            s_Tracked.Remove(link);

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate -= UpdateTrackedInstances;
        }

        void SetAutoUpdate(bool value)
        {
            if (m_AutoUpdatePosition == value)
                return;
            m_AutoUpdatePosition = value;
            if (value)
                AddTracking(this);
            else
                RemoveTracking(this);
        }

        void AddLink()
        {
#if UNITY_EDITOR
            if (m_LinkInstance.valid)
            {
                Debug.LogError("Link is already added: " + this);
                return;
            }
#endif

            var link = new NavMeshLinkData();
            link.startPosition = m_StartPoint;
            link.endPosition = m_EndPoint;
            link.width = m_Width;
            link.costModifier = m_CostModifier;
            link.bidirectional = m_Bidirectional;
            link.area = m_Area;
            link.agentTypeID = m_AgentTypeID;
            m_LinkInstance = NavMesh.AddLink(link, transform.position, transform.rotation);
            if (m_LinkInstance.valid)
                m_LinkInstance.owner = this;

            m_LastPosition = transform.position;
            m_LastRotation = transform.rotation;
        }

        bool HasTransformChanged()
        {
            if (m_LastPosition != transform.position)
                return true;
            if (m_LastRotation != transform.rotation)
                return true;
            return false;
        }

        void OnDidApplyAnimationProperties()
        {
            UpdateLink();
        }

        static void UpdateTrackedInstances()
        {
            foreach (var instance in s_Tracked)
            {
                if (instance.HasTransformChanged())
                    instance.UpdateLink();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            m_Width = Mathf.Max(0.0f, m_Width);

            if (!m_LinkInstance.valid)
                return;

            UpdateLink();

            if (!m_AutoUpdatePosition)
            {
                RemoveTracking(this);
            }
            else if (!s_Tracked.Contains(this))
            {
                AddTracking(this);
            }
        }
#endif
    }
}
