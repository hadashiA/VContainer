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
        public readonly IInstanceSpawner Spawner;

        internal Registration(
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IInstanceSpawner spawner)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;
            Spawner = spawner;
        }

        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            return $"Registration {ImplementationType.Name} ContractTypes=[{contractTypes}] {Lifetime} {Spawner}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => Spawner.Spawn(resolver);
    }
}
