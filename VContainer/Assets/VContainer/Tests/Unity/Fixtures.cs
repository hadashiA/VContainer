using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    public class SampleEntryPoint :
        IInitializable,
        IPostInitializable,
        IStartable,
        IPostStartable,
        IFixedTickable,
        IPostFixedTickable,
        ITickable,
        IPostTickable,
        ILateTickable,
        IPostLateTickable
    {
        public bool InitializeCalled;
        public bool PostInitializeCalled;
        public bool StartCalled;
        public bool PostStartCalled;
        public int FixedTickCalls;
        public int PostFixedTickCalls;
        public int TickCalls;
        public int PostTickCalls;
        public int LateTickCalls;
        public int PostLateTickCalls;

        void IInitializable.Initialize() => InitializeCalled = true;
        void IPostInitializable.PostInitialize() => PostInitializeCalled = true;
        void IStartable.Start() => StartCalled = true;
        void IPostStartable.PostStart() => PostStartCalled = true;
        void IFixedTickable.FixedTick() => FixedTickCalls += 1;
        void IPostFixedTickable.PostFixedTick() => PostFixedTickCalls += 1;
        void ITickable.Tick() => TickCalls += 1;
        void IPostTickable.PostTick() => PostTickCalls += 1;
        void ILateTickable.LateTick() => LateTickCalls += 1;
        void IPostLateTickable.PostLateTick() => PostLateTickCalls += 1;
    }

    public class SampleAsyncEntryPoint : IAsyncStartable
    {
        public bool Started;

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            UnityEngine.Debug.Log("11111");
            await UniTask.Yield();
            UnityEngine.Debug.Log("awaited awaited awaited");
            Started = true;
        }
    }


    public class SampleAsyncEntryPointCancellable : IAsyncStartable
    {
        public bool Started;
        public bool Cancelled;

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            using (cancellation.Register(() => Cancelled = true))
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellation);
                Started = true;
            }
        }
    }
}
