using System;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    /// <summary>
    /// Structure that stores the state of an animation transition on a Selectable.
    /// </summary>
    [Serializable]
    public class AnimationTriggers
    {
        private const string kDefaultNormalAnimName      = "Normal";
        private const string kDefaultHighlightedAnimName = "Highlighted";
        private const string kDefaultPressedAnimName     = "Pressed";
        private const string kDefaultSelectedAnimName    = "Selected";
        private const string kDefaultDisabledAnimName    = "Disabled";

        [FormerlySerializedAs("normalTrigger")]
        [SerializeField]
        private string m_NormalTrigger    = kDefaultNormalAnimName;

        [FormerlySerializedAs("highlightedTrigger")]
        [SerializeField]
        private string m_HighlightedTrigger = kDefaultHighlightedAnimName;

        [FormerlySerializedAs("pressedTrigger")]
        [SerializeField]
        private string m_PressedTrigger = kDefaultPressedAnimName;

        [FormerlySerializedAs("m_HighlightedTrigger")]
        [SerializeField]
        private string m_SelectedTrigger = kDefaultSelectedAnimName;

        [FormerlySerializedAs("disabledTrigger")]
        [SerializeField]
        private string m_DisabledTrigger = kDefaultDisabledAnimName;

        /// <summary>
        /// Trigger to send to animator when entering normal state.
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
        ///     public Animator buttonAnimator;
        ///     public Button button;
        ///     void SomeFunction()
        ///     {
        ///         //Sets the button to the Normal state (Useful when making tutorials).
        ///         buttonAnimator.SetTrigger(button.animationTriggers.normalTrigger);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public string normalTrigger      { get { return m_NormalTrigger; } set { m_NormalTrigger = value; } }

        /// <summary>
        /// Trigger to send to animator when entering highlighted state.
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
        ///     public Animator buttonAnimator;
        ///     public Button button;
        ///     void SomeFunction()
        ///     {
        ///         //Sets the button to the Highlighted state (Useful when making tutorials).
        ///         buttonAnimator.SetTrigger(button.animationTriggers.highlightedTrigger);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public string highlightedTrigger { get { return m_HighlightedTrigger; } set { m_HighlightedTrigger = value; } }

        /// <summary>
        /// Trigger to send to animator when entering pressed state.
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
        ///     public Animator buttonAnimator;
        ///     public Button button;
        ///     void SomeFunction()
        ///     {
        ///         //Sets the button to the Pressed state (Useful when making tutorials).
        ///         buttonAnimator.SetTrigger(button.animationTriggers.pressedTrigger);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public string pressedTrigger     { get { return m_PressedTrigger; } set { m_PressedTrigger = value; } }

        /// <summary>
        /// Trigger to send to animator when entering selected state.
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
        ///     public Animator buttonAnimator;
        ///     public Button button;
        ///     void SomeFunction()
        ///     {
        ///         //Sets the button to the Selected state (Useful when making tutorials).
        ///         buttonAnimator.SetTrigger(button.animationTriggers.selectedTrigger);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public string selectedTrigger    { get { return m_SelectedTrigger; } set { m_SelectedTrigger = value; } }

        /// <summary>
        /// Trigger to send to animator when entering disabled state.
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
        ///     public Animator buttonAnimator;
        ///     public Button button;
        ///     void SomeFunction()
        ///     {
        ///         //Sets the button to the Disabled state (Useful when making tutorials).
        ///         buttonAnimator.SetTrigger(button.animationTriggers.disabledTrigger);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public string disabledTrigger    { get { return m_DisabledTrigger; } set { m_DisabledTrigger = value; } }
    }
}
