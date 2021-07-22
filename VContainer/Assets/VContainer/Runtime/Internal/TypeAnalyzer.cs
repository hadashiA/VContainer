using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace VContainer.Internal
{
    sealed class InjectConstructorInfo
    {
        public readonly ConstructorInfo ConstructorInfo;
        public readonly ParameterInfo[] ParameterInfos;

        public InjectConstructorInfo(ConstructorInfo constructorInfo)
        {
            ConstructorInfo = constructorInfo;
            ParameterInfos = constructorInfo.GetParameters();
        }

        public InjectConstructorInfo(ConstructorInfo constructorInfo, ParameterInfo[] parameterInfos)
        {
            ConstructorInfo = constructorInfo;
            ParameterInfos = parameterInfos;
        }
    }

    sealed class InjectMethodInfo
    {
        public readonly MethodInfo MethodInfo;
        public readonly ParameterInfo[] ParameterInfos;

        public InjectMethodInfo(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            ParameterInfos = methodInfo.GetParameters();
        }
    }

    // sealed class InjectFieldInfo
    // {
    //     public readonly Type FieldType;
    //     public readonly Action<object, object> Setter;
    //
    //     public InjectFieldInfo(FieldInfo fieldInfo)
    //     {
    //         FieldType = fieldInfo.FieldType;
    //         Setter = fieldInfo.SetValue;
    //     }
    // }
    //
    // sealed class InjectPropertyInfo
    // {
    //     public readonly Type PropertyType;
    //     public readonly Action<object, object> Setter;
    //
    //     public InjectPropertyInfo(PropertyInfo propertyInfo)
    //     {
    //         PropertyType = propertyInfo.PropertyType;
    //         Setter = propertyInfo.SetValue;
    //     }
    // }

    sealed class InjectTypeInfo
    {
        public readonly Type Type;
        public readonly InjectConstructorInfo InjectConstructor;
        public readonly IReadOnlyList<InjectMethodInfo> InjectMethods;
        public readonly IReadOnlyList<FieldInfo> InjectFields;
        public readonly IReadOnlyList<PropertyInfo> InjectProperties;

        public InjectTypeInfo(
            Type type,
            InjectConstructorInfo injectConstructor,
            IReadOnlyList<InjectMethodInfo> injectMethods,
            IReadOnlyList<FieldInfo> injectFields,
            IReadOnlyList<PropertyInfo> injectProperties)
        {
            Type = type;
            InjectConstructor = injectConstructor;
            InjectFields = injectFields;
            InjectProperties = injectProperties;
            InjectMethods = injectMethods;
        }
    }

    static class TypeAnalyzer
    {
        public static InjectTypeInfo AnalyzeWithCache(Type type) => Cache.GetOrAdd(type, Analyze);

        static readonly ConcurrentDictionary<Type, InjectTypeInfo> Cache = new ConcurrentDictionary<Type, InjectTypeInfo>();

        [ThreadStatic]
        static Stack<Type> circularDependencyChecker = new Stack<Type>(128);

        public static InjectTypeInfo Analyze(Type type)
        {
            var injectConstructor = default(InjectConstructorInfo);
            var typeInfo = type.GetTypeInfo();

            // Constructor, single [Inject] constructor -> single most parameters constuctor
            var annotatedConstructorCount = 0;
            var maxParameters = -1;
            foreach (var constructorInfo in typeInfo.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (constructorInfo.IsDefined(typeof(InjectAttribute), false))
                {
                    if (++annotatedConstructorCount > 1)
                    {
                        throw new VContainerException(type, $"Type found multiple [Inject] marked constructors, type: {type.Name}");
                    }
                    injectConstructor = new InjectConstructorInfo(constructorInfo);
                }
                else if (annotatedConstructorCount <= 0)
                {
                    var parameterInfos = constructorInfo.GetParameters();
                    if (parameterInfos.Length > maxParameters)
                    {
                        injectConstructor = new InjectConstructorInfo(constructorInfo, parameterInfos);
                        maxParameters = parameterInfos.Length;
                    }
                }
            }

            if (injectConstructor == null)
            {
                var allowNoConstructor = type.IsEnum;
#if UNITY_2018_4_OR_NEWER
                // It seems that Unity sometimes strips the constructor of Component at build time.
                // In that case, allow null.
                allowNoConstructor |= type.IsSubclassOf(typeof(UnityEngine.Component));
#endif
                if (!allowNoConstructor)
                    throw new VContainerException(type, $"Type does not found injectable constructor, type: {type.Name}");
            }
            // Methods, [Inject] Only
            var injectMethods = default(List<InjectMethodInfo>);
            foreach (var methodInfo in type.GetRuntimeMethods())
            {
                if (methodInfo.IsDefined(typeof(InjectAttribute), true))
                {
                    if (injectMethods == null)
                        injectMethods = new List<InjectMethodInfo>();
                    injectMethods.Add(new InjectMethodInfo(methodInfo));
                }
            }

            // Fields, [Inject] Only
            var injectFields = default(List<FieldInfo>);
            foreach (var fieldInfo in type.GetRuntimeFields())
            {
                if (fieldInfo.IsDefined(typeof(InjectAttribute), true))
                {
                    if (injectFields == null)
                        injectFields = new List<FieldInfo>();
                    injectFields.Add(fieldInfo);
                }
            }

            // Properties, [Inject] only
            var injectProperties = default(List<PropertyInfo>);
            foreach (var propertyInfo in type.GetRuntimeProperties())
            {
                if (propertyInfo.IsDefined(typeof(InjectAttribute), true))
                {
                    if (injectProperties == null)
                        injectProperties = new List<PropertyInfo>();
                    injectProperties.Add(propertyInfo);
                }
            }

            return new InjectTypeInfo(
                type,
                injectConstructor,
                injectMethods,
                injectFields,
                injectProperties);
        }

        public static void CheckCircularDependency(Type type)
        {
            // ThreadStatic
            if (circularDependencyChecker == null)
                circularDependencyChecker = new Stack<Type>(128);
            circularDependencyChecker.Clear();
            CheckCircularDependencyRecursive(type, circularDependencyChecker);
        }

        static void CheckCircularDependencyRecursive(Type type, Stack<Type> stack)
        {
            if (stack.Contains(type))
            {
                throw new VContainerException(type, $"Circular dependency detected! type: {type.FullName}");
            }
            stack.Push(type);
            if (Cache.TryGetValue(type, out var injectTypeInfo))
            {
                if (injectTypeInfo.InjectConstructor != null)
                {
                    foreach (var x in injectTypeInfo.InjectConstructor.ParameterInfos)
                    {
                        CheckCircularDependencyRecursive(x.ParameterType, stack);
                    }
                }

                if (injectTypeInfo.InjectMethods != null)
                {
                    foreach (var methodInfo in injectTypeInfo.InjectMethods)
                    {
                        foreach (var x in methodInfo.ParameterInfos)
                        {
                            CheckCircularDependencyRecursive(x.ParameterType, stack);
                        }
                    }
                }

                if (injectTypeInfo.InjectFields != null)
                {
                    foreach (var x in injectTypeInfo.InjectFields)
                    {
                        CheckCircularDependencyRecursive(x.FieldType, stack);
                    }
                }

                if (injectTypeInfo.InjectProperties != null)
                {
                    foreach (var x in injectTypeInfo.InjectProperties)
                    {
                        CheckCircularDependencyRecursive(x.PropertyType, stack);
                    }
                }
            }
            stack.Pop();
        }
    }
}
