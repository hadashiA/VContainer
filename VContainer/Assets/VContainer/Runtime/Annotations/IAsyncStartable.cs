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
#elif UNITY_2021_3_OR_NEWER
using System.Threading;
#if UNITY_2023_1_OR_NEWER
using UnityEngine;
#else
using Awaitable = System.Threading.Tasks.Task;
#endif

namespace VContainer.Unity
{
    public interface IAsyncStartable
    {
        Awaitable StartAsync(CancellationToken cancellation = default);
    }
}
#endif
