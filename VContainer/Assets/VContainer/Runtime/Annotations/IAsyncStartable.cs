using System.Threading;
#if UNITY_2023_1_OR_NEWER
using UnityEngine;
#elif VCONTAINER_UNITASK_INTEGRATION
using Awaitable = Cysharp.Threading.Tasks.UniTask;
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
