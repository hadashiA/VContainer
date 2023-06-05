#if VCONTAINER_UNITASK_INTEGRATION
using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;

namespace VContainer.Internal
{
    internal sealed class AsyncFuncInstanceProvider<T> : IInstanceProvider
    {
        private readonly Func<IObjectResolver, UniTask<T>> implementationProvider;

        public AsyncFuncInstanceProvider(Func<IObjectResolver, UniTask<T>> implementationProvider)
        {
            this.implementationProvider = implementationProvider;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => implementationProvider(resolver);
    }
}
#endif