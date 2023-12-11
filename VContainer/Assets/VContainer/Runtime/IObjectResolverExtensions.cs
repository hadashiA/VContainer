using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer
{
    public static class IObjectResolverExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IObjectResolver resolver) => (T)resolver.Resolve(typeof(T));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetSharedInstance<T>(this IObjectResolver resolver, out T instance)
        {
            if (!resolver.TryGetSharedInstance(typeof(T), out var boxed))
            {
                instance = default;
                return false;
            }

            instance = (T)boxed;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetSharedInstanceOrDefault<T>(this IObjectResolver resolver)
        {
            if (resolver.TryGetSharedInstance<T>(out var instance))
                return instance;
            
            return default;
        }

        // Using from CodeGen
        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ResolveNonGeneric(this IObjectResolver resolve, Type type) => resolve.Resolve(type);

        public static object ResolveOrParameter(
            this IObjectResolver resolver,
            Type parameterType,
            string parameterName,
            IReadOnlyList<IInjectParameter> parameters)
        {
            if (parameters != null)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.Match(parameterType, parameterName))
                    {
                        return parameter.Value;
                    }
                }
            }
            return resolver.Resolve(parameterType);
        }
    }
}