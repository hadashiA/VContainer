using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    sealed class GenericFallbackRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly IReadOnlyList<IInjectParameter> parameters;
        readonly bool asImplementedInterfaces;
        readonly bool asSelf;

        public GenericFallbackRegistration(
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            bool asImplementedInterfaces,
            bool asSelf)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
            InterfaceTypes = interfaceTypes;
            this.parameters = parameters;
            this.asImplementedInterfaces = asImplementedInterfaces;
        }

        public object SpawnInstance(IObjectResolver resolver) => throw new NotSupportedException();

        public IRegistration BuildGenericRegistration(Type interfaceOpenGenericType, Type interfaceType)
        {
            if (!interfaceOpenGenericType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    $"{nameof(interfaceOpenGenericType)} {interfaceOpenGenericType} is not an open generic.");
            }

            if (!interfaceType.IsConstructedGenericType)
            {
                throw new ArgumentException($"{nameof(interfaceType)} {interfaceType} is not a generic type");
            }

            var closedGenericType = interfaceOpenGenericType == ImplementationType
                ? interfaceType
                : ImplementationType.MakeGenericType(interfaceType.GetGenericArguments());

            var injector = InjectorCache.GetOrBuild(closedGenericType);

            return new Registration(
                closedGenericType,
                Lifetime,
                new[] { interfaceType },
                parameters,
                injector);
        }
    }
}