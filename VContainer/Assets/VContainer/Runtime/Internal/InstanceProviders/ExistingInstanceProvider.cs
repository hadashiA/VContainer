using System;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    sealed class ExistingInstanceProvider : IInstanceProvider
    {
        readonly object implementationInstance;
        readonly Func<IObjectResolver, object> implementationProvider;

        public ExistingInstanceProvider(object implementationInstance)
        {
            this.implementationInstance = implementationInstance;
        }
        
        public ExistingInstanceProvider(Func<IObjectResolver, object> implementationProvider)
        {
            this.implementationProvider = implementationProvider;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => implementationInstance ?? implementationProvider(resolver);
    }
}
