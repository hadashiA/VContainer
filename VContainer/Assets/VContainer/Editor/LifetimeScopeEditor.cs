using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VContainer.Unity;

namespace VContainer.Editor
{
    [CustomEditor(typeof(LifetimeScope))]
    public sealed class LifetimeScopeEditor : UnityEditor.Editor
    {
        enum ParentMode
        {
            None,
            ObjectReference,
            KeyReference
            //AutoReference,
        }

        ParentMode? parentMode;
        ReorderableList monoInstallerList;
        ReorderableList scriptableObjectInstallerList;

        void OnEnable()
        {
            InitMonoInstallerList();
            InitScriptableObjectInstallerList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                var keyProp = serializedObject.FindProperty("key");
                EditorGUILayout.PropertyField(keyProp);

                var autoBuildProp = serializedObject.FindProperty("buildOnAwake");
                EditorGUILayout.PropertyField(autoBuildProp);

                var parentProp = serializedObject.FindProperty("parent");
                var parentKeyProp = serializedObject.FindProperty("parentKey");

                if (parentMode == null)
                {
                    parentMode = ParentMode.None;
                    if (parentProp.objectReferenceValue != null)
                    {
                        parentMode = ParentMode.ObjectReference;
                    }
                    else if (!string.IsNullOrEmpty(parentKeyProp.stringValue) &&
                             parentKeyProp.stringValue != LifetimeScope.AutoReferenceKey)
                    {
                        parentMode = ParentMode.KeyReference;
                    }
                }

                parentMode = (ParentMode)EditorGUILayout.EnumPopup("Parent", parentMode.Value);
                switch (parentMode)
                {
                    case ParentMode.None:
                        parentProp.objectReferenceValue = null;
                        parentKeyProp.stringValue = "";
                        break;
                    case ParentMode.ObjectReference:
                        EditorGUILayout.PropertyField(parentProp);
                        parentKeyProp.stringValue = "";
                        break;
                    case ParentMode.KeyReference:
                        parentProp.objectReferenceValue = null;
                        if (parentKeyProp.stringValue == LifetimeScope.AutoReferenceKey)
                        {
                            parentKeyProp.stringValue = "";
                        }
                        EditorGUILayout.PropertyField(parentKeyProp);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                EditorGUILayout.Space(10);

                monoInstallerList.DoLayoutList();
                scriptableObjectInstallerList.DoLayoutList();

                if (scope.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (GUILayout.Button("Validate"))
            {
                Validate();
            }
        }

        void InitMonoInstallerList()
        {
            monoInstallerList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("monoInstallers"),
                true,
                true,
                true,
                true);

            monoInstallerList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Mono Installers");
            };

            monoInstallerList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = monoInstallerList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2, rect.width * 0.7f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };

            monoInstallerList.onSelectCallback = list =>
            {
                var element = list.serializedProperty
                    .GetArrayElementAtIndex(list.index)
                    .objectReferenceValue;

                if (element is MonoInstaller installer)
                {
                    EditorGUIUtility.PingObject(installer.gameObject);
                }
            };

            monoInstallerList.onRemoveCallback = list =>
            {
                var element = monoInstallerList.serializedProperty.GetArrayElementAtIndex(list.index);
                var monoInstaller = element.objectReferenceValue as MonoInstaller;
                if (monoInstaller != null)
                {
                    var instanceId = monoInstaller.GetInstanceID();
                    var attaches = ((LifetimeScope)target).gameObject.GetComponents(monoInstaller.GetType());
                    foreach (var attached in attaches)
                    {
                        if (attached.GetInstanceID() == instanceId)
                        {
                            DestroyImmediate(attached);
                        }
                    }
                }
                element.objectReferenceValue = null;
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            };

            monoInstallerList.onAddDropdownCallback = (buttonRect, list) =>
            {
                var menu = new GenericMenu();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsSubclassOf(typeof(MonoInstaller)))
                        {
                            var label = string.IsNullOrEmpty(type.Namespace) ? type.Name : $"{type.Namespace}/{type.Name}";
                            menu.AddItem(new GUIContent(label), false, HandleAddMonoInstaller, type);
                        }
                    }
                }
                menu.ShowAsContext();
            };

            void HandleAddMonoInstaller(object arg)
            {
                var type = (Type)arg;
                var tail = monoInstallerList.serializedProperty.arraySize++;
                monoInstallerList.index = tail;

                var newMonoInstaller = ((LifetimeScope)target).gameObject.AddComponent(type);
                var element = monoInstallerList.serializedProperty.GetArrayElementAtIndex(tail);
                element.objectReferenceValue = newMonoInstaller;
                serializedObject.ApplyModifiedProperties();
            }
        }

        void InitScriptableObjectInstallerList()
        {
            scriptableObjectInstallerList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("scriptableObjectInstallers"),
                true,
                true,
                true,
                true);

            scriptableObjectInstallerList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "ScriptableObject Installers");
            };

            scriptableObjectInstallerList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = scriptableObjectInstallerList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2, rect.width * 0.7f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };

            scriptableObjectInstallerList.onSelectCallback = list =>
            {
                var element = list.serializedProperty
                    .GetArrayElementAtIndex(list.index)
                    .objectReferenceValue;

                if (element is ScriptableObject installer)
                {
                    EditorGUIUtility.PingObject(installer);
                }
            };

            scriptableObjectInstallerList.onRemoveCallback = list =>
            {
                var element = scriptableObjectInstallerList.serializedProperty.GetArrayElementAtIndex(list.index);
                element.objectReferenceValue = null;
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            };

            scriptableObjectInstallerList.onAddDropdownCallback = (buttonRect, list) =>
            {
                var menu = new GenericMenu();
                var guids = AssetDatabase.FindAssets("t:ScriptableObjectInstaller");
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var label = assetPath;
                    if (label.StartsWith("Assets/"))
                    {
                        label = label.Substring("Assets/".Length);
                    }

                    menu.AddItem(new GUIContent(label), false, HandleAddScriptableObjectInstaller, assetPath);
                }
                menu.ShowAsContext();
            };

            void HandleAddScriptableObjectInstaller(object arg)
            {
                var assetPath = (string)arg;
                var tail = scriptableObjectInstallerList.serializedProperty.arraySize++;
                scriptableObjectInstallerList.index = tail;

                var element = scriptableObjectInstallerList.serializedProperty.GetArrayElementAtIndex(tail);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObjectInstaller>(assetPath);
                element.objectReferenceValue = asset;
                serializedObject.ApplyModifiedProperties();
            }
        }

        void Validate()
        {
            var containerBuilder = new UnityContainerBuilder(((LifetimeScope)target).gameObject.scene);

            for (var i = 0; i < monoInstallerList.count; i++)
            {
                var element = monoInstallerList.serializedProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue is IInstaller monoInstaller)
                {
                    monoInstaller.Install(containerBuilder);
                }
            }

            for (var i = 0; i < scriptableObjectInstallerList.count; i++)
            {
                var element = scriptableObjectInstallerList.serializedProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue is IInstaller monoInstaller)
                {
                    monoInstaller.Install(containerBuilder);
                }
            }

            var _ = containerBuilder.Build();
            UnityEngine.Debug.Log($"<color=green>[VContainer] Validation success {target.name}</color>");
            EditorUtility.DisplayDialog("LifetimeScope validation", "Success", "OK");
        }
    }
}