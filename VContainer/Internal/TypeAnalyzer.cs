using System;
using System.Linq;
using System.Reflection;

namespace VContainer.Internal
{
    public readonly struct InjectTypeInfo
    {
        public readonly Type Type;
        public readonly TypeInfo TypeInfo;
        public readonly ConstructorInfo InjectConstructor;
        public readonly FieldInfo[] InjectFields;
        public readonly PropertyInfo[] InjectProperties;
        public readonly MethodInfo[] InjectMethods;

        public InjectTypeInfo(
            Type type,
            TypeInfo typeInfo,
            ConstructorInfo injectConstructor,
            FieldInfo[] injectFields,
            PropertyInfo[] injectProperties,
            MethodInfo[] injectMethods)
        {
            Type = type;
            TypeInfo = typeInfo;
            InjectConstructor = injectConstructor;
            InjectFields = injectFields;
            InjectProperties = injectProperties;
            InjectMethods = injectMethods;
        }
    }

    static class TypeAnalyzer
    {
        public static InjectTypeInfo Analyze(Type type)
        {
            var injectConstructor = default(ConstructorInfo);
            var typeInfo = type.GetTypeInfo();

            // Constructor, single [Inject] constructor -> single most parameters constuctor
            var injectConstructors = typeInfo.DeclaredConstructors.Where(x => x.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
            if (injectConstructors.Length == 0)
            {
                var grouped = typeInfo.DeclaredConstructors.Where(x => !x.IsStatic).GroupBy(x => x.GetParameters().Length).OrderByDescending(x => x.Key).FirstOrDefault();
                if (grouped == null)
                {
                    throw new VContainerException($"Type does not found injectable constructor, type: {type.Name}");
                }
                if (grouped.Count() != 1)
                {
                    throw new VContainerException($"Type found multiple ambiguous constructors, type: {type.Name}");
                }
                injectConstructor = grouped.First();
            }
            else if (injectConstructors.Length == 1)
            {
                injectConstructor = injectConstructors[0];
            }
            else
            {
                throw new VContainerException("Type found multiple [Inject] marked constructors, type:" + type.Name);
            }

            // Fields, [Inject] Only
            var injectFields = type.GetRuntimeFields().Where(x => x.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();

            // Properties, [Inject] only
            var injectProperties = type.GetRuntimeProperties().Where(x => x.SetMethod != null && x.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();

            // Methods, [Inject] Only
            var injectMethods = type.GetRuntimeMethods().Where(x => x.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();

            return new InjectTypeInfo(
                type,
                typeInfo,
                injectConstructor,
                injectFields,
                injectProperties,
                injectMethods);
        }
    }
}