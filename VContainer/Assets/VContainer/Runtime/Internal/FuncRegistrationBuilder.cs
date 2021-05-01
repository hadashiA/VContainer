using System;

namespace VContainer.Internal
{
    sealed class FuncRegistrationBuilder : RegistrationBuilder
    {
        readonly Func<IObjectResolver, object> implementationConfiguration;

        public FuncRegistrationBuilder(
            Func<IObjectResolver, object> implementationConfiguration,
            Type implementationType,
            Lifetime lifetime) : base(implementationType, lifetime)
        {
            this.implementationConfiguration = implementationConfiguration;
        }

        public override IRegistration Build() => new FuncRegistration(
            implementationConfiguration,
            ImplementationType,
            Lifetime,
            InterfaceTypes);
    }
}
