#if VCONTAINER_UNITASK_INTEGRATION
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;

namespace VContainer
{
    public static class IObjectResolverAsyncExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<T> ResolveAsync<T>(this IObjectResolver resolver) => await resolver.Resolve<UniTask<T>>();
    }
}
#endif