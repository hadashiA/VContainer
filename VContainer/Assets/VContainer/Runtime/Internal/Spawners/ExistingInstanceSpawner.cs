using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    sealed class ExistingInstanceSpawner : IInstanceSpawner
    {
        readonly object implementationInstance;

        public ExistingInstanceSpawner(object implementationInstance)
        {
            this.implementationInstance = implementationInstance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Spawn(IObjectResolver resolver) => implementationInstance;
    }
}
