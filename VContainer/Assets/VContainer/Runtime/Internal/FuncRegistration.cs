using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    sealed class FuncRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }
        public int ExecutionOrder { get; }

        readonly Func<IObjectResolver, object> implementationProvider;

        public FuncRegistration(
            Func<IObjectResolver, object> implementationProvider,
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            int executionOrder)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
            InterfaceTypes = interfaceTypes;
            ExecutionOrder = executionOrder;

            this.implementationProvider = implementationProvider;
        }

        public object SpawnInstance(IObjectResolver resolver) => implementationProvider(resolver);
    }
}