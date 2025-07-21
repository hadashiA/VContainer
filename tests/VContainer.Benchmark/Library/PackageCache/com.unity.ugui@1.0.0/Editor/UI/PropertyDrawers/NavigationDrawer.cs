using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UnityEditor.UI
{
    [CustomPropertyDrawer(typeof(Navigation), true)]
    /// <summary>
    /// This is a PropertyDrawer for Navigation. It is implemented using the standard Unity PropertyDrawer framework.
    /// </summary>
    public class NavigationDrawer : PropertyDrawer
    {
        const string kNavigation = "Navigation";

        const string kModeProp = "m_Mode";
        const string kWrapAroundProp = "m_WrapAround";
        const string kSelectOnUpProp = "m_SelectOnUp";
        const string kSelectOnDownProp = "m_SelectOnDown";
        const string kSelectOnLeftProp = "m_SelectOnLeft";
        const string kSelectOnRightProp = "m_SelectOnRight";

        const string kHiddenClass = "unity-ui-navigation-hidden";

        private class Styles
        {
            readonly public GUIContent navigationContent;

            public Styles()
            {
                navigationContent = EditorGUIUtility.TrTextContent(kNavigation);
            }
        }

        private static Styles s_Styles = null;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (s_Styles == null)
                s_Styles = new Styles();

            Rect drawRect = pos;
            drawRect.height = EditorGUIUtility.singleLineHeight;

            SerializedProperty navigation = prop.FindPropertyRelative(kModeProp);
            SerializedProperty wrapAround = prop.FindPropertyRelative(kWrapAroundProp);
            Navigation.Mode navMode = GetNavigationMode(navigation);

            EditorGUI.PropertyField(drawRect, navigation, s_Styles.navigationContent);

            ++EditorGUI.indentLevel;

            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            switch (navMode)
            {
                case Navigation.Mode.Horizontal:
                case Navigation.Mode.Vertical:
                {
                    EditorGUI.PropertyField(drawRect, wrapAround);
                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                break;
                case Navigation.Mode.Explicit:
                {
                    SerializedProperty selectOnUp = prop.FindPropertyRelative(kSelectOnUpProp);
                    SerializedProperty selectOnDown = prop.FindPropertyRelative(kSelectOnDownProp);
                    SerializedProperty selectOnLeft = prop.FindPropertyRelative(kSelectOnLeftProp);
                    SerializedProperty selectOnRight = prop.FindPropertyRelative(kSelectOnRightProp);

                    EditorGUI.PropertyField(drawRect, selectOnUp);
                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(drawRect, selectOnDown);
                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(drawRect, selectOnLeft);
                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(drawRect, selectOnRight);
                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                break;
            }

            --EditorGUI.indentLevel;
        }

        static Navigation.Mode GetNavigationMode(SerializedProperty navigation)
        {
            return (Navigation.Mode)navigation.enumValueIndex;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            SerializedProperty navigation = prop.FindPropertyRelative(kModeProp);
            if (navigation == null)
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            Navigation.Mode navMode = GetNavigationMode(navigation);

            switch (navMode)
            {
                case Navigation.Mode.None:
                    return EditorGUIUtility.singleLineHeight;
                case Navigation.Mode.Horizontal:
                case Navigation.Mode.Vertical:
                    return 2 * EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
                case Navigation.Mode.Explicit:
                    return 5 * EditorGUIUtility.singleLineHeight + 5 * EditorGUIUtility.standardVerticalSpacing;
                default:
                    return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        PropertyField PrepareField(VisualElement parent, string propertyPath, bool hideable = true, string label = null)
        {
            var field = new PropertyField(null, label) { bindingPath = propertyPath };
            if (hideable) field.AddToClassList(kHiddenClass);
            parent.Add(field);
            return field;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement() { name = kNavigation };
            var indented = new VisualElement() { name = "Indent" };

            indented.AddToClassList("unity-ui-navigation-indent");

            var navigation = PrepareField(container, kModeProp, false, kNavigation);
            var wrapAround = PrepareField(indented, kWrapAroundProp);
            var selectOnUp = PrepareField(indented, kSelectOnUpProp);
            var selectOnDown = PrepareField(indented, kSelectOnDownProp);
            var selectOnLeft = PrepareField(indented, kSelectOnLeftProp);
            var selectOnRight = PrepareField(indented, kSelectOnRightProp);

            Action<Navigation.Mode> callback = (value) =>
            {
                wrapAround.EnableInClassList(kHiddenClass, value != Navigation.Mode.Vertical && value != Navigation.Mode.Horizontal);
                selectOnUp.EnableInClassList(kHiddenClass, value != Navigation.Mode.Explicit);
                selectOnDown.EnableInClassList(kHiddenClass, value != Navigation.Mode.Explicit);
                selectOnLeft.EnableInClassList(kHiddenClass, value != Navigation.Mode.Explicit);
                selectOnRight.EnableInClassList(kHiddenClass, value != Navigation.Mode.Explicit);
            };

            navigation.RegisterValueChangeCallback((e) => callback.Invoke((Navigation.Mode)e.changedProperty.enumValueIndex));
            callback.Invoke((Navigation.Mode)property.FindPropertyRelative(kModeProp).enumValueFlag);

            container.Add(indented);
            return container;
        }
    }
}
