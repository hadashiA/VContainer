using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    sealed class ReflectionInjector : IInjector
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReflectionInjector Build(Type type)
        {
            var injectTypeInfo = TypeAnalyzer.AnalyzeWithCache(type);
            return new ReflectionInjector(injectTypeInfo);
        }

        readonly InjectTypeInfo injectTypeInfo;

        ReflectionInjector(InjectTypeInfo injectTypeInfo)
        {
            this.injectTypeInfo = injectTypeInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
        {
            InjectFields(instance, resolver, parameters);
            InjectProperties(instance, resolver, parameters);
            InjectMethods(instance, resolver, parameters);
        }

        public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
        {
#if UNITY_2020_1_OR_NEWER
            if (typeof(UnityEngine.Component).IsAssignableFrom(injectTypeInfo.Type))
            {
                throw new NotSupportedException($"UnityEngine.Component:{injectTypeInfo.Type.Name} cannot be `new`");
            }
#endif
            var parameterInfos = injectTypeInfo.InjectConstructor.ParameterInfos;
            var parameterKeys = injectTypeInfo.InjectConstructor.ParameterKeys;
            var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(parameterInfos.Length);
            try
            {
                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    var parameterInfo = parameterInfos[i];
                    var key = parameterKeys[i];

                    parameterValues[i] = resolver.ResolveOrParameter(
                        parameterInfo.ParameterType,
                        parameterInfo.Name,
                        parameters,
                        key);
                }
                var instance = injectTypeInfo.InjectConstructor.ConstructorInfo.Invoke(parameterValues);
                Inject(instance, resolver, parameters);
                return instance;
            }
            catch (VContainerException ex)
            {
                throw new VContainerException(ex.InvalidType, $"Failed to resolve {injectTypeInfo.Type} : {ex.Message}");
            }
            finally
            {
                CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
            }
        }

        void InjectFields(object obj, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
        {
            if (injectTypeInfo.InjectFields == null)
                return;

            foreach (var x in injectTypeInfo.InjectFields)
            {
                var fieldValue = resolver.ResolveOrParameter(x.FieldType, x.Name, parameters, x.Key);
                x.SetValue(obj, fieldValue);
            }
        }

        void InjectProperties(object obj, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
        {
            if (injectTypeInfo.InjectProperties == null)
                return;

            foreach (var x in injectTypeInfo.InjectProperties)
            {
                var propValue = resolver.ResolveOrParameter(x.PropertyType, x.Name, parameters, x.Key);
                x.SetValue(obj, propValue);
            }
        }

        void InjectMethods(object obj, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
        {
            if (injectTypeInfo.InjectMethods == null)
                return;

            foreach (var method in injectTypeInfo.InjectMethods)
            {
                var parameterInfos = method.ParameterInfos;
                var parameterKeys = method.ParameterKeys;
                var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(parameterInfos.Length);
                try
                {
                    for (var i = 0; i < parameterInfos.Length; i++)
                    {
                        var parameterInfo = parameterInfos[i];
                        var key = parameterKeys[i];
                        
                        parameterValues[i] = resolver.ResolveOrParameter(
                            parameterInfo.ParameterType,
                            parameterInfo.Name,
                            parameters,
                            key);
                    }
                    method.MethodInfo.Invoke(obj, parameterValues);
                }
                catch (VContainerException ex)
                {
                    throw new VContainerException(ex.InvalidType, $"Failed to resolve {injectTypeInfo.Type} : {ex.Message}");
                }
                finally
                {
                    CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
                }
            }
        }
    }
}