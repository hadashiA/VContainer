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
using System;
using System.Threading;
using UnityEngine;

namespace VContainer.Unity
{
    public interface IAsyncStartable
    {
        Awaitable StartAsync(CancellationToken cancellation);
    }

    static class AwaitableHelper
    {
        public static async Awaitable Forget(Awaitable awaitable, EntryPointExceptionHandler exceptionHandler)
        {
            try
            {
                await awaitable;
            }
            catch (Exception ex)
            {
                if (exceptionHandler != null)
                {
                    exceptionHandler.Publish(ex);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
#endif
