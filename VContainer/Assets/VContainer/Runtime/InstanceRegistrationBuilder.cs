using VContainer.Internal;

namespace VContainer
{
    public sealed class InstanceRegistrationBuilder : RegistrationBuilder
    {
        readonly object instance;

        public InstanceRegistrationBuilder(object instance)
            : base(instance.GetType(), Lifetime.Singleton)
        {
            this.instance = instance;
        }

        public override IRegistration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);

            return new InstanceRegistration(
                instance,
                ImplementationType,
                Lifetime,
                InterfaceTypes,
                Parameters,
                injector);
        }
    }
}