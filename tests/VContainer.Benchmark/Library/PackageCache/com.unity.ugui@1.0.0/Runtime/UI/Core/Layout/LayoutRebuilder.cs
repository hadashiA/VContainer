using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace UnityEngine.UI
{
    /// <summary>
    /// Wrapper class for managing layout rebuilding of CanvasElement.
    /// </summary>
    public class LayoutRebuilder : ICanvasElement
    {
        private RectTransform m_ToRebuild;
        //There are a few of reasons we need to cache the Hash fromt he transform:
        //  - This is a ValueType (struct) and .Net calculates Hash from the Value Type fields.
        //  - The key of a Dictionary should have a constant Hash value.
        //  - It's possible for the Transform to get nulled from the Native side.
        // We use this struct with the IndexedSet container, which uses a dictionary as part of it's implementation
        // So this struct gets used as a key to a dictionary, so we need to guarantee a constant Hash value.
        private int m_CachedHashFromTransform;

        static ObjectPool<LayoutRebuilder> s_Rebuilders = new ObjectPool<LayoutRebuilder>(() => new LayoutRebuilder(), null, x => x.Clear());

        private void Initialize(RectTransform controller)
        {
            m_ToRebuild = controller;
            m_CachedHashFromTransform = controller.GetHashCode();
        }

        private void Clear()
        {
            m_ToRebuild = null;
            m_CachedHashFromTransform = 0;
        }

        static LayoutRebuilder()
        {
            RectTransform.reapplyDrivenProperties += ReapplyDrivenProperties;
        }

        static void ReapplyDrivenProperties(RectTransform driven)
        {
            MarkLayoutForRebuild(driven);
        }

        public Transform transform { get { return m_ToRebuild; }}

        /// <summary>
        /// Has the native representation of this LayoutRebuilder been destroyed?
        /// </summary>
        public bool IsDestroyed()
        {
            return m_ToRebuild == null;
        }

        static void StripDisabledBehavioursFromList(List<Component> components)
        {
            components.RemoveAll(e => e is Behaviour && !((Behaviour)e).isActiveAndEnabled);
        }

        /// <summary>
        /// Forces an immediate rebuild of the layout element and child layout elements affected by the calculations.
        /// </summary>
        /// <param name="layoutRoot">The layout element to perform the layout rebuild on.</param>
        /// <remarks>
        /// Normal use of the layout system should not use this method. Instead MarkLayoutForRebuild should be used instead, which triggers a delayed layout rebuild during the next layout pass. The delayed rebuild automatically handles objects in the entire layout hierarchy in the correct order, and prevents multiple recalculations for the same layout elements.
        /// However, for special layout calculation needs, ::ref::ForceRebuildLayoutImmediate can be used to get the layout of a sub-tree resolved immediately. This can even be done from inside layout calculation methods such as ILayoutController.SetLayoutHorizontal orILayoutController.SetLayoutVertical. Usage should be restricted to cases where multiple layout passes are unavaoidable despite the extra cost in performance.
        /// </remarks>
        public static void ForceRebuildLayoutImmediate(RectTransform layoutRoot)
        {
            var rebuilder = s_Rebuilders.Get();
            rebuilder.Initialize(layoutRoot);
            rebuilder.Rebuild(CanvasUpdate.Layout);
            s_Rebuilders.Release(rebuilder);
        }

        public void Rebuild(CanvasUpdate executing)
        {
            switch (executing)
            {
                case CanvasUpdate.Layout:
                    // It's unfortunate that we'll perform the same GetComponents querys for the tree 2 times,
                    // but each tree have to be fully iterated before going to the next action,
                    // so reusing the results would entail storing results in a Dictionary or similar,
                    // which is probably a bigger overhead than performing GetComponents multiple times.
                    PerformLayoutCalculation(m_ToRebuild, e => (e as ILayoutElement).CalculateLayoutInputHorizontal());
                    PerformLayoutControl(m_ToRebuild, e => (e as ILayoutController).SetLayoutHorizontal());
                    PerformLayoutCalculation(m_ToRebuild, e => (e as ILayoutElement).CalculateLayoutInputVertical());
                    PerformLayoutControl(m_ToRebuild, e => (e as ILayoutController).SetLayoutVertical());
                    break;
            }
        }

        private void PerformLayoutControl(RectTransform rect, UnityAction<Component> action)
        {
            if (rect == null)
                return;

            var components = ListPool<Component>.Get();
            rect.GetComponents(typeof(ILayoutController), components);
            StripDisabledBehavioursFromList(components);

            // If there are no controllers on this rect we can skip this entire sub-tree
            // We don't need to consider controllers on children deeper in the sub-tree either,
            // since they will be their own roots.
            if (components.Count > 0)
            {
                // Layout control needs to executed top down with parents being done before their children,
                // because the children rely on the sizes of the parents.

                // First call layout controllers that may change their own RectTransform
                for (int i = 0; i < components.Count; i++)
                    if (components[i] is ILayoutSelfController)
                        action(components[i]);

                // Then call the remaining, such as layout groups that change their children, taking their own RectTransform size into account.
                for (int i = 0; i < components.Count; i++)
                    if (!(components[i] is ILayoutSelfController))
                    {
                        var scrollRect = components[i];

                        if (scrollRect && scrollRect is UnityEngine.UI.ScrollRect)
                        {
                            if (((UnityEngine.UI.ScrollRect)scrollRect).content != rect)
                                action(components[i]);
                        }
                        else
                        {
                            action(components[i]);
                        }
                    }

                for (int i = 0; i < rect.childCount; i++)
                    PerformLayoutControl(rect.GetChild(i) as RectTransform, action);
            }

            ListPool<Component>.Release(components);
        }

        private void PerformLayoutCalculation(RectTransform rect, UnityAction<Component> action)
        {
            if (rect == null)
                return;

            var components = ListPool<Component>.Get();
            rect.GetComponents(typeof(ILayoutElement), components);
            StripDisabledBehavioursFromList(components);

            // If there are no controllers on this rect we can skip this entire sub-tree
            // We don't need to consider controllers on children deeper in the sub-tree either,
            // since they will be their own roots.
            if (components.Count > 0  || rect.TryGetComponent(typeof(ILayoutGroup), out _))
            {
                // Layout calculations needs to executed bottom up with children being done before their parents,
                // because the parent calculated sizes rely on the sizes of the children.

                for (int i = 0; i < rect.childCount; i++)
                    PerformLayoutCalculation(rect.GetChild(i) as RectTransform, action);

                for (int i = 0; i < components.Count; i++)
                    action(components[i]);
            }

            ListPool<Component>.Release(components);
        }

        /// <summary>
        /// Mark the given RectTransform as needing it's layout to be recalculated during the next layout pass.
        /// </summary>
        /// <param name="rect">Rect to rebuild.</param>
        public static void MarkLayoutForRebuild(RectTransform rect)
        {
            if (rect == null || rect.gameObject == null)
                return;

            var comps = ListPool<Component>.Get();
            bool validLayoutGroup = true;
            RectTransform layoutRoot = rect;
            var parent = layoutRoot.parent as RectTransform;
            while (validLayoutGroup && !(parent == null || parent.gameObject == null))
            {
                validLayoutGroup = false;
                parent.GetComponents(typeof(ILayoutGroup), comps);

                for (int i = 0; i < comps.Count; ++i)
                {
                    var cur = comps[i];
                    if (cur != null && cur is Behaviour && ((Behaviour)cur).isActiveAndEnabled)
                    {
                        validLayoutGroup = true;
                        layoutRoot = parent;
                        break;
                    }
                }

                parent = parent.parent as RectTransform;
            }

            // We know the layout root is valid if it's not the same as the rect,
            // since we checked that above. But if they're the same we still need to check.
            if (layoutRoot == rect && !ValidController(layoutRoot, comps))
            {
                ListPool<Component>.Release(comps);
                return;
            }

            MarkLayoutRootForRebuild(layoutRoot);
            ListPool<Component>.Release(comps);
        }

        private static bool ValidController(RectTransform layoutRoot, List<Component> comps)
        {
            if (layoutRoot == null || layoutRoot.gameObject == null)
                return false;

            layoutRoot.GetComponents(typeof(ILayoutController), comps);
            for (int i = 0; i < comps.Count; ++i)
            {
                var cur = comps[i];
                if (cur != null && cur is Behaviour && ((Behaviour)cur).isActiveAndEnabled)
                {
                    return true;
                }
            }

            return false;
        }

        private static void MarkLayoutRootForRebuild(RectTransform controller)
        {
            if (controller == null)
                return;

            var rebuilder = s_Rebuilders.Get();
            rebuilder.Initialize(controller);
            if (!CanvasUpdateRegistry.TryRegisterCanvasElementForLayoutRebuild(rebuilder))
                s_Rebuilders.Release(rebuilder);
        }

        public void LayoutComplete()
        {
            s_Rebuilders.Release(this);
        }

        public void GraphicUpdateComplete()
        {}

        public override int GetHashCode()
        {
            return m_CachedHashFromTransform;
        }

        /// <summary>
        /// Does the passed rebuilder point to the same CanvasElement.
        /// </summary>
        /// <param name="obj">The other object to compare</param>
        /// <returns>Are they equal</returns>
        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == GetHashCode();
        }

        public override string ToString()
        {
            return "(Layout Rebuilder for) " + m_ToRebuild;
        }
    }
}
