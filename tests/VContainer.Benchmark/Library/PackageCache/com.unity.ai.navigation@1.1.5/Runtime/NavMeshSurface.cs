using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

#if UNITY_EDITOR && UNITY_2022_2_OR_NEWER
[assembly: InternalsVisibleTo("Unity.AI.Navigation.Editor")]
#endif

namespace Unity.AI.Navigation
{
    /// <summary> Sets the method for filtering the objects retrieved when baking the NavMesh. </summary>
    public enum CollectObjects
    {
        /// <summary> Use all the active objects. </summary>
        [InspectorName("All Game Objects")] 
        All = 0,
        /// <summary> Use all the active objects that overlap the bounding volume. </summary>
        [InspectorName("Volume")] 
        Volume = 1,
        /// <summary> Use all the active objects that are children of this GameObject. </summary>
        /// <remarks> This includes the current GameObject and all the children of the children that are active.</remarks>
        [InspectorName("Current Object Hierarchy")]
        Children = 2,
#if UNITY_2022_2_OR_NEWER
        /// <summary> Use all the active objects that are marked with a NavMeshModifier. </summary>
        [InspectorName("NavMeshModifier Component Only")]
        MarkedWithModifier = 3,
#endif
    }

    /// <summary> Component used for building and enabling a NavMesh surface for one agent type. </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-102)]
    [AddComponentMenu("Navigation/NavMeshSurface", 30)]
    [HelpURL(HelpUrls.Manual + "NavMeshSurface.html")]
    public class NavMeshSurface : MonoBehaviour
    {
        [SerializeField]
        int m_AgentTypeID;

        [SerializeField]
        CollectObjects m_CollectObjects = CollectObjects.All;

        [SerializeField]
        Vector3 m_Size = new Vector3(10.0f, 10.0f, 10.0f);

        [SerializeField]
        Vector3 m_Center = new Vector3(0, 2.0f, 0);

        [SerializeField]
        LayerMask m_LayerMask = ~0;

        [SerializeField]
        NavMeshCollectGeometry m_UseGeometry = NavMeshCollectGeometry.RenderMeshes;

        [SerializeField]
        int m_DefaultArea;

#if UNITY_2022_2_OR_NEWER
        [SerializeField]
        bool m_GenerateLinks;
#endif

        [SerializeField]
        bool m_IgnoreNavMeshAgent = true;

        [SerializeField]
        bool m_IgnoreNavMeshObstacle = true;

        [SerializeField]
        bool m_OverrideTileSize;

        [SerializeField]
        int m_TileSize = 256;

        [SerializeField]
        bool m_OverrideVoxelSize;

        [SerializeField]
        float m_VoxelSize;
        
        [SerializeField]
        float m_MinRegionArea = 2;        

        [FormerlySerializedAs("m_BakedNavMeshData")]
        [SerializeField]
        NavMeshData m_NavMeshData;

        [SerializeField]
        bool m_BuildHeightMesh;

        /// <summary> Gets or sets the identifier of the agent type that will use this NavMesh Surface. </summary>
        public int agentTypeID { get { return m_AgentTypeID; } set { m_AgentTypeID = value; } }

        /// <summary> Gets or sets the method for retrieving the objects that will be used for baking. </summary>
        public CollectObjects collectObjects { get { return m_CollectObjects; } set { m_CollectObjects = value; } }

        /// <summary> Gets or sets the size of the volume that delimits the NavMesh created by this component. </summary>
        /// <remarks> It is used only when <c>collectObjects</c> is set to <c>Volume</c>. The size applies in the local space of the GameObject. </remarks>
        public Vector3 size { get { return m_Size; } set { m_Size = value; } }

        /// <summary> Gets or sets the center position of the volume that delimits the NavMesh created by this component. </summary>
        /// <remarks> It is used only when <c>collectObjects</c> is set to <c>Volume</c>. The position applies in the local space of the GameObject. </remarks>
        public Vector3 center { get { return m_Center; } set { m_Center = value; } }

        /// <summary> Gets or sets a bitmask representing which layers to consider when selecting the objects that will be used for baking the NavMesh. </summary>
        public LayerMask layerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }

        /// <summary> Gets or sets which type of component in the GameObjects provides the geometry used for baking the NavMesh. </summary>
        public NavMeshCollectGeometry useGeometry { get { return m_UseGeometry; } set { m_UseGeometry = value; } }

