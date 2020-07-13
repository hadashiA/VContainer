using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VContainer.Unity;

namespace VContainer.Editor
{
    [CustomPropertyDrawer(typeof(ParentReference))]
    public sealed class ParentReferencePropertyDrawer : PropertyDrawer
    {
        static string[] GetAllTypeNames()
        {
            var result = new List<string> { "None" };

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(LifetimeScope)))
                    {
                        result.Add(type.FullName);
                    }
                }
            }
            return result.ToArray();
        }

        static string GetLabel(Type type) => $"{type.Namespace}/{type.Name}";

        string[] names;

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            if (names == null)
            {
                names = GetAllTypeNames();
            }

            var typeNameProp = prop.FindPropertyRelative("TypeName");

            using (new EditorGUI.PropertyScope(rect, label, prop))
            {
                var labelRect = new Rect(rect.x, rect.y, rect.width, 18f);
                var popupRect = new Rect(rect.x, rect.y + labelRect.height, rect.width , 18f);

                var index = Array.IndexOf(names, typeNameProp.stringValue);
                if (index < 0) index = 0;

                EditorGUI.LabelField(labelRect, "Parent");
                index = EditorGUI.Popup(popupRect, index, names);
                typeNameProp.stringValue = names[index];
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18f + 18f + 4f;
        }
    }
}