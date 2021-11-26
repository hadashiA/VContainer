using System;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    sealed class FuncInstanceSpawner : IInstanceSpawner
    {
        readonly Func<IObjectResolver, object> implementationProvider;

        public FuncInstanceSpawner(Func<IObjectResolver, object> implementationProvider)
        {
            this.implementationProvider = implementationProvider;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Spawn(IObjectResolver resolver)
        {
            return implementationProvider(resolver);
        }
    }
}
