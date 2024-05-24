namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Base behaviour that has protected implementations of Unity lifecycle functions.
    /// </summary>
    public abstract class UIBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {}

        protected virtual void OnEnable()
        {}

        protected virtual void Start()
        {}

        protected virtual void OnDisable()
        {}

        protected virtual void OnDestroy()
        {}

        /// <summary>
        /// Returns true if the GameObject and the Component are active.
        /// </summary>
        public virtual bool IsActive()
        {
            return isActiveAndEnabled;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {}

        protected virtual void Reset()
        {}
#endif
        /// <summary>
        /// This callback is called when the dimensions of an associated RectTransform change. It is always called before Awake, OnEnable, or Start. The call is also made to all child RectTransforms, regardless of whether their dimensions change (which depends on how they are anchored).
        /// </summary>
        protected virtual void OnRectTransformDimensionsChange()
        {}

        protected virtual void OnBeforeTransformParentChanged()
        {}

        protected virtual void OnTransformParentChanged()
        {}

        protected virtual void OnDidApplyAnimationProperties()
        {}

        protected virtual void OnCanvasGroupChanged()
        {}

        /// <summary>
        /// Called when the state of the parent Canvas is changed.
        /// </summary>
        protected virtual void OnCanvasHierarchyChanged()
        {}

        /// <summary>
        /// Returns true if the native representation of the behaviour has been destroyed.
        /// </summary>
        /// <remarks>
        /// When a parent canvas is either enabled, disabled or a nested canvas's OverrideSorting is changed this function is called. You can for example use this to modify objects below a canvas that may depend on a parent canvas - for example, if a canvas is disabled you may want to halt some processing of a UI element.
        /// </remarks>
        public bool IsDestroyed()
        {
            // Workaround for Unity native side of the object
            // having been destroyed but accessing via interface
            // won't call the overloaded ==
            return this == null;
        }
    }
}
