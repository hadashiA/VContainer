using System;

namespace VContainer.Internal
{
    sealed class FuncRegistrationBuilder : RegistrationBuilder
    {
        readonly Func<IObjectResolver, object> implementationProvider;

        public FuncRegistrationBuilder(
            Func<IObjectResolver, object> implementationProvider,
            Type implementationType,
            Lifetime lifetime) : base(implementationType, lifetime)
        {
            this.implementationProvider = implementationProvider;
        }

        public override Registration Build()
        {
            var spawner = new FuncInstanceProvider(implementationProvider);
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, spawner);
        }
    }
}
