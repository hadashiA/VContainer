namespace UnityEngine.EventSystems
{
    /// <summary>
    /// A hit result from a BaseRaycaster.
    /// </summary>
    public struct RaycastResult
    {
        private GameObject m_GameObject; // Game object hit by the raycast

        /// <summary>
        /// The GameObject that was hit by the raycast.
        /// </summary>
        public GameObject gameObject
        {
            get { return m_GameObject; }
            set { m_GameObject = value; }
        }

        /// <summary>
        /// BaseRaycaster that raised the hit.
        /// </summary>
        public BaseRaycaster module;

        /// <summary>
        /// Distance to the hit.
        /// </summary>
        public float distance;

        /// <summary>
        /// Hit index
        /// </summary>
        public float index;

        /// <summary>
        /// Used by raycasters where elements may have the same unit distance, but have specific ordering.
        /// </summary>
        public int depth;

        /// <summary>
        /// The sorting group ID when the hit object is influenced by a SortingGroup.
        /// </summary>
        /// <remarks>
        /// For UI.Graphic elements will always be 0.
        /// For 3D objects this will always be 0.
        /// For 2D objects if a SortingOrder is influencing the same object as the hit collider then the renderers sortingGroupID will be used; otherwise SortingGroup.invalidSortingGroupID.
        /// </remarks>
        public int sortingGroupID;

        /// <summary>
        /// The sorting group order when the hit object is influenced by a SortingGroup.
        /// </summary>
        /// <remarks>
        /// For UI.Graphic elements this will always be 0.
        /// For 3D objects this will always be 0.
        /// For 2D objects if a SortingOrder is influencing the same object as the hit collider then the renderers sortingGroupOrder will be used.
        /// </remarks>
        public int sortingGroupOrder;

        /// <summary>
        /// The SortingLayer of the hit object.
        /// </summary>
        /// <remarks>
        /// For UI.Graphic elements this will be the values from that graphic's Canvas
        /// For 3D objects this will always be 0.
        /// For 2D objects if a 2D Renderer (Sprite, Tilemap, SpriteShape) is attached to the same object as the hit collider that sortingLayerID will be used.
        /// </remarks>
        public int sortingLayer;

        /// <summary>
        /// The SortingOrder for the hit object.
        /// </summary>
        /// <remarks>
        /// For Graphic elements this will be the values from that graphics Canvas
        /// For 3D objects this will always be 0.
        /// For 2D objects if a 2D Renderer (Sprite, Tilemap, SpriteShape) is attached to the same object as the hit collider that sortingOrder will be used.
        /// </remarks>
        public int sortingOrder;

        /// <summary>
        /// The world position of the where the raycast has hit.
        /// </summary>
        public Vector3 worldPosition;

        /// <summary>
        /// The normal at the hit location of the raycast.
        /// </summary>
        public Vector3 worldNormal;

        /// <summary>
        /// The screen position from which the raycast was generated.
        /// </summary>
        public Vector2 screenPosition;

        /// <summary>
        /// The display index from which the raycast was generated.
        /// </summary>
        public int displayIndex;

        /// <summary>
        /// Is there an associated module and a hit GameObject.
        /// </summary>
        public bool isValid
        {
            get { return module != null && gameObject != null; }
        }

        /// <summary>
        /// Reset the result.
        /// </summary>
        public void Clear()
        {
            gameObject = null;
            module = null;
            distance = 0;
            index = 0;
            depth = 0;
            sortingLayer = 0;
            sortingOrder = 0;
            worldNormal = Vector3.up;
            worldPosition = Vector3.zero;
            screenPosition = Vector3.zero;
        }

        public override string ToString()
        {
            if (!isValid)
                return "";

            return "Name: " + gameObject + "\n" +
                "module: " + module + "\n" +
                "distance: " + distance + "\n" +
                "index: " + index + "\n" +
                "depth: " + depth + "\n" +
                "worldNormal: " + worldNormal + "\n" +
                "worldPosition: " + worldPosition + "\n" +
                "screenPosition: " + screenPosition + "\n" +
                "module.sortOrderPriority: " + module.sortOrderPriority + "\n" +
                "module.renderOrderPriority: " + module.renderOrderPriority + "\n" +
                "sortingLayer: " + sortingLayer + "\n" +
                "sortingOrder: " + sortingOrder;
        }
    }
}
