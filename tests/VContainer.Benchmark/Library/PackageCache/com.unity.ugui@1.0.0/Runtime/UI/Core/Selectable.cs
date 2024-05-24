using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Selectable", 35)]
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    /// <summary>
    /// Simple selectable object - derived from to create a selectable control.
    /// </summary>
    public class Selectable
        :
        UIBehaviour,
        IMoveHandler,
        IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler
    {
        protected static Selectable[] s_Selectables = new Selectable[10];
        protected static int s_SelectableCount = 0;
        private bool m_EnableCalled = false;

        /// <summary>
        /// Copy of the array of all the selectable objects currently active in the scene.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // required when using UI elements in scripts
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     //Displays the names of all selectable elements in the scene
        ///     public void GetNames()
        ///     {
        ///         foreach (Selectable selectableUI in Selectable.allSelectablesArray)
        ///         {
        ///             Debug.Log(selectableUI.name);
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public static Selectable[] allSelectablesArray
        {
            get
            {
                Selectable[] temp = new Selectable[s_SelectableCount];
                Array.Copy(s_Selectables, temp, s_SelectableCount);
                return temp;
            }
        }

        /// <summary>
        /// How many selectable elements are currently active.
        /// </summary>
        public static int allSelectableCount { get { return s_SelectableCount; } }

        /// <summary>
        /// A List instance of the allSelectablesArray to maintain API compatibility.
        /// </summary>

        [Obsolete("Replaced with allSelectablesArray to have better performance when disabling a element", false)]
        public static List<Selectable> allSelectables
        {
            get
            {
                return new List<Selectable>(allSelectablesArray);
            }
        }


        /// <summary>
        /// Non allocating version for getting the all selectables.
        /// If selectables.Length is less then s_SelectableCount only selectables.Length elments will be copied which
        /// could result in a incomplete list of elements.
        /// </summary>
        /// <param name="selectables">The array to be filled with current selectable objects</param>
        /// <returns>The number of element copied.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // required when using UI elements in scripts
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     Selectable[] m_Selectables = new Selectable[10];
        ///
        ///     //Displays the names of all selectable elements in the scene
        ///     public void GetNames()
        ///     {
        ///         if (m_Selectables.Length < Selectable.allSelectableCount)
        ///             m_Selectables = new Selectable[Selectable.allSelectableCount];
        ///
        ///         int count = Selectable.AllSelectablesNoAlloc(ref m_Selectables);
        ///
        ///         for (int i = 0; i < count; ++i)
        ///         {
        ///             Debug.Log(m_Selectables[i].name);
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public static int AllSelectablesNoAlloc(Selectable[] selectables)
        {
            int copyCount = selectables.Length < s_SelectableCount ? selectables.Length : s_SelectableCount;

            Array.Copy(s_Selectables, selectables, copyCount);

            return copyCount;
        }

        // Navigation information.
        [FormerlySerializedAs("navigation")]
        [SerializeField]
        private Navigation m_Navigation = Navigation.defaultNavigation;

        /// <summary>
        ///Transition mode for a Selectable.
        /// </summary>
        public enum Transition
        {
            /// <summary>
            /// No Transition.
            /// </summary>
            None,

            /// <summary>
            /// Use an color tint transition.
            /// </summary>
            ColorTint,

            /// <summary>
            /// Use a sprite swap transition.
            /// </summary>
            SpriteSwap,

            /// <summary>
            /// Use an animation transition.
            /// </summary>
            Animation
        }

        // Type of the transition that occurs when the button state changes.
        [FormerlySerializedAs("transition")]
        [SerializeField]
        private Transition m_Transition = Transition.ColorTint;

        // Colors used for a color tint-based transition.
        [FormerlySerializedAs("colors")]
        [SerializeField]
        private ColorBlock m_Colors = ColorBlock.defaultColorBlock;

        // Sprites used for a Image swap-based transition.
        [FormerlySerializedAs("spriteState")]
        [SerializeField]
        private SpriteState m_SpriteState;

        [FormerlySerializedAs("animationTriggers")]
        [SerializeField]
        private AnimationTriggers m_AnimationTriggers = new AnimationTriggers();

        [Tooltip("Can the Selectable be interacted with?")]
        [SerializeField]
        private bool m_Interactable = true;

        // Graphic that will be colored.
        [FormerlySerializedAs("highlightGraphic")]
        [FormerlySerializedAs("m_HighlightGraphic")]
        [SerializeField]
        private Graphic m_TargetGraphic;


        private bool m_GroupsAllowInteraction = true;
        protected int m_CurrentIndex = -1;

        /// <summary>
        /// The Navigation setting for this selectable object.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public Button button;
        ///
        ///     void Start()
        ///     {
        ///         //Set the navigation to the default value. ("Automatic" is the default value).
        ///         button.navigation = Navigation.defaultNavigation;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public Navigation        navigation        { get { return m_Navigation; } set { if (SetPropertyUtility.SetStruct(ref m_Navigation, value))        OnSetProperty(); } }

        /// <summary>
        /// The type of transition that will be applied to the targetGraphic when the state changes.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public Button btnMain;
        ///
        ///     void SomeFunction()
        ///     {
        ///         //Sets the main button's transition setting to "Color Tint".
        ///         btnMain.transition = Selectable.Transition.ColorTint;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public Transition        transition        { get { return m_Transition; } set { if (SetPropertyUtility.SetStruct(ref m_Transition, value))        OnSetProperty(); } }

        /// <summary>
        /// The ColorBlock for this selectable object.
        /// </summary>
        /// <remarks>
        /// Modifications will not be visible if  transition is not ColorTint.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public Button button;
        ///
        ///     void Start()
        ///     {
        ///         //Resets the colors in the buttons transitions.
        ///         button.colors = ColorBlock.defaultColorBlock;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public ColorBlock        colors            { get { return m_Colors; } set { if (SetPropertyUtility.SetStruct(ref m_Colors, value))            OnSetProperty(); } }

        /// <summary>
        /// The SpriteState for this selectable object.
        /// </summary>
        /// <remarks>
        /// Modifications will not be visible if transition is not SpriteSwap.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     //Creates an instance of a sprite state (This includes the highlighted, pressed and disabled sprite.
        ///     public SpriteState sprState = new SpriteState();
        ///     public Button btnMain;
        ///
        ///
        ///     void Start()
        ///     {
        ///         //Assigns the new sprite states to the button.
        ///         btnMain.spriteState = sprState;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public SpriteState       spriteState       { get { return m_SpriteState; } set { if (SetPropertyUtility.SetStruct(ref m_SpriteState, value))       OnSetProperty(); } }

        /// <summary>
        /// The AnimationTriggers for this selectable object.
        /// </summary>
        /// <remarks>
        /// Modifications will not be visible if transition is not Animation.
        /// </remarks>
        public AnimationTriggers animationTriggers { get { return m_AnimationTriggers; } set { if (SetPropertyUtility.SetClass(ref m_AnimationTriggers, value)) OnSetProperty(); } }

        /// <summary>
        /// Graphic that will be transitioned upon.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public Image newImage;
        ///     public Button btnMain;
        ///
        ///     void SomeFunction()
        ///     {
        ///         //Displays the sprite transitions on the image when the transition to Highlighted,pressed or disabled is made.
        ///         btnMain.targetGraphic = newImage;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public Graphic           targetGraphic     { get { return m_TargetGraphic; } set { if (SetPropertyUtility.SetClass(ref m_TargetGraphic, value))     OnSetProperty(); } }

        /// <summary>
        /// Is this object interactable.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // required when using UI elements in scripts
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Button startButton;
        ///     public bool playersReady;
        ///
        ///
        ///     void Update()
        ///     {
        ///         // checks if the players are ready and if the start button is useable
        ///         if (playersReady == true && startButton.interactable == false)
        ///         {
        ///             //allows the start button to be used
        ///             startButton.interactable = true;
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public bool              interactable
        {
            get { return m_Interactable; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_Interactable, value))
                {
                    if (!m_Interactable && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
                        EventSystem.current.SetSelectedGameObject(null);
                    OnSetProperty();
                }
            }
        }

        private bool             isPointerInside   { get; set; }
        private bool             isPointerDown     { get; set; }
        private bool             hasSelection      { get; set; }

        protected Selectable()
        {}

        /// <summary>
        /// Convenience function that converts the referenced Graphic to a Image, if possible.
        /// </summary>
        public Image image
        {
            get { return m_TargetGraphic as Image; }
            set { m_TargetGraphic = value; }
        }

        /// <summary>
        /// Convenience function to get the Animator component on the GameObject.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     private Animator buttonAnimator;
        ///     public Button button;
        ///
        ///     void Start()
        ///     {
        ///         //Assigns the "buttonAnimator" with the button's animator.
        ///         buttonAnimator = button.animator;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
