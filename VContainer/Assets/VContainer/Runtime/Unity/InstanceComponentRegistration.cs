using System;
using System.Collections.Generic;

namespace VContainer.Unity
{
    sealed class InstanceComponentRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime => Lifetime.Singleton;
        public int ExecutionOrder { get; }

        readonly object instance;
        readonly IReadOnlyList<IInjectParameter> parameters;
        readonly IInjector injector;

        public InstanceComponentRegistration(
            object instance,
            Type implementationType,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            int executionOrder,
            IInjector injector)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            ExecutionOrder = executionOrder;
            this.instance = instance;
            this.parameters = parameters;
            this.injector = injector;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            injector.Inject(instance, resolver, parameters);
            return instance;
        }
    }
}