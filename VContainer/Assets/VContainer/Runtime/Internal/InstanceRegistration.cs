using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    sealed class InstanceRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }
        public int ExecutionOrder { get; }

        readonly object implementationInstance;

        public InstanceRegistration(object implementationInstance,
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            int executionOrder)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
            InterfaceTypes = interfaceTypes;
            ExecutionOrder = executionOrder;
            this.implementationInstance = implementationInstance;
        }

        public object SpawnInstance(IObjectResolver resolver) => implementationInstance;
    }
}