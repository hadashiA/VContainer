using System;
using System.Collections.Generic;
using VContainer.Runtime.Unity;
#if VCONTAINER_UNITASK_INTEGRATION
using System.Threading;
using Cysharp.Threading.Tasks;
#endif

namespace VContainer.Unity
{
    sealed class InitializationLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IInitializable> entries;
        readonly StartupExceptionHandler exceptionHandler;
        bool disposed;

        public InitializationLoopItem(
            IEnumerable<IInitializable> entries,
            StartupExceptionHandler exceptionHandler = null)
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
                    if (exceptionHandler != null)
                        exceptionHandler.Publish(ex);
                    else
                        throw;
                }
            }
            return false;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostInitializationLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IPostInitializable> entries;
        readonly StartupExceptionHandler exceptionHandler;
        bool disposed;

        public PostInitializationLoopItem(
            IEnumerable<IPostInitializable> entries,
            StartupExceptionHandler exceptionHandler)
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
                    if (exceptionHandler != null)
                        exceptionHandler.Publish(ex);
                    else
                        throw;
                }
            }
            return false;
        }

        public void Dispose() => disposed = true;
    }

    sealed class StartableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IStartable> entries;
        readonly StartupExceptionHandler exceptionHandler;
        bool disposed;

        public StartableLoopItem(
            IEnumerable<IStartable> entries,
            StartupExceptionHandler exceptionHandler)
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
                    if (exceptionHandler != null)
                        exceptionHandler.Publish(ex);
                    else
                        throw;
                }
            }
            return false;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostStartableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IPostStartable> entries;
        readonly StartupExceptionHandler exceptionHandler;
        bool disposed;

        public PostStartableLoopItem(
            IEnumerable<IPostStartable> entries,
            StartupExceptionHandler exceptionHandler)
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
                    if (exceptionHandler != null)
                        exceptionHandler.Publish(ex);
                    else
                        throw;
                }
            }
            return false;
        }

        public void Dispose() => disposed = true;
    }

    sealed class FixedTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IFixedTickable> entries;
        bool disposed;

        public FixedTickableLoopItem(IEnumerable<IFixedTickable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.FixedTick();
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostFixedTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IPostFixedTickable> entries;
        bool disposed;

        public PostFixedTickableLoopItem(IEnumerable<IPostFixedTickable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.PostFixedTick();
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    sealed class TickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<ITickable> entries;
        bool disposed;

        public TickableLoopItem(IEnumerable<ITickable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.Tick();
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IPostTickable> entries;
        bool disposed;

        public PostTickableLoopItem(IEnumerable<IPostTickable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.PostTick();
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    sealed class LateTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<ILateTickable> entries;
        bool disposed;

        public LateTickableLoopItem(IEnumerable<ILateTickable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.LateTick();
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostLateTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IPostLateTickable> entries;
        bool disposed;

        public PostLateTickableLoopItem(IEnumerable<IPostLateTickable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.PostLateTick();
            }
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

#if VCONTAINER_UNITASK_INTEGRATION
    sealed class AsyncStartableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IAsyncStartable> entries;
        readonly StartupExceptionHandler exceptionHandler;
        readonly CancellationTokenSource cts = new CancellationTokenSource();
        bool disposed;

        public AsyncStartableLoopItem(
            IEnumerable<IAsyncStartable> entries,
            StartupExceptionHandler exceptionHandler)
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
