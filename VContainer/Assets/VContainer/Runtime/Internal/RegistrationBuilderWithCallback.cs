using System;

namespace VContainer.Internal
{
    sealed class RegistrationBuilderWithCallback : RegistrationBuilder
    {
        readonly Action<object, IObjectResolver> callback;
        
        public RegistrationBuilderWithCallback(Type implementationType, Lifetime lifetime, Action<object, IObjectResolver> callback) 
            : base(implementationType, lifetime)
        {
            this.callback = callback;
        }
        
        public override Registration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);
            var spawner = new InstanceProviderCallbackDecorator(new InstanceProvider(injector, Parameters), callback);
            return new Registration(
                ImplementationType,
                Lifetime,
                InterfaceTypes,
                spawner);
        }
    }
}