using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEditor.UI
{
    [CustomPropertyDrawer(typeof(SpriteState), true)]
    /// <summary>
    /// This is a PropertyDrawer for SpriteState. It is implemented using the standard Unity PropertyDrawer framework.
    /// </summary>
    public class SpriteStateDrawer : PropertyDrawer
    {
        const string kHighlightedSprite = "m_HighlightedSprite";
        const string kPressedSprite = "m_PressedSprite";
        const string kSelectedSprite = "m_SelectedSprite";
        const string kDisabledSprite = "m_DisabledSprite";
        const string kVisualElementName = "SpriteState";

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            Rect drawRect = rect;
            drawRect.height = EditorGUIUtility.singleLineHeight;
            SerializedProperty highlightedSprite = prop.FindPropertyRelative(kHighlightedSprite);
            SerializedProperty pressedSprite = prop.FindPropertyRelative(kPressedSprite);
            SerializedProperty selectedSprite = prop.FindPropertyRelative(kSelectedSprite);
            SerializedProperty disabledSprite = prop.FindPropertyRelative(kDisabledSprite);

            EditorGUI.PropertyField(drawRect, highlightedSprite);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, pressedSprite);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, selectedSprite);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, disabledSprite);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return 4 * EditorGUIUtility.singleLineHeight + 3 * EditorGUIUtility.standardVerticalSpacing;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.name = kVisualElementName;

            var properties = new[]
            {
                property.FindPropertyRelative(kHighlightedSprite),
                property.FindPropertyRelative(kPressedSprite),
                property.FindPropertyRelative(kSelectedSprite),
                property.FindPropertyRelative(kDisabledSprite)
            };

            foreach (var prop in properties)
            {
                container.Add(new PropertyField(prop));
            }

            return container;
        }
    }
}
