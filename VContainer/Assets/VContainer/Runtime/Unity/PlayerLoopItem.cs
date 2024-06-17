using System;
using System.Collections.Generic;
using System.Threading;
#if VCONTAINER_UNITASK_INTEGRATION
using Cysharp.Threading.Tasks;
#endif

namespace VContainer.Unity
{
    sealed class StartableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IStartable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public StartableLoopItem(
            IEnumerable<IStartable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                try
                {
                    x.Start();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                }
            }
            return false;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostStartableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IPostStartable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public PostStartableLoopItem(
            IEnumerable<IPostStartable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                try
                {
                    x.PostStart();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                }
            }
            return false;
        }

        public void Dispose() => disposed = true;
    }

    sealed class FixedTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IReadOnlyList<IFixedTickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public FixedTickableLoopItem(
            IReadOnlyList<IFixedTickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            for (var i = 0; i < entries.Count; i++)
            {
                try
                {
                    entries[i].FixedTick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                }
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostFixedTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IReadOnlyList<IPostFixedTickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public PostFixedTickableLoopItem(
            IReadOnlyList<IPostFixedTickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            for (var i = 0; i < entries.Count; i++)
            {
                try
                {
                    entries[i].PostFixedTick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                }
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    sealed class TickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IReadOnlyList<ITickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public TickableLoopItem(
            IReadOnlyList<ITickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            for (var i = 0; i < entries.Count; i++)
            {
                try
                {
                    entries[i].Tick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                }
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IReadOnlyList<IPostTickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public PostTickableLoopItem(
            IReadOnlyList<IPostTickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            for (var i = 0; i < entries.Count; i++)
            {
                try
                {
                    entries[i].PostTick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                }
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    sealed class LateTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IReadOnlyList<ILateTickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public LateTickableLoopItem(
            IReadOnlyList<ILateTickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            for (var i = 0; i < entries.Count; i++)
            {
                try
                {
                    entries[i].LateTick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                }
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostLateTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IReadOnlyList<IPostLateTickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public PostLateTickableLoopItem(
            IReadOnlyList<IPostLateTickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            for (var i = 0; i < entries.Count; i++)
            {
                try
                {
                    entries[i].PostLateTick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                }
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

#if VCONTAINER_UNITASK_INTEGRATION || UNITY_2021_3_OR_NEWER
    sealed class AsyncStartableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IAsyncStartable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        readonly CancellationTokenSource cts = new CancellationTokenSource();
        bool disposed;

        public AsyncStartableLoopItem(
            IEnumerable<IAsyncStartable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
#if VCONTAINER_UNITASK_INTEGRATION
                var task = x.StartAsync(cts.Token);
                if (exceptionHandler != null)
                {
                    task.Forget(ex => exceptionHandler.Publish(ex));
                }
                else
                {
                    task.Forget();
                }
#else
                try
                {
                    var task = x.StartAsync(cts.Token);
                    _ = task.Forget(exceptionHandler);
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                }
#endif
            }
            return false;
        }

        public void Dispose()
        {
            lock (entries)
            {
                if (disposed) return;
                disposed = true;
            }
            cts.Cancel();
            cts.Dispose();
        }
    }
#endif
}
