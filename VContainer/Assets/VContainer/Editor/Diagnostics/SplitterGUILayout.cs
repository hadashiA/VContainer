using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VContainer.Editor.Diagnostics
{
    // reflection call of UnityEditor.SplitterGUILayout
    static class SplitterGUILayout
    {
        static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        static readonly Lazy<Type> SplitterStateType = new Lazy<Type>(() =>
        {
            var type = typeof(EditorWindow).Assembly.GetTypes().First(x => x.FullName == "UnityEditor.SplitterState");
            return type;
        });

        static readonly Lazy<ConstructorInfo> SplitterStateCtor = new Lazy<ConstructorInfo>(() =>
        {
            var type = SplitterStateType.Value;
            return type.GetConstructor(flags, null, new Type[] { typeof(float[]), typeof(int[]), typeof(int[]) }, null);
        });

        static readonly Lazy<Type> SplitterGUILayoutType = new Lazy<Type>(() =>
        {
            var type = typeof(EditorWindow).Assembly.GetTypes().First(x => x.FullName == "UnityEditor.SplitterGUILayout");
            return type;
        });

        static readonly Lazy<MethodInfo> BeginVerticalSplitInfo = new Lazy<MethodInfo>(() =>
        {
            var type = SplitterGUILayoutType.Value;
            return type.GetMethod("BeginVerticalSplit", flags, null, new Type[] { SplitterStateType.Value, typeof(GUILayoutOption[]) }, null);
        });

        static readonly Lazy<MethodInfo> EndVerticalSplitInfo = new Lazy<MethodInfo>(() =>
        {
            var type = SplitterGUILayoutType.Value;
            return type.GetMethod("EndVerticalSplit", flags, null, Type.EmptyTypes, null);
        });

        static readonly Lazy<MethodInfo> BeginHorizontalSplitInfo = new Lazy<MethodInfo>(() =>
        {
            var type = SplitterGUILayoutType.Value;
            return type.GetMethod("BeginHorizontalSplit", flags, null, new Type[] { SplitterStateType.Value, typeof(GUILayoutOption[]) }, null);
        });

        static readonly Lazy<MethodInfo> EndHorizontalSplitInfo = new Lazy<MethodInfo>(() =>
        {
            var type = SplitterGUILayoutType.Value;
            return type.GetMethod("EndHorizontalSplit", flags, null, Type.EmptyTypes, null);
        });

        public static object CreateSplitterState(float[] relativeSizes, int[] minSizes, int[] maxSizes)
        {
            return SplitterStateCtor.Value.Invoke(new object[] { relativeSizes, minSizes, maxSizes });
        }

        public static void BeginVerticalSplit(object splitterState, params GUILayoutOption[] options)
        {
            BeginVerticalSplitInfo.Value.Invoke(null, new object[] { splitterState, options });
        }

        public static void EndVerticalSplit()
        {
            EndVerticalSplitInfo.Value.Invoke(null, Array.Empty<object>());
        }

        public static void BeginHorizontalSplit(object splitterState, params GUILayoutOption[] options)
        {
            BeginHorizontalSplitInfo.Value.Invoke(null, new object[] { splitterState, options });
        }

        public static void EndHorizontalSplit()
        {
            EndHorizontalSplitInfo.Value.Invoke(null, Array.Empty<object>());
        }
    }
}