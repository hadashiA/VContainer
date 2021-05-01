using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    sealed class FuncRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly Func<IObjectResolver, object> implementationProvider;

        public FuncRegistration(
            Func<IObjectResolver, object> implementationProvider,
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
            InterfaceTypes = interfaceTypes;

            this.implementationProvider = implementationProvider;
        }

        public object SpawnInstance(IObjectResolver resolver) => implementationProvider(resolver);
    }
}