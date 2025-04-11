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
        public static T Resolve<T>(this IObjectResolver resolver, string id) => (T)resolver.Resolve(typeof(T), id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve<T>(this IObjectResolver resolver, out T resolved)
        {
            if (resolver.TryResolve(typeof(T), out var r))
            {
                resolved = (T)r;
                return true;
            }

            resolved = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve<T>(this IObjectResolver resolver, string id, out T resolved)
        {
            if (resolver.TryResolve(typeof(T), id, out var r))
            {
                resolved = (T)r;
                return true;
            }

            resolved = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ResolveOrDefault<T>(this IObjectResolver resolver, T defaultValue = default)
        {
            if (resolver.TryResolve(typeof(T), out var value))
            {
                return (T)value;
            }

            return defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ResolveOrDefault<T>(this IObjectResolver resolver, string id, T defaultValue = default)
        {
            if (resolver.TryResolve(typeof(T), id, out var value))
            {
                return (T)value;
            }

            return defaultValue;
        }

        // Using from CodeGen
        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ResolveNonGeneric(this IObjectResolver resolve, Type type) => resolve.Resolve(type);

        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ResolveNonGeneric(this IObjectResolver resolve, Type type, string id) => resolve.Resolve(type, id);

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
                        return parameter.GetValue(resolver);
                    }
                }
            }
            return resolver.Resolve(parameterType);
        }

        public static object ResolveOrParameter(
            this IObjectResolver resolver,
            Type parameterType,
            string parameterName,
            string id,
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
                        return parameter.GetValue(resolver);
                    }
                }
            }
            
            var result = resolver.Resolve(parameterType, id);
            return result;
        }
    }
}