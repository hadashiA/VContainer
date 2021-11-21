using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    sealed class Registration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> parameters;

        internal Registration(
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            IInjector injector)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;

            this.injector = injector;
            this.parameters = parameters;
        }

        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            return $"Registration {ImplementationType.Name} ContractTypes=[{contractTypes}] {Lifetime} {injector.GetType().Name}";
        }

        public object SpawnInstance(IObjectResolver resolver)
            => injector.CreateInstance(resolver, parameters);
    }

    sealed class ContainerRegistration : IRegistration
    {
        public static readonly ContainerRegistration Default = new ContainerRegistration();

        public Type ImplementationType => typeof(IObjectResolver);
        public IReadOnlyList<Type> InterfaceTypes => null;
        public Lifetime Lifetime => Lifetime.Transient;

        public object SpawnInstance(IObjectResolver resolver) => resolver;
    }
}
