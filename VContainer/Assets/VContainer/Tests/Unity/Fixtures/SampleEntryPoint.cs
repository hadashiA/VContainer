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
        public int InitializeCalled;
        public int PostInitializeCalled;
        public int StartCalled;
        public int PostStartCalled;
        public int FixedTickCalls;
        public int PostFixedTickCalls;
        public int TickCalls;
        public int PostTickCalls;
        public int LateTickCalls;
        public int PostLateTickCalls;

        void IInitializable.Initialize() => InitializeCalled += 1;
        void IPostInitializable.PostInitialize() => PostInitializeCalled += 1;
        void IStartable.Start() => StartCalled += 1;
        void IPostStartable.PostStart() => PostStartCalled += 1;
        void IFixedTickable.FixedTick() => FixedTickCalls += 1;
        void IPostFixedTickable.PostFixedTick() => PostFixedTickCalls += 1;
        void ITickable.Tick() => TickCalls += 1;
        void IPostTickable.PostTick() => PostTickCalls += 1;
        void ILateTickable.LateTick() => LateTickCalls += 1;
        void IPostLateTickable.PostLateTick() => PostLateTickCalls += 1;
    }

    public class SampleEntryPoint2 :
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
        public int InitializeCalled;
        public int PostInitializeCalled;
        public int StartCalled;
        public int PostStartCalled;
        public int FixedTickCalls;
        public int PostFixedTickCalls;
        public int TickCalls;
        public int PostTickCalls;
        public int LateTickCalls;
        public int PostLateTickCalls;

        void IInitializable.Initialize() => InitializeCalled += 1;
        void IPostInitializable.PostInitialize() => PostInitializeCalled += 1;
        void IStartable.Start() => StartCalled += 1;
        void IPostStartable.PostStart() => PostStartCalled += 1;
        void IFixedTickable.FixedTick() => FixedTickCalls += 1;
        void IPostFixedTickable.PostFixedTick() => PostFixedTickCalls += 1;
        void ITickable.Tick() => TickCalls += 1;
        void IPostTickable.PostTick() => PostTickCalls += 1;
        void ILateTickable.LateTick() => LateTickCalls += 1;
        void IPostLateTickable.PostLateTick() => PostLateTickCalls += 1;
    }

    public class InitializableThrowable : IInitializable
    {
        public void Initialize()
        {
            throw new System.NotImplementedException();
        }
    }

    public class PostInitializableThrowable : IInitializable
    {
        public void Initialize()
        {
            throw new System.NotImplementedException();
        }
    }

    public class StartableThrowable : IStartable
    {
        public void Start()
        {
            throw new System.NotImplementedException();
        }
    }

    public class PostStartableThrowable : IPostStartable
    {
        public void PostStart()
        {
            throw new System.NotImplementedException();
        }
    }

    public class FixedTickableThrowable : IFixedTickable
    {
        public void FixedTick()
        {
            throw new System.NotImplementedException();
        }
    }

    public class PostFixedTickableThrowable : IPostFixedTickable
    {
        public void PostFixedTick()
        {
            throw new System.NotImplementedException();
        }
    }

    public class TickableThrowable : ITickable
    {
        public void Tick()
        {
            throw new System.NotImplementedException();
        }
    }

    public class PostTickableThrowable : IPostTickable
    {
        public void PostTick()
        {
            throw new System.NotImplementedException();
        }
    }

    public class LateTickableThrowable : ILateTickable
    {
        public void LateTick()
        {
            throw new System.NotImplementedException();
        }
    }

    public class PostLateTickableThrowable : IPostLateTickable
    {
        public void PostLateTick()
        {
            throw new System.NotImplementedException();
        }
    }
}
