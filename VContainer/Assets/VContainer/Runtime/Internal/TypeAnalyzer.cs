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

    readonly struct DependencyInfo
    {
        public Type ImplementationType => Dependency.ImplementationType;
        public IInstanceProvider Provider => Dependency.Provider;

        public readonly Registration Dependency;
        readonly Registration owner;
        readonly object method; // ctor or method
        readonly ParameterInfo param; // param or field or prop

        public DependencyInfo(Registration dependency)
        {
            Dependency = dependency;
            owner = null;
            method = null;
            param = null;
        }

        public DependencyInfo(Registration dependency, Registration owner, ConstructorInfo ctor, ParameterInfo param)
        {
            Dependency = dependency;
            this.owner = owner;
            method = ctor;
            this.param = param;
        }

        public DependencyInfo(Registration dependency, Registration owner, MethodInfo method, ParameterInfo param)
        {
            Dependency = dependency;
            this.owner = owner;
            this.method = method;
            this.param = param;
        }

        public DependencyInfo(Registration dependency, Registration owner, FieldInfo field)
        {
            Dependency = dependency;
            this.owner = owner;
            method = field;
            param = null;
        }

        public DependencyInfo(Registration dependency, Registration owner, PropertyInfo prop)
        {
            Dependency = dependency;
            this.owner = owner;
            method = prop;
            param = null;
        }

        public override string ToString()
        {
            switch (method)
            {
                case ConstructorInfo _:
                    return $"{owner.ImplementationType}..ctor({param.Name})";
                case MethodInfo methodInfo:
                    return $"{owner.ImplementationType.FullName}.{methodInfo.Name}({param.Name})";
                case FieldInfo field:
                    return $"{owner.ImplementationType.FullName}.{field.Name}";
                case PropertyInfo prop:
                    return $"{owner.ImplementationType.FullName}.{prop.Name}";
                default:
                    return "";
            }
        }
    }

    static class TypeAnalyzer
    {
        public static InjectTypeInfo AnalyzeWithCache(Type type) => Cache.GetOrAdd(type, AnalyzeFunc);

        static readonly ConcurrentDictionary<Type, InjectTypeInfo> Cache = new ConcurrentDictionary<Type, InjectTypeInfo>();

        [ThreadStatic]
        static Stack<DependencyInfo> circularDependencyChecker;

        static readonly Func<Type, InjectTypeInfo> AnalyzeFunc = Analyze;

        public static InjectTypeInfo Analyze(Type type)
        {
            var injectConstructor = default(InjectConstructorInfo);
            var analyzedType = type;
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

            var injectMethods = default(List<InjectMethodInfo>);
            var injectFields = default(List<FieldInfo>);
            var injectProperties = default(List<PropertyInfo>);
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            while (type != null && type != typeof(object))
            {
                // Methods, [Inject] Only
                var methods = type.GetMethods(bindingFlags);
                foreach (var methodInfo in methods)
                {
                    if (methodInfo.IsDefined(typeof(InjectAttribute), false))
                    {
                        if (injectMethods == null)
                        {
                            injectMethods = new List<InjectMethodInfo>();
                        }
                        else
                        {
                            // Skip if already exists
                            foreach (var x in injectMethods)
                            {
                                if (x.MethodInfo.GetBaseDefinition() == methodInfo.GetBaseDefinition())
                                    goto EndMethod;
                            }
                        }

                        injectMethods.Add(new InjectMethodInfo(methodInfo));
                    }
                }
                EndMethod:

                // Fields, [Inject] Only
                var fields = type.GetFields(bindingFlags);
                foreach (var fieldInfo in fields)
                {
                    if (fieldInfo.IsDefined(typeof(InjectAttribute), false))
                    {
                        if (injectFields == null)
                        {
                            injectFields = new List<FieldInfo>();
                        }
                        else
                        {
                            // Skip if already exists
                            foreach (var x in injectFields)
                            {
                                if (x.Name == fieldInfo.Name)
                                    goto EndField;
                            }

                            if (injectFields.Any(x => x.Name == fieldInfo.Name))
                            {
                                continue;
                            }
                        }
                        injectFields.Add(fieldInfo);
                    }
                }
                EndField:

                // Properties, [Inject] only
                var props = type.GetProperties(bindingFlags);
                foreach (var propertyInfo in props)
                {
                    if (propertyInfo.IsDefined(typeof(InjectAttribute), false))
                    {
                        if (injectProperties == null)
                        {
                            injectProperties = new List<PropertyInfo>();
                        }
                        else
                        {
                            foreach (var x in injectProperties)
                            {
                                if (x.Name == propertyInfo.Name)
                                    goto EndProperty;
                            }
                        }
                        injectProperties.Add(propertyInfo);
                    }
                }
                EndProperty:

                type = type.BaseType;
            }

            return new InjectTypeInfo(
                analyzedType,
                injectConstructor,
                injectMethods,
                injectFields,
                injectProperties);
        }

        public static void CheckCircularDependency(IReadOnlyList<Registration> registrations, Registry registry)
        {
            // ThreadStatic
            if (circularDependencyChecker == null)
                circularDependencyChecker = new Stack<DependencyInfo>();

            for (var i = 0; i < registrations.Count; i++)
            {
                circularDependencyChecker.Clear();
                CheckCircularDependencyRecursive(new DependencyInfo(registrations[i]), registry, circularDependencyChecker);
            }
        }

        static void CheckCircularDependencyRecursive(DependencyInfo current, Registry registry, Stack<DependencyInfo> stack)
        {
            var i = 0;
            foreach (var dependency in stack)
            {
                if (current.ImplementationType == dependency.ImplementationType)
                {
                    // When instantiated by Func, the abstract type cycle is user-avoidable.
                    if (current.Dependency.Provider is FuncInstanceProvider)
                    {
                        return;
                    }

                    stack.Push(current);

                    var path = string.Join("\n",
                        stack.Take(i + 1)
                            .Reverse()
                            .Select((item, itemIndex) => $"    [{itemIndex + 1}] {item} --> {item.ImplementationType.FullName}"));
                    throw new VContainerException(current.Dependency.ImplementationType,
                        $"Circular dependency detected!\n{path}");
                }
                i++;
            }

            stack.Push(current);

            if (Cache.TryGetValue(current.ImplementationType, out var injectTypeInfo))
            {
                if (injectTypeInfo.InjectConstructor != null)
                {
                    foreach (var x in injectTypeInfo.InjectConstructor.ParameterInfos)
                    {
                        if (registry.TryGet(x.ParameterType, out var parameterRegistration))
                        {
                            CheckCircularDependencyRecursive(new DependencyInfo(parameterRegistration, current.Dependency, injectTypeInfo.InjectConstructor.ConstructorInfo, x), registry, stack);
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
                                CheckCircularDependencyRecursive(new DependencyInfo(parameterRegistration, current.Dependency, methodInfo.MethodInfo, x), registry, stack);
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
                            CheckCircularDependencyRecursive(new DependencyInfo(fieldRegistration, current.Dependency, x), registry, stack);
                        }
                    }
                }

                if (injectTypeInfo.InjectProperties != null)
                {
                    foreach (var x in injectTypeInfo.InjectProperties)
                    {
                        if (registry.TryGet(x.PropertyType, out var propertyRegistration))
                        {
                            CheckCircularDependencyRecursive(new DependencyInfo(propertyRegistration, current.Dependency, x), registry, stack);
                        }
                    }
                }
            }

            stack.Pop();
        }
    }
}
