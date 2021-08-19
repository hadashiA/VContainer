using System;
using System.Collections.Generic;

namespace VContainer
{
    /// <summary>
    /// Extension methods for <see cref="IObjectResolver"/> to simplify its use.
    /// </summary>
    /// <remarks>
    /// Most uses of <see cref="IObjectResolver"/> will be through extension methods
    /// such as these.
    /// </remarks>
    /// <seealso cref="Unity.ObjectResolverUnityExtensions"/>
    public static class IObjectResolverExtensions
    {
        /// <summary>
        /// Retrieves the dependency mapped to the given type.
        /// </summary>
        /// <param name="resolver">
        /// The container in which the dependency will be resolved.
        /// </param>
        /// <typeparam name="T">
        /// The type of the dependency to resolve. Note that multiple types can
        /// map to the same dependency.
        /// </typeparam>
        /// <returns>The resolved dependency.</returns>
        /// <exception cref="VContainerException">
        /// No dependency of type <typeparamref name="T"/> could be resolved.
        /// </exception>
        /// <seealso cref="IObjectResolver.Resolve(System.Type)"/>
        public static T Resolve<T>(this IObjectResolver resolver) => (T)resolver.Resolve(typeof(T));

        /// <remarks>
        /// Used internally by VContainer and the IL code it generates. You probably
        /// won't need to use this in your own code.
        /// </remarks>
        /// <seealso cref="IInjector"/>
        /// <seealso cref="IInjectParameter"/>
        [Preserve]
        public static object ResolveNonGeneric(this IObjectResolver resolve, Type type) => resolve.Resolve(type);

        /// <inheritdoc cref="ResolveNonGeneric"/>
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
