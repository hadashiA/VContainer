using System;
using System.Linq;
using System.Reflection;

namespace VContainer.Internal
{
    // static class InjectTypeInfoCache<T>
    // {
    //     static InjectTypeInfoCache()
    //     {
    //         Value = TypeAnalyzer.Analyze(typeof(T), false);
    //     }
    //
    //     public static readonly InjectTypeInfo Value;
    // }
    //
    // static class InjectTypeInfoCacheWithoutConstructor<T>
    // {
    //     static InjectTypeInfoCacheWithoutConstructor()
    //     {
    //         Value = TypeAnalyzer.Analyze(typeof(T), true);
    //     }
    //
    //     public static readonly InjectTypeInfo Value;
    // }

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

    sealed class InjectFieldInfo
    {
        public readonly FieldInfo FieldInfo;

        public InjectFieldInfo(FieldInfo fieldInfo)
        {
            FieldInfo = fieldInfo;
        }
    }

    sealed class InjectPropertyInfo
    {
        public readonly PropertyInfo PropertyInfo;
        // public readonly MethodInfo Getter;
        // public readonly MethodInfo Setter;

        public InjectPropertyInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            // Getter = propertyInfo.GetMethod;
            // Setter = propertyInfo.SetMethod;
        }
    }

    sealed class InjectTypeInfo
    {
        public readonly Type Type;
        public readonly InjectConstructorInfo InjectConstructor;
        public readonly InjectFieldInfo[] InjectFields;
        public readonly InjectPropertyInfo[] InjectProperties;
        public readonly InjectMethodInfo[] InjectMethods;

        public InjectTypeInfo(
            Type type,
            InjectConstructorInfo injectConstructor,
            InjectFieldInfo[] injectFields,
            InjectPropertyInfo[] injectProperties,
            InjectMethodInfo[] injectMethods)
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
        public static InjectTypeInfo Analyze(Type type, bool skipConstructor = false)
        {
            var injectConstructor = default(ConstructorInfo);
            var typeInfo = type.GetTypeInfo();

            if (!skipConstructor)
            {
                // Constructor, single [Inject] constructor -> single most parameters constuctor
                var injectConstructors = typeInfo.DeclaredConstructors.Where(x => x.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
                if (injectConstructors.Length == 0)
                {
                    var grouped = typeInfo.DeclaredConstructors.Where(x => !x.IsStatic).GroupBy(x => x.GetParameters().Length).OrderByDescending(x => x.Key).FirstOrDefault();
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
            }

            // Methods, [Inject] Only
            var injectMethods = type.GetRuntimeMethods()
                .Where(x => x.GetCustomAttribute<InjectAttribute>(true) != null)
                .Select(x => new InjectMethodInfo(x))
                .ToArray();

            // Fields, [Inject] Only
            var injectFields = type.GetRuntimeFields()
                .Where(x => x.GetCustomAttribute<InjectAttribute>(true) != null)
                .Select(x => new InjectFieldInfo(x))
                .ToArray();

            // Properties, [Inject] only
            var injectProperties = type.GetRuntimeProperties()
                .Where(x => x.SetMethod != null && x.GetCustomAttribute<InjectAttribute>(true) != null)
                .Select(x => new InjectPropertyInfo(x))
                .ToArray();

            return new InjectTypeInfo(
                type,
                injectConstructor != null ? new InjectConstructorInfo(injectConstructor) : null,
                injectFields,
                injectProperties,
                injectMethods);
        }
    }
}
