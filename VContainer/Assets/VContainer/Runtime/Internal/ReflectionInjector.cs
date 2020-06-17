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
            var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(parameters.Length);
            try
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        parameterValues[i] = resolver.Resolve(parameters[i].ParameterType);
                    }
                    catch (VContainerException ex)
                    {
                        throw new VContainerException($"Failed to resolve {injectTypeInfo.Type.FullName} : {ex.Message}");
                    }
                }
                var instance = injectTypeInfo.InjectConstructor.Invoke(parameterValues);
                Inject(instance, resolver);
                return instance;
            }
            finally
            {
                CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
            }
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
                var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(parameters.Length);
                try
                {
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        try
                        {
                            parameterValues[i] = resolver.Resolve(parameters[i].ParameterType);
                        }
                        catch (VContainerException ex)
                        {
                            throw new VContainerException($"Failed to resolve {injectTypeInfo.Type.FullName} : {ex.Message}");
                        }
                    }
                    method.Invoke(obj, parameterValues);
                }
                finally
                {
                    CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
                }
            }
        }
    }

    sealed class ReflectionInjectorBuilder : IInjectorBuilder
    {
        public static ReflectionInjectorBuilder Default = new ReflectionInjectorBuilder();

        public IInjector Build(Type type, bool skipConstructor)
        {
            var injectTypeInfo = TypeAnalyzer.Analyze(type, skipConstructor);
            return new ReflectionInjector(injectTypeInfo);
        }
    }
}