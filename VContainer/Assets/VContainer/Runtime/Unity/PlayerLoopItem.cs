using System;
using System.Collections.Generic;
using System.Linq;
#if VCONTAINER_UNITASK_INTEGRATION
using System.Threading;
using Cysharp.Threading.Tasks;
#endif

namespace VContainer.Unity
{
    sealed class InitializationLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IInitializable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public InitializationLoopItem(
            IEnumerable<IInitializable> entries,
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
                    x.Initialize();
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

    sealed class PostInitializationLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IPostInitializable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public PostInitializationLoopItem(
            IEnumerable<IPostInitializable> entries,
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
                    x.PostInitialize();
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
                var x = entries[i];
                try
                {
                    x.FixedTick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                    return false;
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
                var x = entries[i];
                try
                {
                    x.PostFixedTick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                    return false;
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
                var x = entries[i];
                try
                {
                    x.Tick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                    return false;
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
                var x = entries[i];
                try
                {
                    x.PostTick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                    return false;
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
                var x = entries[i];
                try
                {
                    x.LateTick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                    return false;
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
                var x = entries[i];
                try
                {
                    x.PostLateTick();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) throw;
                    exceptionHandler.Publish(ex);
                    return false;
                }
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

#if VCONTAINER_UNITASK_INTEGRATION
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
                var task = x.StartAsync(cts.Token);
                if (exceptionHandler != null)
                    task.Forget(ex => exceptionHandler.Publish(ex));
                else
                    task.Forget();
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
