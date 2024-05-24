using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering;

#if PACKAGE_TILEMAP
using UnityEngine.Tilemaps;
#endif
using UnityEngine.U2D;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Simple event system using physics raycasts.
    /// </summary>
    [AddComponentMenu("Event/Physics 2D Raycaster")]
    [RequireComponent(typeof(Camera))]
    /// <summary>
    /// Raycaster for casting against 2D Physics components.
    /// </summary>
    public class Physics2DRaycaster : PhysicsRaycaster
    {
#if PACKAGE_PHYSICS2D
        RaycastHit2D[] m_Hits;
#endif

        protected Physics2DRaycaster()
        {}

        /// <summary>
        /// Raycast against 2D elements in the scene.
        /// </summary>
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
#if PACKAGE_PHYSICS2D
            Ray ray = new Ray();
            float distanceToClipPlane = 0;
            int displayIndex = 0;
            if (!ComputeRayAndDistance(eventData, ref ray, ref displayIndex, ref distanceToClipPlane))
                return;

            int hitCount = 0;

            if (maxRayIntersections == 0)
            {
                if (ReflectionMethodsCache.Singleton.getRayIntersectionAll == null)
                    return;
                m_Hits = ReflectionMethodsCache.Singleton.getRayIntersectionAll(ray, distanceToClipPlane, finalEventMask);
                hitCount = m_Hits.Length;
            }
            else
            {
                if (ReflectionMethodsCache.Singleton.getRayIntersectionAllNonAlloc == null)
                    return;

                if (m_LastMaxRayIntersections != m_MaxRayIntersections)
                {
                    m_Hits = new RaycastHit2D[maxRayIntersections];
                    m_LastMaxRayIntersections = m_MaxRayIntersections;
                }

                hitCount = ReflectionMethodsCache.Singleton.getRayIntersectionAllNonAlloc(ray, m_Hits, distanceToClipPlane, finalEventMask);
            }

            if (hitCount != 0)
            {
                for (int b = 0, bmax = hitCount; b < bmax; ++b)
                {
                    Renderer r2d = null;
                    // Case 1198442: Check for 2D renderers when filling in RaycastResults
                    var rendererResult = m_Hits[b].collider.gameObject.GetComponent<Renderer>();
                    if (rendererResult != null)
                    {
                        if (rendererResult is SpriteRenderer)
                        {
                            r2d = rendererResult;
                        }
#if PACKAGE_TILEMAP
                        if (rendererResult is TilemapRenderer)
                        {
                            r2d = rendererResult;
                        }
#endif
                        if (rendererResult is SpriteShapeRenderer)
                        {
                            r2d = rendererResult;
                        }
                    }

                    var result = new RaycastResult
                    {
                        gameObject = m_Hits[b].collider.gameObject,
                        module = this,
                        distance = m_Hits[b].distance,
                        worldPosition = m_Hits[b].point,
                        worldNormal = m_Hits[b].normal,
                        screenPosition = eventData.position,
                        displayIndex = displayIndex,
                        index = resultAppendList.Count,
                        sortingGroupID = r2d != null ? r2d.sortingGroupID : SortingGroup.invalidSortingGroupID,
                        sortingGroupOrder = r2d != null ? r2d.sortingGroupOrder : 0,
                        sortingLayer = r2d != null ? r2d.sortingLayerID : 0,
                        sortingOrder = r2d != null ? r2d.sortingOrder : 0
                    };

                    if (result.sortingGroupID != SortingGroup.invalidSortingGroupID &&
                        SortingGroup.GetSortingGroupByIndex(r2d.sortingGroupID) is SortingGroup sortingGroup)
                    {
                        // Calculate how far along the ray the sorting group is.
                        result.distance = Vector3.Dot(ray.direction, sortingGroup.transform.position - ray.origin);
                        result.sortingLayer = sortingGroup.sortingLayerID;
                        result.sortingOrder = sortingGroup.sortingOrder;
                    }

                    resultAppendList.Add(result);
                }
            }
#endif
        }
    }
}