#if PACKAGE_ANIMATION
        public Animator animator
        {
            get { return GetComponent<Animator>(); }
        }
#endif

        protected override void Awake()
        {
            if (m_TargetGraphic == null)
                m_TargetGraphic = GetComponent<Graphic>();
        }

        private readonly List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();
        protected override void OnCanvasGroupChanged()
        {
            var parentGroupAllowsInteraction = ParentGroupAllowsInteraction();

            if (parentGroupAllowsInteraction != m_GroupsAllowInteraction)
            {
                m_GroupsAllowInteraction = parentGroupAllowsInteraction;
                OnSetProperty();
            }
        }

        bool ParentGroupAllowsInteraction()
        {
            Transform t = transform;
            while (t != null)
            {
                t.GetComponents(m_CanvasGroupCache);
                for (var i = 0; i < m_CanvasGroupCache.Count; i++)
                {
                    if (m_CanvasGroupCache[i].enabled && !m_CanvasGroupCache[i].interactable)
                        return false;

                    if (m_CanvasGroupCache[i].ignoreParentGroups)
                        return true;
                }

                t = t.parent;
            }

            return true;
        }

        /// <summary>
        /// Is the object interactable.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // required when using UI elements in scripts
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Button startButton;
        ///
        ///     void Update()
        ///     {
        ///         if (!startButton.IsInteractable())
        ///         {
        ///             Debug.Log("Start Button has been Disabled");
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual bool IsInteractable()
        {
            return m_GroupsAllowInteraction && m_Interactable;
        }

        // Call from unity if animation properties have changed
        protected override void OnDidApplyAnimationProperties()
        {
            OnSetProperty();
        }

        // Select on enable and add to the list.
        protected override void OnEnable()
        {
            //Check to avoid multiple OnEnable() calls for each selectable
            if (m_EnableCalled)
                return;

            base.OnEnable();

            if (s_SelectableCount == s_Selectables.Length)
            {
                Selectable[] temp = new Selectable[s_Selectables.Length * 2];
                Array.Copy(s_Selectables, temp, s_Selectables.Length);
                s_Selectables = temp;
            }

            if (EventSystem.current && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                hasSelection = true;
            }

            m_CurrentIndex = s_SelectableCount;
            s_Selectables[m_CurrentIndex] = this;
            s_SelectableCount++;
            isPointerDown = false;
            m_GroupsAllowInteraction = ParentGroupAllowsInteraction();
            DoStateTransition(currentSelectionState, true);

            m_EnableCalled = true;
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            // If our parenting changes figure out if we are under a new CanvasGroup.
            OnCanvasGroupChanged();
        }

        private void OnSetProperty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DoStateTransition(currentSelectionState, true);
            else
