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
    //     public readonly FieldInfo FieldInfo;
    //
    //     public InjectFieldInfo(FieldInfo fieldInfo)
    //     {
    //         FieldInfo = fieldInfo;
    //     }
    // }
    //
    // sealed class InjectPropertyInfo
    // {
    //     public readonly PropertyInfo PropertyInfo;
    //     public readonly bool HasGetter;
    //
    //     public InjectPropertyInfo(PropertyInfo propertyInfo)
    //     {
    //         PropertyInfo = propertyInfo;
    //         HasGetter = propertyInfo.GetMethod != null;
    //     }
    // }

    sealed class InjectTypeInfo
    {
        public readonly Type Type;
        public readonly InjectConstructorInfo InjectConstructor;
        public readonly InjectMethodInfo[] InjectMethods;
        public readonly FieldInfo[] InjectFields;
        public readonly PropertyInfo[] InjectProperties;

        public InjectTypeInfo(
            Type type,
            InjectConstructorInfo injectConstructor,
            InjectMethodInfo[] injectMethods,
            FieldInfo[] injectFields,
            PropertyInfo[] injectProperties)
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
        static readonly ConcurrentDictionary<Type, InjectTypeInfo> Cache = new ConcurrentDictionary<Type, InjectTypeInfo>();

        public static InjectTypeInfo AnalyzeWithCache(Type type) => Cache.GetOrAdd(type, Analyze);

        public static InjectTypeInfo Analyze(Type type)
        {
            var injectConstructor = default(ConstructorInfo);
            var typeInfo = type.GetTypeInfo();

            // Constructor, single [Inject] constructor -> single most parameters constuctor
            var constructors = typeInfo.DeclaredConstructors;
            var injectConstructors = constructors
                .Where(x => x.IsDefined(typeof(InjectAttribute), true))
                .ToArray();
            if (injectConstructors.Length == 0)
            {
                var grouped = constructors
                    .Where(x => !x.IsStatic)
                    .GroupBy(x => x.GetParameters().Length)
                    .OrderByDescending(x => x.Key)
                    .FirstOrDefault();
                if (grouped == null)
                {
                    throw new VContainerException(type, $"Type does not found injectable constructor, type: {type.Name}");
                }
                if (grouped.Count() != 1)
                {
                    throw new VContainerException(type, $"Type found multiple ambiguous constructors, type: {type.Name}");
                }
                injectConstructor = grouped.First();
            }
            else if (injectConstructors.Length == 1)
            {
                injectConstructor = injectConstructors[0];
            }
            else
            {
                throw new VContainerException(type, "Type found multiple [Inject] marked constructors, type:" + type.Name);
            }

            // Methods, [Inject] Only
            var injectMethods = type.GetRuntimeMethods()
                .Where(x => x.IsDefined(typeof(InjectAttribute), true))
                .Select(x => new InjectMethodInfo(x))
                .ToArray();

            // Fields, [Inject] Only
            var injectFields = type.GetRuntimeFields()
                .Where(x => x.IsDefined(typeof(InjectAttribute), true))
                .ToArray();

            // Properties, [Inject] only
            var injectProperties = type.GetRuntimeProperties()
                .Where(x => x.SetMethod != null && x.IsDefined(typeof(InjectAttribute)))
                .ToArray();

            return new InjectTypeInfo(
                type,
                new InjectConstructorInfo(injectConstructor),
                injectMethods,
                injectFields,
                injectProperties);
        }

        public static void CheckCircularDependency(Type type)
        {
            var stack = StackPool<Type>.Default.Rent();
            try
            {
                CheckCircularDependencyRecursive(type, stack);
            }
            finally
            {
                StackPool<Type>.Default.Return(stack);
            }
        }

        static void CheckCircularDependencyRecursive(Type type, Stack<Type> stack)
        {
            if (stack.Any(x => x == type))
            {
                throw new VContainerException(type, $"Circular dependency detected! type: {type.FullName}");
            }
            stack.Push(type);
            if (Cache.TryGetValue(type, out var injectTypeInfo))
            {
                foreach (var x in injectTypeInfo.InjectConstructor.ParameterInfos)
                {
                    CheckCircularDependencyRecursive(x.ParameterType, stack);
                }
                foreach (var methodInfo in injectTypeInfo.InjectMethods)
                {
                    foreach (var x in methodInfo.ParameterInfos)
                    {
                        CheckCircularDependencyRecursive(x.ParameterType, stack);
                    }
                }
                foreach (var x in injectTypeInfo.InjectFields)
                {
                    CheckCircularDependencyRecursive(x.FieldType, stack);
                }
                foreach (var x in injectTypeInfo.InjectProperties)
                {
                    CheckCircularDependencyRecursive(x.PropertyType, stack);
                }
            }
            stack.Pop();
        }
    }
}
