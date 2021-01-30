using System;
using System.Collections.Generic;
#if VCONTAINER_UNITASK_INTEGRATION
using System.Threading;
using Cysharp.Threading.Tasks;
#endif

namespace VContainer.Unity
{
    public interface IInitializable
    {
        void Initialize();
    }

    public interface IPostInitializable
    {
        void PostInitialize();
    }

    public interface IStartable
    {
        void Start();
    }

    public interface IPostStartable
    {
        void PostStart();
    }

    public interface IFixedTickable
    {
        void FixedTick();
    }

    public interface IPostFixedTickable
    {
        void PostFixedTick();
    }

    public interface ITickable
    {
        void Tick();
    }

    public interface IPostTickable
    {
        void PostTick();
    }

    public interface ILateTickable
    {
        void LateTick();
    }

    public interface IPostLateTickable
    {
        void PostLateTick();
    }

#if VCONTAINER_UNITASK_INTEGRATION
    public interface IAsyncStartable
    {
        UniTask StartAsync(CancellationToken cancellation);
    }
#endif

    sealed class InitializationLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IInitializable> entries;
        bool disposed;

        public InitializationLoopItem(IEnumerable<IInitializable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.Initialize();
            }
            return false;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostInitializationLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IPostInitializable> entries;
        bool disposed;

        public PostInitializationLoopItem(IEnumerable<IPostInitializable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.PostInitialize();
            }
            return false;
        }

        public void Dispose() => disposed = true;
    }

    sealed class StartableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IStartable> entries;
        bool disposed;

        public StartableLoopItem(IEnumerable<IStartable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.Start();
            }
            return false;
        }

        public void Dispose() => disposed = true;
    }

    sealed class PostStartableLoopItem : IPlayerLoopItem, IDisposable
    {
        readonly IEnumerable<IPostStartable> entries;
        bool disposed;

        public PostStartableLoopItem(IEnumerable<IPostStartable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.PostStart();
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
        readonly CancellationTokenSource cts = new CancellationTokenSource();
        bool disposed;

        public AsyncStartableLoopItem(IEnumerable<IAsyncStartable> entries)
        {
            this.entries = entries;
        }

        public bool MoveNext()
        {
            if (disposed) return false;
            foreach (var x in entries)
            {
                x.StartAsync(cts.Token).Forget();
            }
            return false;
        }

        public void Dispose()
        {
            lock (entries)
            {
                if (disposed) return;
                cts.Cancel();
                cts.Dispose();
                disposed = true;
            }
        }
    }
#endif
}
