using System;
using System.Collections.Concurrent;

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
            var parameters = injectTypeInfo.InjectConstructor.ParameterInfos;
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
                        throw new VContainerException(injectTypeInfo.Type, $"Failed to resolve {injectTypeInfo.Type.FullName} : {ex.Message}");
                    }
                }
                var instance = injectTypeInfo.InjectConstructor.ConstructorInfo.Invoke(parameterValues);
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
                // if (f.GetValue())
                {
                    var fieldValue = resolver.Resolve(f.FieldInfo.FieldType);
                    f.FieldInfo.SetValue(obj, fieldValue);
                }
            }
        }

        void InjectProperties(object obj, IObjectResolver resolver)
        {
            foreach (var prop in injectTypeInfo.InjectProperties)
            {
                var propValue = resolver.Resolve(prop.PropertyInfo.PropertyType);
                prop.PropertyInfo.SetValue(obj, propValue);
            }
        }

        void InjectMethods(object obj, IObjectResolver resolver)
        {
            foreach (var method in injectTypeInfo.InjectMethods)
            {
                var parameters = method.ParameterInfos;
                var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(parameters.Length);
                try
                {
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var parameterType = parameters[i].ParameterType;
                        try
                        {
                            parameterValues[i] = resolver.Resolve(parameterType);
                        }
                        catch (VContainerException ex)
                        {
                            throw new VContainerException(parameterType, $"Failed to resolve {injectTypeInfo.Type.FullName} : {ex.Message}");
                        }
                    }
                    method.MethodInfo.Invoke(obj, parameterValues);
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
        public static readonly ReflectionInjectorBuilder Default = new ReflectionInjectorBuilder();

        // readonly ConcurrentDictionary<Type, ReflectionInjector> cache = new ConcurrentDictionary<Type, ReflectionInjector>();
        //
        // static readonly Func<Type, ReflectionInjector> Factory = type =>
        // {
        //     var injectTypeInfo = TypeAnalyzer.Analyze(type, false);
        //     return new ReflectionInjector(injectTypeInfo);
        // };
        //
        // static readonly Func<Type, ReflectionInjector> FactorySkipConstructor = type =>
        // {
        //     var injectTypeInfo = TypeAnalyzer.Analyze(type, true);
        //     return new ReflectionInjector(injectTypeInfo);
        // };

        public IInjector Build(Type type, bool skipConstructor)
        {
            // No cache
            var injectTypeInfo = TypeAnalyzer.Analyze(type, skipConstructor);

            // Dictionary cache
            // return cache.GetOrAdd(type, skipConstructor ? FactorySkipConstructor : Factory);

            // Static type cache
            // var cacheType = skipConstructor
            //     ? typeof(InjectTypeInfoCacheWithoutConstructor<>)
            //     : typeof(InjectTypeInfoCache<>);
            //
            // var genericCacheType = cacheType.MakeGenericType(type);
            // var injectTypeInfo = (InjectTypeInfo)genericCacheType.GetField("Value").GetValue(null);
            return new ReflectionInjector(injectTypeInfo);
        }
    }
}