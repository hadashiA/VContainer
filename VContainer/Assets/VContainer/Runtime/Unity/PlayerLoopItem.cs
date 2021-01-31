using System;
using System.Collections.Generic;
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
        readonly IEnumerable<IFixedTickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public FixedTickableLoopItem(
            IEnumerable<IFixedTickable> entries,
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
        readonly IEnumerable<IPostFixedTickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public PostFixedTickableLoopItem(
            IEnumerable<IPostFixedTickable> entries,
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
        readonly IEnumerable<ITickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public TickableLoopItem(
            IEnumerable<ITickable> entries,
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
        readonly IEnumerable<IPostTickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public PostTickableLoopItem(
            IEnumerable<IPostTickable> entries,
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
        readonly IEnumerable<ILateTickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public LateTickableLoopItem(
            IEnumerable<ILateTickable> entries,
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
        readonly IEnumerable<IPostLateTickable> entries;
        readonly EntryPointExceptionHandler exceptionHandler;
        bool disposed;

        public PostLateTickableLoopItem(
            IEnumerable<IPostLateTickable> entries,
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
