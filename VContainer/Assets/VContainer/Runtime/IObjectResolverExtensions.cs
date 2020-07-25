using System;
using System.Collections.Generic;

namespace VContainer
{
    public static class IObjectResolverExtensions
    {
        public static T Resolve<T>(this IObjectResolver resolver) => (T)resolver.Resolve(typeof(T));

        public static T ResolveOrParameter<T>(
            this IObjectResolver resolver,
            string parameterName,
            IReadOnlyList<IInjectParameter> parameters)
            => (T)resolver.ResolveOrParameter(typeof(T), parameterName, parameters);

        public static object ResolveOrParameter(
            this IObjectResolver resolver,
            Type parameterType,
            string parameterName,
            IReadOnlyList<IInjectParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
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