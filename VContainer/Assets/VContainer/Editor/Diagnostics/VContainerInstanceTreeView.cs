using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor.IMGUI.Controls;
using VContainer.Diagnostics;

namespace VContainer.Editor.Diagnostics
{
    public sealed class VContainerInstanceTreeView : TreeView
    {
        static readonly Dictionary<Type, string> TypeAlias = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(void), "void" }
        };

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
                    var item = new TreeViewItem(NextId(), 0, $"({GetTypeAlias(instance.GetType())}) {instance}");
                    children.Add(item);
                    SetExpanded(item.id, true);
                    AddProperties(instance, item);
                }
            }

            root.children = children;
            return root;
        }

        void AddProperties(object instance, TreeViewItem parent)
        {
            var type = instance.GetType();
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var prop in props)
            {
                var value = prop.GetValue(instance);
                parent.AddChild(new TreeViewItem(
                    NextId(),
                    parent.depth + 1,
                    $"{prop.Name} = ({GetTypeAlias(prop.PropertyType)}) {value}"));
            }

            foreach (var field in fields)
            {
                if (field.IsDefined(typeof(CompilerGeneratedAttribute)))
                {
                    continue;
                }

                var value = field.GetValue(instance);
                parent.AddChild(new TreeViewItem(
                    NextId(),
                    parent.depth + 1,
                    $"{field.Name} = ({GetTypeAlias(field.FieldType)}) {value}"));
            }
        }

        string GetTypeAlias(Type type)
        {
            if (TypeAlias.TryGetValue(type, out var alias))
            {
                return alias;
            }
            return type.Name;
        }
    }
}
