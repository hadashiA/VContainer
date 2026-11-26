using NUnit.Framework;
using VContainer.Internal;

namespace VContainer.Tests
{
    class CycleLeaf { }
    class CycleSharedMid { public CycleSharedMid(CycleLeaf leaf) { } }
    class CycleRootA { public CycleRootA(CycleSharedMid mid) { } }
    class CycleRootB { public CycleRootB(CycleSharedMid mid, CycleLeaf leaf) { } }
    class CycleOne { public CycleOne(CycleSharedMid mid, CycleTwo two) { } }
    class CycleTwo { public CycleTwo(CycleOne one) { } }

    class CycleEntry { public CycleEntry(CycleGate gate) { } }
    class CycleGate { public CycleGate(CycleLoopA a) { } }
    class CycleLoopA { public CycleLoopA(CycleLoopB b) { } }
    class CycleLoopB { public CycleLoopB(CycleLoopA a) { } }

    class Chain0 { }
    class Chain1 { public Chain1(Chain0 x, Chain0 y) { } }
    class Chain2 { public Chain2(Chain1 x, Chain1 y) { } }
    class Chain3 { public Chain3(Chain2 x, Chain2 y) { } }
    class Chain4 { public Chain4(Chain3 x, Chain3 y) { } }
    class Chain5 { public Chain5(Chain4 x, Chain4 y) { } }

    [TestFixture]
    public class CircularDependencyCheckTest
    {
        [Test]
        public void CycleThroughSharedAcyclicType_Throws()
        {
            var builder = new ContainerBuilder();
            builder.Register<CycleLeaf>(Lifetime.Singleton);
            builder.Register<CycleSharedMid>(Lifetime.Singleton);
            builder.Register<CycleRootA>(Lifetime.Singleton);
            builder.Register<CycleRootB>(Lifetime.Singleton);
            builder.Register<CycleOne>(Lifetime.Singleton);
            builder.Register<CycleTwo>(Lifetime.Singleton);

            // Only reflection mode can detect circular dependency errors at runtime.
            if (InjectorCache.GetOrBuild(typeof(CycleOne)) is ReflectionInjector)
            {
                Assert.Throws<VContainerException>(() => builder.Build());
            }
        }

        [Test]
        public void CycleBelowRootOutsideIt_Throws()
        {
            var builder = new ContainerBuilder();
            builder.Register<CycleEntry>(Lifetime.Singleton);
            builder.Register<CycleGate>(Lifetime.Singleton);
            builder.Register<CycleLoopA>(Lifetime.Singleton);
            builder.Register<CycleLoopB>(Lifetime.Singleton);

            // Only reflection mode can detect circular dependency errors at runtime.
            if (InjectorCache.GetOrBuild(typeof(CycleLoopA)) is ReflectionInjector)
            {
                Assert.Throws<VContainerException>(() => builder.Build());
            }
        }

        [Test]
        public void SharedChainWithoutCycle_DoesNotThrow()
        {
            var builder = new ContainerBuilder();
            builder.Register<Chain0>(Lifetime.Transient);
            builder.Register<Chain1>(Lifetime.Transient);
            builder.Register<Chain2>(Lifetime.Transient);
            builder.Register<Chain3>(Lifetime.Transient);
            builder.Register<Chain4>(Lifetime.Transient);
            builder.Register<Chain5>(Lifetime.Transient);

            Assert.DoesNotThrow(() => builder.Build());
        }
    }
}