        /// <summary> Gets or sets the area type assigned to any object that does not have one specified. </summary>
        /// <remarks> To customize the area type of an object add a <see cref="NavMeshModifier"/> component and set <see cref="NavMeshModifier.overrideArea"/> to <c>true</c>. The area type information is used when baking the NavMesh. </remarks>
        /// <seealso href="https://docs.unity3d.com/Manual/nav-AreasAndCosts.html"/>
        public int defaultArea { get { return m_DefaultArea; } set { m_DefaultArea = value; } }

        /// <summary> Gets or sets whether the process of building the NavMesh ignores the GameObjects containing a <see cref="NavMeshAgent"/> component. </summary>
        /// <remarks> There is generally no need for the NavMesh to take into consideration the objects that can move.</remarks>
        public bool ignoreNavMeshAgent { get { return m_IgnoreNavMeshAgent; } set { m_IgnoreNavMeshAgent = value; } }

        /// <summary> Gets or sets whether the process of building the NavMesh ignores the GameObjects containing a <see cref="NavMeshObstacle"/> component. </summary>
        /// <remarks> There is generally no need for the NavMesh to take into consideration the objects that can move.</remarks>
        public bool ignoreNavMeshObstacle { get { return m_IgnoreNavMeshObstacle; } set { m_IgnoreNavMeshObstacle = value; } }

        /// <summary> Gets or sets whether the NavMesh building process uses the <see cref="tileSize"/> value. </summary>
        public bool overrideTileSize { get { return m_OverrideTileSize; } set { m_OverrideTileSize = value; } }

        /// <summary> Gets or sets the width of the square grid of voxels that the NavMesh building process uses for sampling the scene geometry. </summary>
        /// <remarks> This value represents a number of voxels. Together with <see cref="voxelSize"/> it determines the real size of the individual sections that comprise the NavMesh. </remarks>
        public int tileSize { get { return m_TileSize; } set { m_TileSize = value; } }

        /// <summary> Gets or sets whether the NavMesh building process uses the <see cref="voxelSize"/> value. </summary>
        public bool overrideVoxelSize { get { return m_OverrideVoxelSize; } set { m_OverrideVoxelSize = value; } }

        /// <summary> Gets or sets the width of the square voxels that the NavMesh building process uses for sampling the scene geometry. </summary>
        /// <remarks> This value is in world units. Together with <see cref="tileSize"/> it determines the real size of the individual sections that comprise the NavMesh. </remarks>
        public float voxelSize { get { return m_VoxelSize; } set { m_VoxelSize = value; } }
        
        /// <summary> Gets or sets the minimum acceptable surface area of any continuous portion of the NavMesh. </summary>
        /// <remarks> This parameter is used only at the time when the NavMesh is getting built. It allows you to cull away any isolated NavMesh regions that are smaller than this value and that do not straddle or touch a tile boundary. </remarks>
        public float minRegionArea { get { return m_MinRegionArea; } set { m_MinRegionArea = value; } }

        /// <summary> Gets or sets whether the NavMesh building process produces more detailed elevation information. </summary>
        /// <seealso href="https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.0/manual/NavMeshSurface.html#advanced-settings"/>
        public bool buildHeightMesh { get { return m_BuildHeightMesh; } set { m_BuildHeightMesh = value; } }

        /// <summary> Gets or sets the reference to the NavMesh data instantiated by this surface. </summary>
        public NavMeshData navMeshData { get { return m_NavMeshData; } set { m_NavMeshData = value; } }

        // Do not serialize - runtime only state.
        NavMeshDataInstance m_NavMeshDataInstance;
        Vector3 m_LastPosition = Vector3.zero;
        Quaternion m_LastRotation = Quaternion.identity;

        internal NavMeshDataInstance navMeshDataInstance => m_NavMeshDataInstance;

        static readonly List<NavMeshSurface> s_NavMeshSurfaces = new List<NavMeshSurface>();

        /// <summary> Gets the list of all the <see cref="NavMeshSurface"/> components that are currently active in the scene. </summary>
        public static List<NavMeshSurface> activeSurfaces
        {
            get { return s_NavMeshSurfaces; }
        }

        Bounds GetInflatedBounds()
        {
            var settings = NavMesh.GetSettingsByID(m_AgentTypeID);
            var agentRadius = settings.agentTypeID != -1 ? settings.agentRadius : 0f;
            
            var bounds = new Bounds(center, size);
            bounds.Expand(new Vector3(agentRadius, 0, agentRadius));
            return bounds;
        }
        
        void OnEnable()
        {
            Register(this);
            AddData();
        }

        void OnDisable()
        {
            RemoveData();
            Unregister(this);
        }

