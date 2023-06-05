#if VCONTAINER_UNITASK_INTEGRATION
using System;
using Cysharp.Threading.Tasks;

namespace VContainer.Internal
{
    internal sealed class AsyncFuncRegistrationBuilder<T> : RegistrationBuilder
    {
        private readonly Func<IObjectResolver, UniTask<T>> implementationProvider;

        public AsyncFuncRegistrationBuilder(
            Func<IObjectResolver, UniTask<T>> implementationProvider,
            Type implementationType,
            Lifetime lifetime)
            : base(implementationType, lifetime)
        {
            this.implementationProvider = implementationProvider;
        }

        public override Registration Build()
        {
            var spawner = new AsyncFuncInstanceProvider<T>(implementationProvider);
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, spawner);
        }
    }
}
#endif