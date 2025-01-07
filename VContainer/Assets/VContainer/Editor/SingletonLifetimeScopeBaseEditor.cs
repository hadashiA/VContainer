using UnityEditor;
using VContainer.Unity.Extensions;

namespace VContainer.Editor
{
    [CustomEditor(typeof(SingletonLifetimeScopeBase), true)]
    public sealed class SingletonLifetimeScopeBaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorGUI.BeginDisabledGroup(true);
                var property = serializedObject.FindProperty("m_Script");
                EditorGUILayout.PropertyField(property, true);
                EditorGUI.EndDisabledGroup();
            }
            {
                var property = serializedObject.GetIterator();
                property.NextVisible(true);
                while (property.NextVisible(false))
                {
                    var isParentReference = property.propertyType == SerializedPropertyType.Generic
                                            && property.type == nameof(global::VContainer.Unity.ParentReference);
                    
                    EditorGUI.BeginDisabledGroup(isParentReference);
                    EditorGUILayout.PropertyField(property, true);
                    EditorGUI.EndDisabledGroup();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}