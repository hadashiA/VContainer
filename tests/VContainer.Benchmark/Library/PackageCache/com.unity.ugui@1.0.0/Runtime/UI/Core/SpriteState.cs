using System;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    [Serializable]
    /// <summary>
    /// Structure that stores the state of a sprite transition on a Selectable.
    /// </summary>
    public struct SpriteState : IEquatable<SpriteState>
    {
        [SerializeField]
        private Sprite m_HighlightedSprite;

        [SerializeField]
        private Sprite m_PressedSprite;

        [FormerlySerializedAs("m_HighlightedSprite")]
        [SerializeField]
        private Sprite m_SelectedSprite;

        [SerializeField]
        private Sprite m_DisabledSprite;

        /// <summary>
        /// Highlighted sprite.
        /// </summary>
        public Sprite highlightedSprite    { get { return m_HighlightedSprite; } set { m_HighlightedSprite = value; } }

        /// <summary>
        /// Pressed sprite.
        /// </summary>
        public Sprite pressedSprite     { get { return m_PressedSprite; } set { m_PressedSprite = value; } }

        /// <summary>
        /// Selected sprite.
        /// </summary>
        public Sprite selectedSprite    { get { return m_SelectedSprite; } set { m_SelectedSprite = value; } }

        /// <summary>
        /// Disabled sprite.
        /// </summary>
        public Sprite disabledSprite    { get { return m_DisabledSprite; } set { m_DisabledSprite = value; } }

        public bool Equals(SpriteState other)
        {
            return highlightedSprite == other.highlightedSprite &&
                pressedSprite == other.pressedSprite &&
                selectedSprite == other.selectedSprite &&
                disabledSprite == other.disabledSprite;
        }
    }
}