        /// <summary> Creates an instance of the NavMesh data and activates it in the navigation system. </summary>
        /// <remarks> The instance is created at the position and with the orientation of the GameObject. </remarks>
        public void AddData()
        {
#if UNITY_EDITOR
            if (IsEditedInPrefab(this))
                return;
#endif
            if (m_NavMeshDataInstance.valid)
                return;

            if (m_NavMeshData != null)
            {
                m_NavMeshDataInstance = NavMesh.AddNavMeshData(m_NavMeshData, transform.position, transform.rotation);
                m_NavMeshDataInstance.owner = this;
            }

            m_LastPosition = transform.position;
            m_LastRotation = transform.rotation;
        }

        /// <summary> Removes the instance of this NavMesh data from the navigation system. </summary>
        /// <remarks> This operation does not destroy the <see cref="navMeshData"/>. </remarks>
        public void RemoveData()
        {
            m_NavMeshDataInstance.Remove();
            m_NavMeshDataInstance = new NavMeshDataInstance();
        }

        /// <summary> Retrieves a copy of the current settings chosen for building this NavMesh surface. </summary>
        /// <returns> The settings configured in this NavMeshSurface. </returns>
        public NavMeshBuildSettings GetBuildSettings()
        {
            var buildSettings = NavMesh.GetSettingsByID(m_AgentTypeID);
            if (buildSettings.agentTypeID == -1)
            {
                Debug.LogWarning("No build settings for agent type ID " + agentTypeID, this);
                buildSettings.agentTypeID = m_AgentTypeID;
            }

            if (overrideTileSize)
            {
                buildSettings.overrideTileSize = true;
                buildSettings.tileSize = tileSize;
            }
            if (overrideVoxelSize)
            {
                buildSettings.overrideVoxelSize = true;
                buildSettings.voxelSize = voxelSize;
            }

            buildSettings.minRegionArea = minRegionArea;

#if UNITY_2022_2_OR_NEWER
            buildSettings.buildHeightMesh = buildHeightMesh;
#endif

            return buildSettings;
        }

        /// <summary> Builds and instantiates this NavMesh surface. </summary>
        public void BuildNavMesh()
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var surfaceBounds = new Bounds(m_Center, Abs(m_Size));
            if (m_CollectObjects != CollectObjects.Volume)
            {
                surfaceBounds = CalculateWorldBounds(sources);
            }

            var data = NavMeshBuilder.BuildNavMeshData(GetBuildSettings(),
                    sources, surfaceBounds, transform.position, transform.rotation);

