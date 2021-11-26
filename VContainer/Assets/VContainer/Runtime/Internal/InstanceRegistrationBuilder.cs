using System.Runtime.CompilerServices;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Registration Build()
        {
            var spawner = new ExistingInstanceSpawner(implementationInstance);
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, spawner);
        }
    }
}
