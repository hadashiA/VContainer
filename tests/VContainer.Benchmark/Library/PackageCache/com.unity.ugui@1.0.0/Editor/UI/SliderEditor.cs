using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(Slider), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom Editor for the Slider Component.
    /// Extend this class to write a custom editor for a component derived from Slider.
    /// </summary>
    public class SliderEditor : SelectableEditor
    {
        SerializedProperty m_Direction;
        SerializedProperty m_FillRect;
        SerializedProperty m_HandleRect;
        SerializedProperty m_MinValue;
        SerializedProperty m_MaxValue;
        SerializedProperty m_WholeNumbers;
        SerializedProperty m_Value;
        SerializedProperty m_OnValueChanged;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_FillRect = serializedObject.FindProperty("m_FillRect");
            m_HandleRect = serializedObject.FindProperty("m_HandleRect");
            m_Direction = serializedObject.FindProperty("m_Direction");
            m_MinValue = serializedObject.FindProperty("m_MinValue");
            m_MaxValue = serializedObject.FindProperty("m_MaxValue");
            m_WholeNumbers = serializedObject.FindProperty("m_WholeNumbers");
            m_Value = serializedObject.FindProperty("m_Value");
            m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_FillRect);
            EditorGUILayout.PropertyField(m_HandleRect);

            if (m_FillRect.objectReferenceValue != null || m_HandleRect.objectReferenceValue != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_Direction);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObjects(serializedObject.targetObjects, "Change Slider Direction");

                    Slider.Direction direction = (Slider.Direction)m_Direction.enumValueIndex;
                    foreach (var obj in serializedObject.targetObjects)
                    {
                        Slider slider = obj as Slider;
                        slider.SetDirection(direction, true);
                    }
                }

                EditorGUI.BeginChangeCheck();
                float newMin = EditorGUILayout.FloatField("Min Value", m_MinValue.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_WholeNumbers.boolValue ? Mathf.Round(newMin) < m_MaxValue.floatValue : newMin < m_MaxValue.floatValue)
                    {
                        m_MinValue.floatValue = newMin;
                        if (m_Value.floatValue < newMin)
                            m_Value.floatValue = newMin;
                    }
                }

                EditorGUI.BeginChangeCheck();
                float newMax = EditorGUILayout.FloatField("Max Value", m_MaxValue.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_WholeNumbers.boolValue ? Mathf.Round(newMax) > m_MinValue.floatValue : newMax > m_MinValue.floatValue)
                    {
                        m_MaxValue.floatValue = newMax;
                        if (m_Value.floatValue > newMax)
                            m_Value.floatValue = newMax;
                    }
                }

                EditorGUILayout.PropertyField(m_WholeNumbers);

                bool areMinMaxEqual = (m_MinValue.floatValue == m_MaxValue.floatValue);

                if (areMinMaxEqual)
                    EditorGUILayout.HelpBox("Min Value and Max Value cannot be equal.", MessageType.Warning);

                if (m_WholeNumbers.boolValue)
                    m_Value.floatValue = Mathf.Round(m_Value.floatValue);

                EditorGUI.BeginDisabledGroup(areMinMaxEqual);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Slider(m_Value, m_MinValue.floatValue, m_MaxValue.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    // Apply the change before sending the event
                    serializedObject.ApplyModifiedProperties();

                    foreach (var t in targets)
                    {
                        if (t is Slider slider)
                        {
                            slider.onValueChanged?.Invoke(slider.value);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();

                bool warning = false;
                foreach (var obj in serializedObject.targetObjects)
                {
                    Slider slider = obj as Slider;
                    Slider.Direction dir = slider.direction;
                    if (dir == Slider.Direction.LeftToRight || dir == Slider.Direction.RightToLeft)
                        warning = (slider.navigation.mode != Navigation.Mode.Automatic && (slider.FindSelectableOnLeft() != null || slider.FindSelectableOnRight() != null));
                    else
                        warning = (slider.navigation.mode != Navigation.Mode.Automatic && (slider.FindSelectableOnDown() != null || slider.FindSelectableOnUp() != null));
                }

                if (warning)
                    EditorGUILayout.HelpBox("The selected slider direction conflicts with navigation. Not all navigation options may work.", MessageType.Warning);

                // Draw the event notification options
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(m_OnValueChanged);
            }
            else
            {
                EditorGUILayout.HelpBox("Specify a RectTransform for the slider fill or the slider handle or both. Each must have a parent RectTransform that it can slide within.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