            if (data != null)
            {
                data.name = gameObject.name;
                RemoveData();
                m_NavMeshData = data;
                if (isActiveAndEnabled)
                    AddData();
            }
        }

        /// <summary> Rebuilds parts of an existing NavMesh in the regions of the scene where the objects have changed. </summary>
        /// <remarks> This operation is executed asynchronously. </remarks>
        /// <param name="data"> The NavMesh to update according to the changes in the scene. </param>
        /// <returns> A reference to the asynchronous coroutine that builds the NavMesh. </returns>
        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var surfaceBounds = new Bounds(m_Center, Abs(m_Size));
            if (m_CollectObjects != CollectObjects.Volume)
                surfaceBounds = CalculateWorldBounds(sources);

            return NavMeshBuilder.UpdateNavMeshDataAsync(data, GetBuildSettings(), sources, surfaceBounds);
        }

        static void Register(NavMeshSurface surface)
        {
#if UNITY_EDITOR
            if (IsEditedInPrefab(surface))
                return;
#endif
            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate += UpdateActive;

            if (!s_NavMeshSurfaces.Contains(surface))
                s_NavMeshSurfaces.Add(surface);
        }

        static void Unregister(NavMeshSurface surface)
        {
            s_NavMeshSurfaces.Remove(surface);

            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate -= UpdateActive;
        }

        static void UpdateActive()
        {
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
                s_NavMeshSurfaces[i].UpdateDataIfTransformChanged();
        }

        void AppendModifierVolumes(ref List<NavMeshBuildSource> sources)
        {
#if UNITY_EDITOR
            var myStage = StageUtility.GetStageHandle(gameObject);
            if (!myStage.IsValid())
                return;
#endif
            // Modifiers
            List<NavMeshModifierVolume> modifiers;
            if (m_CollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifierVolume>(GetComponentsInChildren<NavMeshModifierVolume>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifierVolume.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((m_LayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(m_AgentTypeID))
                    continue;
#if UNITY_EDITOR
                if (!myStage.Contains(m.gameObject))
                    continue;
#endif
                var mcenter = m.transform.TransformPoint(m.center);
                var scale = m.transform.lossyScale;
                var msize = new Vector3(m.size.x * Mathf.Abs(scale.x), m.size.y * Mathf.Abs(scale.y), m.size.z * Mathf.Abs(scale.z));

                var src = new NavMeshBuildSource();
                src.shape = NavMeshBuildSourceShape.ModifierBox;
                src.transform = Matrix4x4.TRS(mcenter, m.transform.rotation, Vector3.one);
                src.size = msize;
                src.area = m.area;
                sources.Add(src);
            }
        }

        List<NavMeshBuildSource> CollectSources()
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            List<NavMeshModifier> modifiers;
            if (m_CollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifier>(GetComponentsInChildren<NavMeshModifier>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifier.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((m_LayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(m_AgentTypeID))
                    continue;
                var markup = new NavMeshBuildMarkup();
                markup.root = m.transform;
                markup.overrideArea = m.overrideArea;
                markup.area = m.area;
                markup.ignoreFromBuild = m.ignoreFromBuild;
#if UNITY_2022_2_OR_NEWER
                markup.applyToChildren = m.applyToChildren;
                markup.overrideGenerateLinks = m.overrideGenerateLinks;
                markup.generateLinks = m.generateLinks;
#endif
                markups.Add(markup);
            }

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying || IsPartOfPrefab())
            {
#if UNITY_2022_2_OR_NEWER
                if (m_CollectObjects == CollectObjects.All)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        null, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, markups, false, gameObject.scene, sources);
                }
                else if (m_CollectObjects == CollectObjects.Children)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        transform, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, markups, false, gameObject.scene, sources);
                }
                else if (m_CollectObjects == CollectObjects.Volume)
                {
                    Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, GetInflatedBounds());

                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        worldBounds, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, markups, false, gameObject.scene, sources);
                }
                else if (m_CollectObjects == CollectObjects.MarkedWithModifier)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        null, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, markups, true, gameObject.scene, sources);                    
                }
#else
                if (m_CollectObjects == CollectObjects.All)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        null, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, gameObject.scene, sources);
                }
                else if (m_CollectObjects == CollectObjects.Children)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        transform, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, gameObject.scene, sources);
                }
                else if (m_CollectObjects == CollectObjects.Volume)
                {
                    Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, GetInflatedBounds());

                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        worldBounds, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, gameObject.scene, sources);
                }
#endif //UNITY_2022_2_OR_NEWER
            }
            else
#endif
            {
#if UNITY_2022_2_OR_NEWER
                if (m_CollectObjects == CollectObjects.All)
                {
                    NavMeshBuilder.CollectSources(null, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, markups, false, sources);
                }
                else if (m_CollectObjects == CollectObjects.Children)
                {
                    NavMeshBuilder.CollectSources(transform, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, markups, false, sources);
                }
                else if (m_CollectObjects == CollectObjects.Volume)
                {
                    Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, GetInflatedBounds());
                    NavMeshBuilder.CollectSources(worldBounds, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, markups, false, sources);
                }
                else  if (m_CollectObjects == CollectObjects.MarkedWithModifier)
                {
                    NavMeshBuilder.CollectSources(null, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, markups, true, sources);
                }
#else
                if (m_CollectObjects == CollectObjects.All)
                {
                    NavMeshBuilder.CollectSources(null, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
                }
                else if (m_CollectObjects == CollectObjects.Children)
                {
                    NavMeshBuilder.CollectSources(transform, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
                }
                else if (m_CollectObjects == CollectObjects.Volume)
                {
                    Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, GetInflatedBounds());
                    NavMeshBuilder.CollectSources(worldBounds, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
                }
#endif //UNITY_2022_2_OR_NEWER
            }

            if (m_IgnoreNavMeshAgent)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshAgent>() != null));

            if (m_IgnoreNavMeshObstacle)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshObstacle>() != null));

            AppendModifierVolumes(ref sources);

            return sources;
        }

        static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
        {
            var absAxisX = Abs(mat.MultiplyVector(Vector3.right));
            var absAxisY = Abs(mat.MultiplyVector(Vector3.up));
            var absAxisZ = Abs(mat.MultiplyVector(Vector3.forward));
            var worldPosition = mat.MultiplyPoint(bounds.center);
            var worldSize = absAxisX * bounds.size.x + absAxisY * bounds.size.y + absAxisZ * bounds.size.z;
            return new Bounds(worldPosition, worldSize);
        }

        Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
        {
            // Use the unscaled matrix for the NavMeshSurface
            Matrix4x4 worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            worldToLocal = worldToLocal.inverse;

            var result = new Bounds();
            foreach (var src in sources)
            {
                switch (src.shape)
                {
                    case NavMeshBuildSourceShape.Mesh:
                        {
                            var m = src.sourceObject as Mesh;
                            result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, m.bounds));
                            break;
                        }
                    case NavMeshBuildSourceShape.Terrain:
                        {
#if NMC_CAN_ACCESS_TERRAIN
                            // Terrain pivot is lower/left corner - shift bounds accordingly
                            var t = src.sourceObject as TerrainData;
                            result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(0.5f * t.size, t.size)));
