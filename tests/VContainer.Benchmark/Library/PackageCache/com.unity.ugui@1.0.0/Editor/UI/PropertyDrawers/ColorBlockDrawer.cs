using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEditor.UI
{
    [CustomPropertyDrawer(typeof(ColorBlock), true)]
    /// <summary>
    /// This is a PropertyDrawer for ColorBlock. It is implemented using the standard Unity PropertyDrawer framework..
    /// </summary>
    public class ColorBlockDrawer : PropertyDrawer
    {
        const string kNormalColor = "m_NormalColor";
        const string kHighlightedColor = "m_HighlightedColor";
        const string kPressedColor = "m_PressedColor";
        const string kSelectedColor = "m_SelectedColor";
        const string kDisabledColor = "m_DisabledColor";
        const string kColorMultiplier = "m_ColorMultiplier";
        const string kFadeDuration = "m_FadeDuration";

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            Rect drawRect = rect;
            drawRect.height = EditorGUIUtility.singleLineHeight;

            SerializedProperty normalColor = prop.FindPropertyRelative(kNormalColor);
            SerializedProperty highlighted = prop.FindPropertyRelative(kHighlightedColor);
            SerializedProperty pressedColor = prop.FindPropertyRelative(kPressedColor);
            SerializedProperty selectedColor = prop.FindPropertyRelative(kSelectedColor);
            SerializedProperty disabledColor = prop.FindPropertyRelative(kDisabledColor);
            SerializedProperty colorMultiplier = prop.FindPropertyRelative(kColorMultiplier);
            SerializedProperty fadeDuration = prop.FindPropertyRelative(kFadeDuration);

            EditorGUI.PropertyField(drawRect, normalColor);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, highlighted);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, pressedColor);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, selectedColor);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, disabledColor);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, colorMultiplier);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, fadeDuration);
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return 7 * EditorGUIUtility.singleLineHeight + 6 * EditorGUIUtility.standardVerticalSpacing;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new VisualElement();

            var properties = new[]
            {
                property.FindPropertyRelative(kNormalColor),
                property.FindPropertyRelative(kHighlightedColor),
                property.FindPropertyRelative(kPressedColor),
                property.FindPropertyRelative(kSelectedColor),
                property.FindPropertyRelative(kDisabledColor),
                property.FindPropertyRelative(kColorMultiplier),
                property.FindPropertyRelative(kFadeDuration)
            };

            foreach (var prop in properties)
            {
                var field = new PropertyField(prop);
                container.Add(field);
            }

            return container;
        }
    }
}
