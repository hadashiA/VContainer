using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

    readonly struct DependencyNode
    {
        readonly Registration registration;
        readonly object method; // ctor or method
        readonly object param; // param or field or prop

        public DependencyNode(Registration registration, InjectConstructorInfo ctor, ParameterInfo param)
        {
            this.registration = registration;
            this.method = ctor;
            this.param = param;
        }

        public DependencyNode(Registration registration, InjectMethodInfo method, ParameterInfo param)
        {
            this.registration = registration;
            this.method = method;
            this.param = param;
        }

        public DependencyNode(Registration registration, FieldInfo field)
        {
            this.registration = registration;
            this.method = null;
            this.param = field;
        }

        public DependencyNode(Registration registration, PropertyInfo prop)
        {
            this.registration = registration;
            this.method = null;
            this.param = prop;
        }

        public override string ToString()
        {
            if (this.method is InjectConstructorInfo ctor)
            {
                var param = (ParameterInfo)this.param;
                return $"{registration.ImplementationType.FullName}..ctor({param.Name})";
            }
            else if (this.method is InjectMethodInfo method)
            {
                var param = (ParameterInfo)this.param;
                return $"{registration.ImplementationType.FullName}.{method.MethodInfo.Name}({param.Name})";
            }
            else if (this.param is FieldInfo field)
            {
                return $"{registration.ImplementationType.FullName}.{field.Name}";
            }
            else if (this.param is PropertyInfo prop)
            {
                return $"{registration.ImplementationType.FullName}.{prop.Name}";
            }
            else
            {
                return "";
            }
        }
    }

    static class TypeAnalyzer
    {
        public static InjectTypeInfo AnalyzeWithCache(Type type) => Cache.GetOrAdd(type, AnalyzeFunc);

        static readonly ConcurrentDictionary<Type, InjectTypeInfo> Cache = new ConcurrentDictionary<Type, InjectTypeInfo>();

        [ThreadStatic]
        static Stack<(DependencyNode Node, Registration Registration)> circularDependencyChecker;

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

        public static void CheckCircularDependency(IReadOnlyList<Registration> registrations, Registry registry)
        {
            // ThreadStatic
            if (circularDependencyChecker == null)
                circularDependencyChecker = new Stack<(DependencyNode Node, Registration Registration)>();

            for (var i = 0; i < registrations.Count; i++)
            {
                circularDependencyChecker.Clear();
                CheckCircularDependencyRecursive(default, registrations[i], registry, circularDependencyChecker);
            }
        }

        static void CheckCircularDependencyRecursive(DependencyNode node, Registration registration, Registry registry, Stack<(DependencyNode Node, Registration Registration)> stack)
        {
            var i = 0;
            foreach (var x in stack)
            {
                if (registration.ImplementationType == x.Registration.ImplementationType)
                {
                    stack.Push((node, registration));
                    var path = string.Join("\n",
                        stack.Take(i + 1)
                            .Reverse()
                            .Select((item, itemIndex) => $"    [{itemIndex + 1}] {item.Node} --> {item.Registration.ImplementationType.FullName}"));
                    throw new VContainerException(registration.ImplementationType,
                        $"Circular dependency detected!\n{path}");
                }
                i++;
            }

            stack.Push((node, registration));

            if (Cache.TryGetValue(registration.ImplementationType, out var injectTypeInfo))
            {
                if (injectTypeInfo.InjectConstructor != null)
                {
                    foreach (var x in injectTypeInfo.InjectConstructor.ParameterInfos)
                    {
                        if (registry.TryGet(x.ParameterType, out var parameterRegistration))
                        {
                            CheckCircularDependencyRecursive(new DependencyNode(registration, injectTypeInfo.InjectConstructor, x), parameterRegistration, registry, stack);
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
                                CheckCircularDependencyRecursive(new DependencyNode(registration, methodInfo, x), parameterRegistration, registry, stack);
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
                            CheckCircularDependencyRecursive(new DependencyNode(registration, x),fieldRegistration, registry, stack);
                        }
                    }
                }

                if (injectTypeInfo.InjectProperties != null)
                {
                    foreach (var x in injectTypeInfo.InjectProperties)
                    {
                        if (registry.TryGet(x.PropertyType, out var propertyRegistration))
                        {
                            CheckCircularDependencyRecursive(new DependencyNode(registration, x), propertyRegistration, registry, stack);
                        }
                    }
                }
            }

            stack.Pop();
        }
    }
}
