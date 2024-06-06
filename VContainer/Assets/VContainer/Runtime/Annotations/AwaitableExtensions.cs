#if UNITY_2021_3_OR_NEWER
using System;
#if UNITY_2023_1_OR_NEWER
using UnityEngine;
#else
using Awaitable = System.Threading.Tasks.Task;
#endif


namespace VContainer.Unity
{
    internal static class AwaitableExtensions
    {
        public static async Awaitable Forget(this Awaitable awaitable,
            EntryPointExceptionHandler exceptionHandler = null)
        {
            try
            {
                await awaitable;
            }
            catch (Exception ex)
            {
                if (exceptionHandler != null)
                    exceptionHandler.Publish(ex);
                else
                    throw;
            }
        }
    }
}
#endif
