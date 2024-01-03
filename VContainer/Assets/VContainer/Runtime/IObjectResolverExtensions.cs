using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VContainer.Internal;

namespace VContainer
{
    public static class IObjectResolverExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IObjectResolver resolver) => (T)resolver.Resolve(typeof(T));

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

        public static object CreateInstance<T>(this IObjectResolver container) =>
            container.CreateInstance(typeof(T));

        public static object CreateInstance<T>(this IObjectResolver container, IReadOnlyList<IInjectParameter> parameters) =>
            container.CreateInstance(typeof(T), parameters);

        public static object CreateInstance(this IObjectResolver container, Type type) =>
            container.CreateInstance(type, null);

        public static object CreateInstance(this IObjectResolver container, Type type, IReadOnlyList<IInjectParameter> parameters)
        {
            var injector = InjectorCache.GetOrBuild(type);
            return injector.CreateInstance(container, parameters);
        }
    }
}