namespace VContainer.Internal
{
    sealed class InstanceRegistrationBuilder : RegistrationBuilder
    {
        readonly object implementationInstance;

        public InstanceRegistrationBuilder(object implementationInstance)
            : base(implementationInstance.GetType(), Lifetime.Singleton)
        {
            this.implementationInstance = implementationInstance;
        }

        public override IRegistration Build()
        {
            return new InstanceRegistration(
                implementationInstance,
                ImplementationType,
                Lifetime,
                InterfaceTypes);
        }
    }
}
