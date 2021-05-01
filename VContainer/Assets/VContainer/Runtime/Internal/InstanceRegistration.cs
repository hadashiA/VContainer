using System;
using System.Collections.Generic;
using System.Threading;

namespace VContainer.Internal
{
    sealed class InstanceRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly object implementationInstance;
        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> parameters;

        long injected;

        public InstanceRegistration(
            object implementationInstance,
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            IInjector injector)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
            InterfaceTypes = interfaceTypes;

            this.implementationInstance = implementationInstance;
            this.injector = injector;
            this.parameters = parameters;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            if (Interlocked.CompareExchange(ref injected, 1, 0) == 0)
            {
                injector.Inject(implementationInstance, resolver, parameters);
            }
            return implementationInstance;
        }
    }
}