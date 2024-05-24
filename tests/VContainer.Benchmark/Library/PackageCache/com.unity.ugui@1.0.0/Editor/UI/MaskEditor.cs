using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(Mask), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom Editor for the Mask component.
    /// Extend this class to write a custom editor for a component derived from Mask.
    /// </summary>
    public class MaskEditor : Editor
    {
        SerializedProperty m_ShowMaskGraphic;

        protected virtual void OnEnable()
        {
            m_ShowMaskGraphic = serializedObject.FindProperty("m_ShowMaskGraphic");
        }

        public override void OnInspectorGUI()
        {
            var graphic = (target as Mask).GetComponent<Graphic>();

            if (graphic && !graphic.IsActive())
                EditorGUILayout.HelpBox("Masking disabled due to Graphic component being disabled.", MessageType.Warning);

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_ShowMaskGraphic);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
