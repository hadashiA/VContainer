using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VContainer.Diagnostics;

namespace VContainer.Editor.Diagnostics
{
    public sealed class VContainerInstanceTreeView : TreeView
    {
        static int idSeed;
        static int NextId() => ++idSeed;

        public DiagnosticsInfo CurrentDiagnosticsInfo { get; set; }

        public VContainerInstanceTreeView() : base(new TreeViewState())
        {
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = NextId(), depth = -1, displayName = "Root" };
            var children = new List<TreeViewItem>();
            if (CurrentDiagnosticsInfo is DiagnosticsInfo info)
            {
                for (var i = 0; i < info.ResolveInfo.Instances.Count; i++)
                {
                    var instance = info.ResolveInfo.Instances[i];

                    var displayName = TypeNameHelper.IsNullOrDestroyed(instance)
                        ? "null or destroyed"
                        : $"({TypeNameHelper.GetTypeAlias(instance.GetType())}) {instance}";

                    var item = new TreeViewItem(NextId(), 0, displayName);
                    AddProperties(instance, item);
                    children.Add(item);
                    SetExpanded(item.id, true);
                }
            }

            root.children = children;
            return root;
        }

        void AddProperties(object instance, TreeViewItem parent)
        {
            if (TypeNameHelper.IsNullOrDestroyed(instance))
                return;

            var type = instance.GetType();
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var prop in props)
            {
                if (prop.PropertyType.IsSubclassOf(typeof(UnityEngine.Object)) &&
                    prop.IsDefined(typeof(ObsoleteAttribute), true))
                {
                    continue;
                }

                try
                {
                    var value = prop.GetValue(instance);
                    var displayName = TypeNameHelper.IsNullOrDestroyed(value)
                        ? "null or destroyed"
                        : $"{prop.Name} = ({TypeNameHelper.GetTypeAlias(prop.PropertyType)}) {value}";
                    parent.AddChild(new TreeViewItem(NextId(), parent.depth + 1, displayName));
                }
                catch (MissingReferenceException)
                {
                }
                catch (NotSupportedException)
                {
                }
            }

            foreach (var field in fields)
            {
                if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) &&
                    field.IsDefined(typeof(ObsoleteAttribute), true) &&
                    field.IsDefined(typeof(CompilerGeneratedAttribute), true))
                {
                    continue;
                }

                try
                {
                    var value = field.GetValue(instance);
                    var displayName = TypeNameHelper.IsNullOrDestroyed(value)
                        ? "null or destroyed"
                        : $"{field.Name} = ({TypeNameHelper.GetTypeAlias(field.FieldType)}) {value}";
                    parent.AddChild(new TreeViewItem(NextId(), parent.depth + 1, displayName));
                }
                catch (MissingReferenceException)
                {
                }
                catch (NotSupportedException)
                {
                }
            }
        }
    }
}
