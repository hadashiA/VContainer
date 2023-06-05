#if VCONTAINER_UNITASK_INTEGRATION
using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using VContainer.Internal;

namespace VContainer
{
    public static class ContainerAsyncBuilderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterAsync<TInterface>(
            this IContainerBuilder builder,
            Func<IObjectResolver, UniTask<TInterface>> implementationConfiguration,
            Lifetime lifetime)
            where TInterface : class
        {
            return builder.Register(new AsyncFuncRegistrationBuilder<TInterface>(implementationConfiguration, typeof(UniTask<TInterface>), lifetime));
        }
    }
}
#endif