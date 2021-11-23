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
        public static InjectTypeInfo AnalyzeWithCache(Type type) => Cache.GetOrAdd(type, AnalyzeFunc);

        static readonly ConcurrentDictionary<Type, InjectTypeInfo> Cache = new ConcurrentDictionary<Type, InjectTypeInfo>();

        [ThreadStatic]
        static Stack<IRegistration> circularDependencyChecker;

        static Func<Type, InjectTypeInfo> AnalyzeFunc = Analyze;

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

        public static void CheckCircularDependency(IReadOnlyList<IRegistration> registrations, Registry registry)
        {
            // ThreadStatic
            if (circularDependencyChecker == null)
                circularDependencyChecker = new Stack<IRegistration>();

            for (var i = 0; i < registrations.Count; i++)
            {
                circularDependencyChecker.Clear();
                CheckCircularDependencyRecursive(registrations[i], registry, circularDependencyChecker);
            }
        }

        static void CheckCircularDependencyRecursive(IRegistration registration, Registry registry, Stack<IRegistration> stack)
        {
            foreach (var x in stack)
            {
                if (registration.ImplementationType == x.ImplementationType)
                {
                    throw new VContainerException(registration.ImplementationType,
                        $"Circular dependency detected! {registration}");
                }
            }

            stack.Push(registration);

            if (Cache.TryGetValue(registration.ImplementationType, out var injectTypeInfo))
            {
                if (injectTypeInfo.InjectConstructor != null)
                {
                    foreach (var x in injectTypeInfo.InjectConstructor.ParameterInfos)
                    {
                        if (registry.TryGet(x.ParameterType, out var parameterRegistration))
                        {
                            CheckCircularDependencyRecursive(parameterRegistration, registry, stack);
                        }
                    }
                }

                if (injectTypeInfo.InjectMethods != null)
                {
                    foreach (var methodInfo in injectTypeInfo.InjectMethods)
                    {
                        foreach (var x in methodInfo.ParameterInfos)
                        {
                            if (registry.TryGet(x.ParameterType, out var parameterRegistration))
                            {
                                CheckCircularDependencyRecursive(parameterRegistration, registry, stack);
                            }
                        }
                    }
                }

                if (injectTypeInfo.InjectFields != null)
                {
                    foreach (var x in injectTypeInfo.InjectFields)
                    {
                        if (registry.TryGet(x.FieldType, out var fieldRegistration))
                        {
                            CheckCircularDependencyRecursive(fieldRegistration, registry, stack);
                        }
                    }
                }

                if (injectTypeInfo.InjectProperties != null)
                {
                    foreach (var x in injectTypeInfo.InjectProperties)
                    {
                        if (registry.TryGet(x.PropertyType, out var propertyRegistration))
                        {
                            CheckCircularDependencyRecursive(propertyRegistration, registry, stack);
                        }
                    }
                }
            }

            stack.Pop();
        }
    }
}
