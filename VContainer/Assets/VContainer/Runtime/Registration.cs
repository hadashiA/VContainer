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
        public readonly object Key;

        public Registration(
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IInstanceProvider provider,
            object key = null)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;
            Provider = provider;
            Key = key;
        }

        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            var keyStr = Key == null ? "" : $" (Key: {Key})";
            return $"Registration {ImplementationType.Name}{keyStr} ContractTypes=[{contractTypes}] {Lifetime} {Provider}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => Provider.SpawnInstance(resolver);
    }
}
