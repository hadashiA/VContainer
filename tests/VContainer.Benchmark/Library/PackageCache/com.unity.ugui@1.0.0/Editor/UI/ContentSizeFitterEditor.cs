using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(ContentSizeFitter), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom Editor for the ContentSizeFitter Component.
    /// Extend this class to write a custom editor for a component derived from ContentSizeFitter.
    /// </summary>
    public class ContentSizeFitterEditor : SelfControllerEditor
    {
        SerializedProperty m_HorizontalFit;
        SerializedProperty m_VerticalFit;

        protected virtual void OnEnable()
        {
            m_HorizontalFit = serializedObject.FindProperty("m_HorizontalFit");
            m_VerticalFit = serializedObject.FindProperty("m_VerticalFit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_HorizontalFit, true);
            EditorGUILayout.PropertyField(m_VerticalFit, true);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
