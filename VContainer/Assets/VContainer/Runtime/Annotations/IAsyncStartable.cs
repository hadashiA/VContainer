#if VCONTAINER_UNITASK_INTEGRATION
using System.Threading;
using Cysharp.Threading.Tasks;

namespace VContainer.Unity
{
    public interface IAsyncStartable
    {
        UniTask StartAsync(CancellationToken cancellation);
    }
}
#elif UNITY_2023_1_OR_NEWER
using System.Threading;
using UnityEngine;

namespace VContainer.Unity
{
    public interface IAsyncStartable
    {
        Awaitable StartAsync(CancellationToken cancellation = default);
    }
}
#endif