#endif
            DoStateTransition(currentSelectionState, false);
        }

        // Remove from the list.
        protected override void OnDisable()
        {
            //Check to avoid multiple OnDisable() calls for each selectable
            if (!m_EnableCalled)
                return;

            s_SelectableCount--;

            // Update the last elements index to be this index
            s_Selectables[s_SelectableCount].m_CurrentIndex = m_CurrentIndex;

            // Swap the last element and this element
            s_Selectables[m_CurrentIndex] = s_Selectables[s_SelectableCount];

            // null out last element.
            s_Selectables[s_SelectableCount] = null;

            InstantClearState();
            base.OnDisable();

            m_EnableCalled = false;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsPressed())
            {
                InstantClearState();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_Colors.fadeDuration = Mathf.Max(m_Colors.fadeDuration, 0.0f);

            // OnValidate can be called before OnEnable, this makes it unsafe to access other components
            // since they might not have been initialized yet.
            // OnSetProperty potentially access Animator or Graphics. (case 618186)
            if (isActiveAndEnabled)
            {
                if (!interactable && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
                    EventSystem.current.SetSelectedGameObject(null);
                // Need to clear out the override image on the target...
                DoSpriteSwap(null);

                // If the transition mode got changed, we need to clear all the transitions, since we don't know what the old transition mode was.
                StartColorTween(Color.white, true);
                TriggerAnimation(m_AnimationTriggers.normalTrigger);

                // And now go to the right state.
                DoStateTransition(currentSelectionState, true);
            }
        }

        protected override void Reset()
        {
            m_TargetGraphic = GetComponent<Graphic>();
        }

#endif // if UNITY_EDITOR

        protected SelectionState currentSelectionState
        {
            get
            {
                if (!IsInteractable())
                    return SelectionState.Disabled;
                if (isPointerDown)
                    return SelectionState.Pressed;
                if (hasSelection)
                    return SelectionState.Selected;
                if (isPointerInside)
                    return SelectionState.Highlighted;
                return SelectionState.Normal;
            }
        }

        /// <summary>
        /// Clear any internal state from the Selectable (used when disabling).
        /// </summary>
        protected virtual void InstantClearState()
        {
            string triggerName = m_AnimationTriggers.normalTrigger;

            isPointerInside = false;
            isPointerDown = false;
            hasSelection = false;

            switch (m_Transition)
            {
                case Transition.ColorTint:
                    StartColorTween(Color.white, true);
                    break;
                case Transition.SpriteSwap:
                    DoSpriteSwap(null);
                    break;
                case Transition.Animation:
                    TriggerAnimation(triggerName);
                    break;
            }
        }

        /// <summary>
        /// Transition the Selectable to the entered state.
        /// </summary>
        /// <param name="state">State to transition to</param>
        /// <param name="instant">Should the transition occur instantly.</param>
        protected virtual void DoStateTransition(SelectionState state, bool instant)
        {
            if (!gameObject.activeInHierarchy)
                return;

            Color tintColor;
            Sprite transitionSprite;
            string triggerName;

            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = m_Colors.normalColor;
                    transitionSprite = null;
                    triggerName = m_AnimationTriggers.normalTrigger;
                    break;
                case SelectionState.Highlighted:
                    tintColor = m_Colors.highlightedColor;
                    transitionSprite = m_SpriteState.highlightedSprite;
                    triggerName = m_AnimationTriggers.highlightedTrigger;
                    break;
                case SelectionState.Pressed:
                    tintColor = m_Colors.pressedColor;
                    transitionSprite = m_SpriteState.pressedSprite;
                    triggerName = m_AnimationTriggers.pressedTrigger;
                    break;
                case SelectionState.Selected:
                    tintColor = m_Colors.selectedColor;
                    transitionSprite = m_SpriteState.selectedSprite;
                    triggerName = m_AnimationTriggers.selectedTrigger;
                    break;
                case SelectionState.Disabled:
                    tintColor = m_Colors.disabledColor;
                    transitionSprite = m_SpriteState.disabledSprite;
                    triggerName = m_AnimationTriggers.disabledTrigger;
                    break;
                default:
                    tintColor = Color.black;
                    transitionSprite = null;
                    triggerName = string.Empty;
                    break;
            }

            switch (m_Transition)
            {
                case Transition.ColorTint:
                    StartColorTween(tintColor * m_Colors.colorMultiplier, instant);
                    break;
                case Transition.SpriteSwap:
                    DoSpriteSwap(transitionSprite);
                    break;
                case Transition.Animation:
                    TriggerAnimation(triggerName);
                    break;
            }
        }

        /// <summary>
        /// An enumeration of selected states of objects
        /// </summary>
        protected enum SelectionState
        {
            /// <summary>
            /// The UI object can be selected.
            /// </summary>
            Normal,

            /// <summary>
            /// The UI object is highlighted.
            /// </summary>
            Highlighted,

            /// <summary>
            /// The UI object is pressed.
            /// </summary>
            Pressed,

            /// <summary>
            /// The UI object is selected
            /// </summary>
            Selected,

            /// <summary>
            /// The UI object cannot be selected.
            /// </summary>
            Disabled,
        }

        // Selection logic

        /// <summary>
        /// Finds the selectable object next to this one.
        /// </summary>
        /// <remarks>
        /// The direction is determined by a Vector3 variable.
        /// </remarks>
        /// <param name="dir">The direction in which to search for a neighbouring Selectable object.</param>
        /// <returns>The neighbouring Selectable object. Null if none found.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // required when using UI elements in scripts
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     //Sets the direction as "Up" (Y is in positive).
        ///     public Vector3 direction = new Vector3(0, 1, 0);
        ///     public Button btnMain;
        ///
        ///     public void Start()
        ///     {
        ///         //Finds and assigns the selectable above the main button
        ///         Selectable newSelectable = btnMain.FindSelectable(direction);
        ///
        ///         Debug.Log(newSelectable.name);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public Selectable FindSelectable(Vector3 dir)
        {
            dir = dir.normalized;
            Vector3 localDir = Quaternion.Inverse(transform.rotation) * dir;
            Vector3 pos = transform.TransformPoint(GetPointOnRectEdge(transform as RectTransform, localDir));
            float maxScore = Mathf.NegativeInfinity;
            float maxFurthestScore = Mathf.NegativeInfinity;
            float score = 0;

            bool wantsWrapAround = navigation.wrapAround && (m_Navigation.mode == Navigation.Mode.Vertical || m_Navigation.mode == Navigation.Mode.Horizontal);

            Selectable bestPick = null;
            Selectable bestFurthestPick = null;

            for (int i = 0; i < s_SelectableCount; ++i)
            {
                Selectable sel = s_Selectables[i];

                if (sel == this)
                    continue;

                if (!sel.IsInteractable() || sel.navigation.mode == Navigation.Mode.None)
                    continue;

#if UNITY_EDITOR
                // Apart from runtime use, FindSelectable is used by custom editors to
                // draw arrows between different selectables. For scene view cameras,
                // only selectables in the same stage should be considered.
                if (Camera.current != null && !UnityEditor.SceneManagement.StageUtility.IsGameObjectRenderedByCamera(sel.gameObject, Camera.current))
                    continue;
#endif

                var selRect = sel.transform as RectTransform;
                Vector3 selCenter = selRect != null ? (Vector3)selRect.rect.center : Vector3.zero;
                Vector3 myVector = sel.transform.TransformPoint(selCenter) - pos;

                // Value that is the distance out along the direction.
                float dot = Vector3.Dot(dir, myVector);

                // If element is in wrong direction and we have wrapAround enabled check and cache it if furthest away.
                if (wantsWrapAround && dot < 0)
                {
                    score = -dot * myVector.sqrMagnitude;

                    if (score > maxFurthestScore)
                    {
                        maxFurthestScore = score;
                        bestFurthestPick = sel;
                    }

                    continue;
                }

                // Skip elements that are in the wrong direction or which have zero distance.
                // This also ensures that the scoring formula below will not have a division by zero error.
                if (dot <= 0)
                    continue;

                // This scoring function has two priorities:
                // - Score higher for positions that are closer.
                // - Score higher for positions that are located in the right direction.
                // This scoring function combines both of these criteria.
                // It can be seen as this:
                //   Dot (dir, myVector.normalized) / myVector.magnitude
                // The first part equals 1 if the direction of myVector is the same as dir, and 0 if it's orthogonal.
                // The second part scores lower the greater the distance is by dividing by the distance.
                // The formula below is equivalent but more optimized.
                //
                // If a given score is chosen, the positions that evaluate to that score will form a circle
                // that touches pos and whose center is located along dir. A way to visualize the resulting functionality is this:
                // From the position pos, blow up a circular balloon so it grows in the direction of dir.
                // The first Selectable whose center the circular balloon touches is the one that's chosen.
                score = dot / myVector.sqrMagnitude;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPick = sel;
                }
            }

            if (wantsWrapAround && null == bestPick) return bestFurthestPick;

            return bestPick;
        }

        private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
        {
            if (rect == null)
                return Vector3.zero;
            if (dir != Vector2.zero)
                dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
            return dir;
        }

        // Convenience function -- change the selection to the specified object if it's not null and happens to be active.
        void Navigate(AxisEventData eventData, Selectable sel)
        {
            if (sel != null && sel.IsActive())
                eventData.selectedObject = sel.gameObject;
        }

        /// <summary>
        /// Find the selectable object to the left of this one.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // required when using UI elements in scripts
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public Button btnMain;
        ///
        ///     // Disables the selectable UI element directly to the left of the Start Button
        ///     public void IgnoreSelectables()
        ///     {
        ///         //Finds the selectable UI element to the left the start button and assigns it to a variable of type "Selectable"
        ///         Selectable secondButton = startButton.FindSelectableOnLeft();
        ///         //Disables interaction with the selectable UI element
        ///         secondButton.interactable = false;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual Selectable FindSelectableOnLeft()
        {
            if (m_Navigation.mode == Navigation.Mode.Explicit)
            {
                return m_Navigation.selectOnLeft;
            }
            if ((m_Navigation.mode & Navigation.Mode.Horizontal) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.left);
            }
            return null;
        }

        /// <summary>
        /// Find the selectable object to the right of this one.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // required when using UI elements in scripts
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public Button btnMain;
        ///
        ///     // Disables the selectable UI element directly to the right the Start Button
        ///     public void IgnoreSelectables()
        ///     {
        ///         //Finds the selectable UI element to the right the start button and assigns it to a variable of type "Selectable"
        ///         Selectable secondButton = startButton.FindSelectableOnRight();
        ///         //Disables interaction with the selectable UI element
        ///         secondButton.interactable = false;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual Selectable FindSelectableOnRight()
        {
            if (m_Navigation.mode == Navigation.Mode.Explicit)
            {
                return m_Navigation.selectOnRight;
            }
            if ((m_Navigation.mode & Navigation.Mode.Horizontal) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.right);
            }
            return null;
        }

        /// <summary>
        /// The Selectable object above current
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // required when using UI elements in scripts
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public Button btnMain;
        ///
        ///     // Disables the selectable UI element directly above the Start Button
        ///     public void IgnoreSelectables()
        ///     {
        ///         //Finds the selectable UI element above the start button and assigns it to a variable of type "Selectable"
        ///         Selectable secondButton = startButton.FindSelectableOnUp();
        ///         //Disables interaction with the selectable UI element
        ///         secondButton.interactable = false;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual Selectable FindSelectableOnUp()
        {
            if (m_Navigation.mode == Navigation.Mode.Explicit)
            {
                return m_Navigation.selectOnUp;
            }
            if ((m_Navigation.mode & Navigation.Mode.Vertical) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.up);
            }
            return null;
        }

        /// <summary>
        /// Find the selectable object below this one.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // required when using UI elements in scripts
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Button startButton;
        ///
        ///     // Disables the selectable UI element directly below the Start Button
        ///     public void IgnoreSelectables()
        ///     {
        ///         //Finds the selectable UI element below the start button and assigns it to a variable of type "Selectable"
        ///         Selectable secondButton = startButton.FindSelectableOnDown();
        ///         //Disables interaction with the selectable UI element
        ///         secondButton.interactable = false;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual Selectable FindSelectableOnDown()
        {
            if (m_Navigation.mode == Navigation.Mode.Explicit)
            {
                return m_Navigation.selectOnDown;
            }
            if ((m_Navigation.mode & Navigation.Mode.Vertical) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.down);
            }
            return null;
        }

        /// <summary>
        /// Determine in which of the 4 move directions the next selectable object should be found.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        /// using UnityEngine.EventSystems;// Required when using Event data.
        ///
        /// public class ExampleClass : MonoBehaviour, IMoveHandler
        /// {
        ///     //When the focus moves to another selectable object, Invoke this Method.
        ///     public void OnMove(AxisEventData eventData)
        ///     {
        ///         //Assigns the move direction and the raw input vector representing the direction from the event data.
        ///         MoveDirection moveDir = eventData.moveDir;
        ///         Vector2 moveVector = eventData.moveVector;
        ///
        ///         //Displays the information in the console
        ///         Debug.Log(moveDir + ", " + moveVector);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                    Navigate(eventData, FindSelectableOnRight());
                    break;

                case MoveDirection.Up:
                    Navigate(eventData, FindSelectableOnUp());
                    break;

                case MoveDirection.Left:
                    Navigate(eventData, FindSelectableOnLeft());
                    break;

                case MoveDirection.Down:
                    Navigate(eventData, FindSelectableOnDown());
                    break;
            }
        }

        void StartColorTween(Color targetColor, bool instant)
        {
            if (m_TargetGraphic == null)
                return;

            m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, true, true);
        }

        void DoSpriteSwap(Sprite newSprite)
        {
            if (image == null)
                return;

            image.overrideSprite = newSprite;
        }

        void TriggerAnimation(string triggername)
        {
#if PACKAGE_ANIMATION
            if (transition != Transition.Animation || animator == null || !animator.isActiveAndEnabled || !animator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
                return;

            animator.ResetTrigger(m_AnimationTriggers.normalTrigger);
            animator.ResetTrigger(m_AnimationTriggers.highlightedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.pressedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.selectedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.disabledTrigger);

            animator.SetTrigger(triggername);
#endif
        }

        /// <summary>
        /// Returns whether the selectable is currently 'highlighted' or not.
        /// </summary>
        /// <remarks>
        /// Use this to check if the selectable UI element is currently highlighted.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //Create a UI element. To do this go to Create>UI and select from the list. Attach this script to the UI GameObject to see this script working. The script also works with non-UI elements, but highlighting works better with UI.
        ///
        /// using UnityEngine;
        /// using UnityEngine.Events;
        /// using UnityEngine.EventSystems;
        /// using UnityEngine.UI;
        ///
        /// //Use the Selectable class as a base class to access the IsHighlighted method
        /// public class Example : Selectable
        /// {
        ///     //Use this to check what Events are happening
        ///     BaseEventData m_BaseEvent;
        ///
        ///     void Update()
        ///     {
        ///         //Check if the GameObject is being highlighted
        ///         if (IsHighlighted())
        ///         {
        ///             //Output that the GameObject was highlighted, or do something else
        ///             Debug.Log("Selectable is Highlighted");
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        protected bool IsHighlighted()
        {
            if (!IsActive() || !IsInteractable())
                return false;
            return isPointerInside && !isPointerDown && !hasSelection;
        }

        /// <summary>
        /// Whether the current selectable is being pressed.
        /// </summary>
        protected bool IsPressed()
        {
            if (!IsActive() || !IsInteractable())
                return false;
            return isPointerDown;
        }

        // Change the button to the correct state
        private void EvaluateAndTransitionToSelectionState()
        {
            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(currentSelectionState, false);
        }

        /// <summary>
        /// Evaluate current state and transition to pressed state.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        /// using UnityEngine.EventSystems;// Required when using Event data.
        ///
        /// public class ExampleClass : MonoBehaviour, IPointerDownHandler// required interface when using the OnPointerDown method.
        /// {
        ///     //Do this when the mouse is clicked over the selectable object this script is attached to.
        ///     public void OnPointerDown(PointerEventData eventData)
        ///     {
        ///         Debug.Log(this.gameObject.name + " Was Clicked.");
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            // Selection tracking
            if (IsInteractable() && navigation.mode != Navigation.Mode.None && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            isPointerDown = true;
            EvaluateAndTransitionToSelectionState();
        }

        /// <summary>
        /// Evaluate eventData and transition to appropriate state.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        /// using UnityEngine.EventSystems;// Required when using Event data.
        ///
        /// public class ExampleClass : MonoBehaviour, IPointerUpHandler, IPointerDownHandler// These are the interfaces the OnPointerUp method requires.
        /// {
        ///     //OnPointerDown is also required to receive OnPointerUp callbacks
        ///     public void OnPointerDown(PointerEventData eventData)
        ///     {
        ///     }
        ///
        ///     //Do this when the mouse click on this selectable UI object is released.
        ///     public void OnPointerUp(PointerEventData eventData)
        ///     {
        ///         Debug.Log("The mouse click was released");
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            isPointerDown = false;
            EvaluateAndTransitionToSelectionState();
        }

        /// <summary>
        /// Evaluate current state and transition to appropriate state.
        /// New state could be pressed or hover depending on pressed state.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        /// using UnityEngine.EventSystems;// Required when using Event data.
        ///
        /// public class ExampleClass : MonoBehaviour, IPointerEnterHandler// required interface when using the OnPointerEnter method.
        /// {
        ///     //Do this when the cursor enters the rect area of this selectable UI object.
        ///     public void OnPointerEnter(PointerEventData eventData)
        ///     {
        ///         Debug.Log("The cursor entered the selectable UI element.");
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
            EvaluateAndTransitionToSelectionState();
        }

        /// <summary>
        /// Evaluate current state and transition to normal state.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        /// using UnityEngine.EventSystems;// Required when using Event data.
        ///
        /// public class ExampleClass : MonoBehaviour, IPointerExitHandler// required interface when using the OnPointerExit method.
        /// {
        ///     //Do this when the cursor exits the rect area of this selectable UI object.
        ///     public void OnPointerExit(PointerEventData eventData)
        ///     {
        ///         Debug.Log("The cursor exited the selectable UI element.");
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
            EvaluateAndTransitionToSelectionState();
        }

        /// <summary>
        /// Set selection and transition to appropriate state.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        /// using UnityEngine.EventSystems;// Required when using Event data.
        ///
        /// public class ExampleClass : MonoBehaviour, ISelectHandler// required interface when using the OnSelect method.
        /// {
        ///     //Do this when the selectable UI object is selected.
        ///     public void OnSelect(BaseEventData eventData)
        ///     {
        ///         Debug.Log(this.gameObject.name + " was selected");
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual void OnSelect(BaseEventData eventData)
        {
            hasSelection = true;
            EvaluateAndTransitionToSelectionState();
        }

        /// <summary>
        /// Unset selection and transition to appropriate state.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        /// using UnityEngine.EventSystems;// Required when using Event data.
        ///
        /// public class ExampleClass : MonoBehaviour, IDeselectHandler //This Interface is required to receive OnDeselect callbacks.
        /// {
        ///     public void OnDeselect(BaseEventData data)
        ///     {
        ///         Debug.Log("Deselected");
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual void OnDeselect(BaseEventData eventData)
        {
            hasSelection = false;
            EvaluateAndTransitionToSelectionState();
        }

        /// <summary>
        /// Selects this Selectable.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // required when using UI elements in scripts
        /// using UnityEngine.EventSystems;// Required when using Event data.
        ///
        /// public class ExampleClass : MonoBehaviour// required interface when using the OnSelect method.
        /// {
        ///     public InputField myInputField;
        ///
        ///     //Do this OnClick.
        ///     public void SaveGame()
        ///     {
        ///         //Makes the Input Field the selected UI Element.
        ///         myInputField.Select();
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual void Select()
        {
            if (EventSystem.current == null || EventSystem.current.alreadySelecting)
                return;

            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
