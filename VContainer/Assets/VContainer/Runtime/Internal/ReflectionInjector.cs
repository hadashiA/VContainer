using System;
using System.Collections.Generic;
#if UNITY_2018_4_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace VContainer.Internal
{
    sealed class ReflectionInjector : IInjector
    {
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

        public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
        {
            InjectMethods(instance, resolver, parameters);
            InjectProperties(instance, resolver);
            InjectFields(instance, resolver);
        }

        public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
        {
            var parameterInfos = injectTypeInfo.InjectConstructor.ParameterInfos;
            var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(parameterInfos.Length);
            try
            {
                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    var parameterInfo = parameterInfos[i];
                    try
                    {
                        parameterValues[i] = resolver.ResolveOrParameter(
                            parameterInfo.ParameterType,
                            parameterInfo.Name,
                            parameters);
                    }
                    catch (VContainerException ex)
                    {
                        throw new VContainerException(parameterInfo.ParameterType, $"Failed to resolve {injectTypeInfo.Type.FullName} : {ex.Message}");
                    }
                }
                var instance = injectTypeInfo.InjectConstructor.ConstructorInfo.Invoke(parameterValues);
                Inject(instance, resolver, parameters);
                return instance;
            }
            finally
            {
                CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
            }
        }

        void InjectFields(object obj, IObjectResolver resolver)
        {
            if (injectTypeInfo.InjectFields == null)
                return;

            foreach (var x in injectTypeInfo.InjectFields)
            {
                var fieldValue = resolver.Resolve(x.FieldInfo.FieldType);
#if UNITY_2018_4_OR_NEWER
                if (!x.FieldInfo.FieldType.IsValueType)
                {
                    unsafe
                    {
                        var objPtr = UnsafeUtility.PinGCObjectAndGetAddress(obj, out var targetHandle);
                        UnsafeUtility.CopyObjectAddressToPtr(fieldValue, (byte*)objPtr + x.Offset);
                        UnsafeUtility.ReleaseGCObject(targetHandle);
                    }
                }
                else
                {
                    x.FieldInfo.SetValue(obj, fieldValue);
                }
#else
                x.FieldInfo.SetValue(obj, fieldValue);
#endif
            }
        }

        void InjectProperties(object obj, IObjectResolver resolver)
        {
            if (injectTypeInfo.InjectProperties == null)
                return;

            foreach (var x in injectTypeInfo.InjectProperties)
            {
                var propValue = resolver.Resolve(x.PropertyType);
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
                var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(parameterInfos.Length);
                try
                {
                    for (var i = 0; i < parameterInfos.Length; i++)
                    {
                        var parameterInfo = parameterInfos[i];
                        try
                        {
                            parameterValues[i] = resolver.ResolveOrParameter(
                                parameterInfo.ParameterType,
                                parameterInfo.Name,
                                parameters);
                        }
                        catch (VContainerException ex)
                        {
                            throw new VContainerException(parameterInfo.ParameterType, $"Failed to resolve {injectTypeInfo.Type.FullName} : {ex.Message}");
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
}