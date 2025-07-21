using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(RectMask2D), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom editor for the RectMask2d component.
    /// Extend this class to write a custom editor for a component derived from Mask.
    /// </summary>
    public class RectMask2DEditor : Editor
    {
        SerializedProperty m_Padding;
        SerializedProperty m_Softness;
        GUIContent m_PaddingContent;
        GUIContent m_LeftContent;
        GUIContent m_RightContent;
        GUIContent m_TopContent;
        GUIContent m_BottomContent;

        static private bool m_ShowOffsets = false;

        protected virtual void OnEnable()
        {
            m_PaddingContent = EditorGUIUtility.TrTextContent("Padding");
            m_LeftContent = EditorGUIUtility.TrTextContent("Left");
            m_RightContent = EditorGUIUtility.TrTextContent("Right");
            m_TopContent = EditorGUIUtility.TrTextContent("Top");
            m_BottomContent = EditorGUIUtility.TrTextContent("Bottom");
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_Softness = serializedObject.FindProperty("m_Softness");
        }

        public override void OnInspectorGUI()
        {
            m_ShowOffsets = EditorGUILayout.Foldout(m_ShowOffsets, m_PaddingContent, true);

            if (m_ShowOffsets)
                OffsetGUI();

            EditorGUILayout.PropertyField(m_Softness);

            serializedObject.ApplyModifiedProperties();
        }

        void OffsetGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.indentLevel++;
                Vector4 newPadding = m_Padding.vector4Value;

                newPadding.x = EditorGUILayout.FloatField(m_LeftContent, newPadding.x);
                newPadding.z = EditorGUILayout.FloatField(m_RightContent, newPadding.z);
                newPadding.w = EditorGUILayout.FloatField(m_TopContent, newPadding.w);
                newPadding.y = EditorGUILayout.FloatField(m_BottomContent, newPadding.y);

                if (check.changed)
                {
                    m_Padding.vector4Value = newPadding;
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
