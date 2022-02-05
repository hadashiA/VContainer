using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer
{
    public sealed class Registration
    {
        public readonly Type ImplementationType;
        public readonly IReadOnlyList<Type> InterfaceTypes;
        public readonly Lifetime Lifetime;
        public readonly IInstanceProvider Provider;

        public Registration(
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IInstanceProvider provider)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;
            Provider = provider;
        }

        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            return $"Registration {ImplementationType.Name} ContractTypes=[{contractTypes}] {Lifetime} {Provider}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => Provider.SpawnInstance(resolver);
    }
}
