using System;

namespace VContainer.Internal
{
    sealed class ReflectionInjector : IInjector
    {
        readonly InjectTypeInfo injectTypeInfo;

        public ReflectionInjector(InjectTypeInfo injectTypeInfo)
        {
            this.injectTypeInfo = injectTypeInfo;
        }

        public void Inject(object instance, IObjectResolver resolver)
        {
            InjectMethods(instance, resolver);
            InjectProperties(instance, resolver);
            InjectFields(instance, resolver);
        }

        public object CreateInstance(IObjectResolver resolver)
        {
            var parameters = injectTypeInfo.InjectConstructor.GetParameters();
            var parameterValues = FixedArrayPool<object>.Shared8.Rent(parameters.Length);
            for (var i = 0; i < parameters.Length; i++)
            {
                parameterValues[i] = resolver.Resolve(parameters[i].ParameterType);
            }

            var instance = injectTypeInfo.InjectConstructor.Invoke(parameterValues);
            Inject(instance, resolver);

            FixedArrayPool<object>.Shared8.Return(parameterValues);
            return instance;
        }

        void InjectFields(object obj, IObjectResolver resolver)
        {
            foreach (var f in injectTypeInfo.InjectFields)
            {
                var fieldValue = resolver.Resolve(f.FieldType);
                f.SetValue(obj, fieldValue);
            }
        }

        void InjectProperties(object obj, IObjectResolver resolver)
        {
            foreach (var prop in injectTypeInfo.InjectProperties)
            {
                var propValue = resolver.Resolve(prop.PropertyType);
                prop.SetValue(obj, propValue);
            }
        }

        void InjectMethods(object obj, IObjectResolver resolver)
        {
            // methods
            foreach (var method in injectTypeInfo.InjectMethods)
            {
                var parameters = method.GetParameters();
                var parameterValues = new object[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    parameterValues[i] = resolver.Resolve(parameters[i].ParameterType);
                }

                method.Invoke(obj, parameterValues);
            }
        }
    }

    sealed class ReflectionInjectorBuilder : IInjectorBuilder
    {
        public static ReflectionInjectorBuilder Default = new ReflectionInjectorBuilder();

        public IInjector Build(Type type)
        {
            var injectTypeInfo = TypeAnalyzer.Analyze(type);
            return new ReflectionInjector(injectTypeInfo);
        }
    }
}