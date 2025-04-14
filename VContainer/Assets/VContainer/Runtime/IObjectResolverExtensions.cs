using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer
{
    public static class IObjectResolverExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IObjectResolver resolver, object id = null) => (T)resolver.Resolve(typeof(T), id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve<T>(this IObjectResolver resolver, out T resolved, object id = null)
        {
            if (resolver.TryResolve(typeof(T), out var r, id))
            {
                resolved = (T)r;
                return true;
            }

            resolved = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ResolveOrDefault<T>(this IObjectResolver resolver, T defaultValue = default, object id = null)
        {
            if (resolver.TryResolve(typeof(T), out var value, id))
            {
                return (T)value;
            }

            return defaultValue;
        }

        // Using from CodeGen
        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ResolveNonGeneric(this IObjectResolver resolve, Type type, object id = null) => resolve.Resolve(type, id);

        public static object ResolveOrParameter(
            this IObjectResolver resolver,
            Type parameterType,
            string parameterName,
            IReadOnlyList<IInjectParameter> parameters,
            object id = null)
        {
            if (parameters == null)
            {
                return id != null 
                    ? resolver.Resolve(parameterType, id) 
                    : resolver.Resolve(parameterType);
            }
            
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (parameter.Match(parameterType, parameterName))
                {
                    return parameter.GetValue(resolver);
                }
            }

            return id != null ?
                resolver.Resolve(parameterType, id) :
                resolver.Resolve(parameterType);
        }
    }
}