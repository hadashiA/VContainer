using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    public sealed class SampleEntryPoint :
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
}
