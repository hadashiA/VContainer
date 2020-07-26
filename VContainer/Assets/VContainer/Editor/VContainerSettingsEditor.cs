using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VContainer.Unity;

namespace VContainer.Editor
{
    [CustomEditor(typeof(VContainerSettings))]
    public class VContainerSettingsEditor : UnityEditor.Editor
    {
        ReorderableList codeGenAssemblyNameList;
        ReorderableList codeGenNamespaceList;
        bool expandCodeGen;

        void OnEnable()
        {
            InitCodeGenAssemblyNameList();
            InitCodegenNamespaceList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                var rootLifetimeScopeProp = serializedObject.FindProperty("RootLifetimeScope");
                EditorGUILayout.PropertyField(rootLifetimeScopeProp);

                expandCodeGen = EditorGUILayout.Foldout(expandCodeGen, "Prepare Code Generation", true);
                if (expandCodeGen)
                {
                    var codeGenEnabledProp = serializedObject.FindProperty("CodeGen.Enabled");
                    EditorGUILayout.PropertyField(codeGenEnabledProp);

                    codeGenAssemblyNameList.DoLayoutList();
                    codeGenNamespaceList.DoLayoutList();
                }

                if (scope.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        void InitCodeGenAssemblyNameList()
        {
            codeGenAssemblyNameList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("CodeGen.AssemblyNames"),
                true,
                true,
                true,
                true);

            codeGenAssemblyNameList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Target Assemblies");
            };

            codeGenAssemblyNameList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = codeGenAssemblyNameList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y + 2, rect.width * 0.7f, EditorGUIUtility.singleLineHeight),
                    element.stringValue);
            };

            codeGenAssemblyNameList.onRemoveCallback = list =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(list.index);
                element.stringValue = null;
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            };

            codeGenAssemblyNameList.onAddDropdownCallback = (buttonRect, list) =>
            {
                var menu = new GenericMenu();
                var assemblies = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .OrderBy(assembly => assembly.GetName().Name);
                foreach (var assembly in assemblies)
                {
                    var label = assembly.GetName().Name.Replace(".", "/");
                    menu.AddItem(new GUIContent(label), false, HandleAddAssemblyName, assembly);
                }
                menu.ShowAsContext();
            };

            void HandleAddAssemblyName(object arg)
            {
                var assembly = (Assembly)arg;
                var tail = codeGenAssemblyNameList.serializedProperty.arraySize++;
                codeGenAssemblyNameList.index = tail;

                var element = codeGenAssemblyNameList.serializedProperty.GetArrayElementAtIndex(tail);
                element.stringValue = assembly.GetName().Name;
                serializedObject.ApplyModifiedProperties();
            }
        }

        void InitCodegenNamespaceList()
        {
            codeGenNamespaceList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("CodeGen.Namespaces"),
                true,
                true,
                true,
                true);

            codeGenNamespaceList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Target Namespace");
            };

            codeGenNamespaceList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = codeGenNamespaceList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y + 2, rect.width * 0.7f, EditorGUIUtility.singleLineHeight),
                    element.stringValue);
            };

            codeGenNamespaceList.onRemoveCallback = list =>
            {
                var element = codeGenNamespaceList.serializedProperty.GetArrayElementAtIndex(list.index);
                element.stringValue = null;
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            };

            codeGenNamespaceList.onAddDropdownCallback = (buttonRect, list) =>
            {
                var menu = new GenericMenu();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var namespaces = assembly.GetTypes()
                        .Select(t => t.Namespace)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .OrderBy(x => x);
                    foreach (var ns in namespaces)
                    {
                        var label = ns.Replace(".", "/");
                        menu.AddItem(new GUIContent(label), false, OnAddNamespace, ns);
                    }
                }
                menu.ShowAsContext();
            };

            void OnAddNamespace(object arg)
            {
                var ns = (string)arg;
                var tail = codeGenNamespaceList.serializedProperty.arraySize++;
                codeGenNamespaceList.index = tail;

                var element = codeGenNamespaceList.serializedProperty.GetArrayElementAtIndex(tail);
                element.stringValue = ns;
                serializedObject.ApplyModifiedProperties();
            }
        }

    }
}