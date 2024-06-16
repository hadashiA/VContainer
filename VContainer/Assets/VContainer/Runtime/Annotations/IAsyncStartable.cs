using System.Threading;
#if VCONTAINER_UNITASK_INTEGRATION
using Awaitable = Cysharp.Threading.Tasks.UniTask;
#elif UNITY_2023_1_OR_NEWER
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
