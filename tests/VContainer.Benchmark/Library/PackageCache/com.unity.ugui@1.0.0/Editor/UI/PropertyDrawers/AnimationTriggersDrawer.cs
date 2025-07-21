using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UnityEditor.UI
{
    [CustomPropertyDrawer(typeof(AnimationTriggers), true)]
    /// <summary>
    /// This is a PropertyDrawer for AnimationTriggers. It is implemented using the standard Unity PropertyDrawer framework.
    /// </summary>
    public class AnimationTriggersDrawer : PropertyDrawer
    {
        const string kNormalTrigger = "m_NormalTrigger";
        const string kHighlightedTrigger = "m_HighlightedTrigger";
        const string kPressedTrigger = "m_PressedTrigger";
        const string kSelectedTrigger = "m_SelectedTrigger";
        const string kDisabledTrigger = "m_DisabledTrigger";

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            Rect drawRect = rect;
            drawRect.height = EditorGUIUtility.singleLineHeight;

            SerializedProperty normalTrigger = prop.FindPropertyRelative(kNormalTrigger);
            SerializedProperty higlightedTrigger = prop.FindPropertyRelative(kHighlightedTrigger);
            SerializedProperty pressedTrigger = prop.FindPropertyRelative(kPressedTrigger);
            SerializedProperty selectedTrigger = prop.FindPropertyRelative(kSelectedTrigger);
            SerializedProperty disabledTrigger = prop.FindPropertyRelative(kDisabledTrigger);

            EditorGUI.PropertyField(drawRect, normalTrigger);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, higlightedTrigger);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, pressedTrigger);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, selectedTrigger);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, disabledTrigger);
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return 5 * EditorGUIUtility.singleLineHeight + 4 * EditorGUIUtility.standardVerticalSpacing;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var properties = new[]
            {
                property.FindPropertyRelative(kNormalTrigger),
                property.FindPropertyRelative(kHighlightedTrigger),
                property.FindPropertyRelative(kPressedTrigger),
                property.FindPropertyRelative(kSelectedTrigger),
                property.FindPropertyRelative(kDisabledTrigger),
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
