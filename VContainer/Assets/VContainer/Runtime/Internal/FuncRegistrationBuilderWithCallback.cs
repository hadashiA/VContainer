using System;

namespace VContainer.Internal
{
    sealed class FuncRegistrationBuilderWithCallback : RegistrationBuilder
    {
        readonly Func<IObjectResolver, object> implementationProvider;
        readonly Action<object, IObjectResolver> callback;

        public FuncRegistrationBuilderWithCallback(
            Func<IObjectResolver, object> implementationProvider,
            Type implementationType,
            Lifetime lifetime, 
            Action<object, IObjectResolver> callback) : base(implementationType, lifetime)
        {
            this.implementationProvider = implementationProvider;
            this.callback = callback;
        }

        public override Registration Build()
        {
            var spawner = new InstanceProviderCallbackDecorator(new FuncInstanceProvider(implementationProvider), callback);
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, spawner);
        }
    }
}