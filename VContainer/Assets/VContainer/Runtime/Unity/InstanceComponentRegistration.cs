using System;
using System.Collections.Generic;

namespace VContainer.Unity
{
    sealed class InstanceComponentRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime => Lifetime.Singleton;

        readonly object instance;
        readonly IReadOnlyList<IInjectParameter> parameters;
        readonly IInjector injector;

        public InstanceComponentRegistration(
            object instance,
            Type implementationType,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            IInjector injector)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            this.instance = instance;
            this.parameters = parameters;
            this.injector = injector;
        }
        
        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            return $"InstanceComponentRegistration {instance.GetType()} ContractTypes=[{contractTypes}] {Lifetime} {injector.GetType().Name})]";
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            injector.Inject(instance, resolver, parameters);
            return instance;
        }
    }
}