#else
                            Debug.LogWarning("The NavMesh cannot be properly baked for the terrain because the necessary functionality is missing. Add the com.unity.modules.terrain package through the Package Manager.");
#endif
                            break;
                        }
                    case NavMeshBuildSourceShape.Box:
                    case NavMeshBuildSourceShape.Sphere:
                    case NavMeshBuildSourceShape.Capsule:
                    case NavMeshBuildSourceShape.ModifierBox:
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(Vector3.zero, src.size)));
                        break;
                }
            }
            // Inflate the bounds a bit to avoid clipping co-planar sources
            result.Expand(0.1f);
            return result;
        }

        bool HasTransformChanged()
        {
            if (m_LastPosition != transform.position)
                return true;
            if (m_LastRotation != transform.rotation)
                return true;
            return false;
        }

        void UpdateDataIfTransformChanged()
        {
            if (HasTransformChanged())
            {
                RemoveData();
                AddData();
            }
        }

#if UNITY_EDITOR
        bool UnshareNavMeshAsset()
        {
            // Nothing to unshare
            if (m_NavMeshData == null)
                return false;

            // Prefab parent owns the asset reference
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(this);
            var isPersistentObject = EditorUtility.IsPersistent(this);
            if (isInPreviewScene || isPersistentObject)
                return false;

            // An instance can share asset reference only with its prefab parent
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(this) as NavMeshSurface;
            if (prefab != null && prefab.navMeshData == navMeshData)
                return false;

            // Don't allow referencing an asset that's assigned to another surface
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
            {
                var surface = s_NavMeshSurfaces[i];
                if (surface != this && surface.m_NavMeshData == m_NavMeshData)
                    return true;
            }

            // Asset is not referenced by known surfaces
            return false;
        }

        void OnValidate()
        {
            if (UnshareNavMeshAsset())
            {
                Debug.LogWarning("Duplicating NavMeshSurface does not duplicate the referenced navmesh data", this);
                m_NavMeshData = null;
            }

            var settings = NavMesh.GetSettingsByID(m_AgentTypeID);
            if (settings.agentTypeID != -1)
            {
                // When unchecking the override control, revert to automatic value.
                const float kMinVoxelSize = 0.01f;
                if (!m_OverrideVoxelSize)
                    m_VoxelSize = settings.agentRadius / 3.0f;
                if (m_VoxelSize < kMinVoxelSize)
                    m_VoxelSize = kMinVoxelSize;

                // When unchecking the override control, revert to default value.
                const int kMinTileSize = 16;
                const int kMaxTileSize = 1024;
                const int kDefaultTileSize = 256;

                if (!m_OverrideTileSize)
                    m_TileSize = kDefaultTileSize;
                // Make sure tilesize is in sane range.
                if (m_TileSize < kMinTileSize)
                    m_TileSize = kMinTileSize;
                if (m_TileSize > kMaxTileSize)
                    m_TileSize = kMaxTileSize;
                
                if (m_MinRegionArea < 0)
                    m_MinRegionArea = 0;
            }
        }

        static bool IsEditedInPrefab(NavMeshSurface navMeshSurface)
        {
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(navMeshSurface);
            var isPrefab = isInPreviewScene || EditorUtility.IsPersistent(navMeshSurface);
            // if (isPrefab)
            //     Debug.Log($"NavMeshData from {navMeshSurface.gameObject.name}.{navMeshSurface.name} will not be added to the NavMesh world because the gameObject is a prefab.");
            return isPrefab;
        }

        internal bool IsPartOfPrefab()
        {
            var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
            var isPartOfPrefab = prefabStage != null && prefabStage.IsPartOfPrefabContents(gameObject);
            return isPartOfPrefab;
        }
#endif
    }
}
