using System;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    sealed class InstanceRegistrationFromResolveBuilder : RegistrationBuilder
    {
        readonly Func<IObjectResolver, object> implementationProvider;

        public InstanceRegistrationFromResolveBuilder(Func<IObjectResolver, object> implementationProvider, Type implementationType)
            : base(implementationType, Lifetime.Singleton)
        {
            this.implementationProvider = implementationProvider;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Registration Build()
        {
            var spawner = new ExistingInstanceProvider(implementationProvider);
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, spawner);
        }
    }
}