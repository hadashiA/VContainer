#if VCONTAINER_ECS_INTEGRATION
using System;
using System.Collections.Generic;
using Unity.Entities;
using NUnit.Framework;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    class SystemA : ComponentSystemBase
    {
        public bool UpdateCalled;
        public I2 Service;

        [Inject]
        public void Construct(I2 service) => Service = service;

        public override void Update() => UpdateCalled = true;
    }

    [DisableAutoCreation]
    class SystemB : ComponentSystemBase, IDisposable
    {
        public I2 Service;
        public bool UpdateCalled;
        public bool Disposed;

        public SystemB(I2 service) => Service = service;

        public override void Update() => UpdateCalled = true;
        public void Dispose() => Disposed = true;
    }
#if UNITY_2022_2_OR_NEWER
    
    
    [DisableAutoCreation]
    class SystemC : ComponentSystemBase, IDisposable
    {
        public I2 Service;
        public readonly UnmanagedSystemReference<UnmanagedSystemB> UnmanagedSystem;
        public bool UpdateCalled;
        public bool Disposed;

        public SystemC(I2 service, UnmanagedSystemReference<UnmanagedSystemB> systemA) 
        {
            UnmanagedSystem = systemA;
            Service = service;
        }

        public override void Update() => UpdateCalled = true;
        public void Dispose() => Disposed = true;
    }
    
    partial struct UnmanagedSystemA : ISystem
    {
        public bool UpdateCalled;
        public NoDependencyUnmanagedServiceA Service;

        [Inject]
        public void Construct(NoDependencyUnmanagedServiceA service) => Service = service;
        void ISystem.OnCreate(ref SystemState state) {}
        void ISystem.OnDestroy(ref SystemState state) {}
        void ISystem.OnUpdate(ref SystemState state) => UpdateCalled = true;
    }

    [DisableAutoCreation]
    partial struct UnmanagedSystemB : ISystem, IDisposable
    {
        public NoDependencyUnmanagedServiceA Service;
        public bool UpdateCalled;
        public bool Disposed;

        public UnmanagedSystemB(NoDependencyUnmanagedServiceA service) : this() => Service = service;
        void ISystem.OnCreate(ref SystemState state) {}
        void ISystem.OnDestroy(ref SystemState state) {}
        void ISystem.OnUpdate(ref SystemState state) => UpdateCalled = true;
        public void Dispose() => Disposed = true;
    }
#endif

    public class EcsTest
    {
        [Test]
        public void RegisterSystemFromDefaultWorld()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.RegisterSystemFromDefaultWorld<SystemA>();

            var container = builder.Build();
            var system = container.Resolve<SystemA>();
            Assert.That(system, Is.InstanceOf<SystemA>());
            Assert.That(system.Service, Is.InstanceOf<I2>());
        }
#if UNITY_2022_2_OR_NEWER
        [Test]
        public void RegisterUnmanagedSystemFromDefaultWorld()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<NoDependencyUnmanagedServiceA>(default);
            builder.RegisterUnmanagedSystemFromDefaultWorld<UnmanagedSystemA>();

            IObjectResolver container = builder.Build();
            var systemRef = container.Resolve<UnmanagedSystemReference<UnmanagedSystemA>>();
            Assert.That(systemRef, Is.InstanceOf<UnmanagedSystemReference<UnmanagedSystemA>>());
            ref UnmanagedSystemA system = ref systemRef.GetSystemFromDefaultWorld();
            Assert.That(system, Is.InstanceOf<UnmanagedSystemA>());
            Assert.That(system.Service, Is.InstanceOf<NoDependencyUnmanagedServiceA>());
        }
#endif
        
        [Test]
        public void RegisterSystemIntoDefaultWorld()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.RegisterSystemIntoDefaultWorld<SystemB>();

            var container = builder.Build();
            var system = container.Resolve<SystemB>();
            Assert.That(system.World, Is.EqualTo(World.DefaultGameObjectInjectionWorld));
        }
        
#if UNITY_2022_2_OR_NEWER
        [Test]
        public void RegisterUnmanagedSystemIntoDefaultWorld()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<NoDependencyUnmanagedServiceA>(default);
            builder.RegisterUnmanagedSystemIntoDefaultWorld<UnmanagedSystemB>();

            var container = builder.Build();
            var systemReference = container.Resolve<UnmanagedSystemReference<UnmanagedSystemB>>();
            Assert.That(systemReference.Id, Is.EqualTo(World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingUnmanagedSystem<UnmanagedSystemB>()));
        }
#endif
        [Test]
        public void RegisterNewWorld()
        {
            var builder = new ContainerBuilder();
            builder.RegisterNewWorld("My World 1", Lifetime.Scoped);
            builder.RegisterNewWorld("My World 2", Lifetime.Scoped);

            var container = builder.Build();
            var worlds = container.Resolve<IReadOnlyList<World>>();

            Assert.That(worlds[0].Name, Is.EqualTo("My World 1"));
            Assert.That(worlds[1].Name, Is.EqualTo("My World 2"));

            Assert.That(ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(worlds[0]), Is.True);
            Assert.That(ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(worlds[1]), Is.True);

            container.Dispose();

            Assert.That(worlds[0].IsCreated, Is.False);
            Assert.That(worlds[1].IsCreated, Is.False);
        }

        [Test]
        public void RegisterSystemIntoWorld()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.RegisterNewWorld("My World 1", Lifetime.Scoped);
            builder.RegisterSystemIntoWorld<SystemB>("My World 1")
                .IntoGroup<InitializationSystemGroup>();

            var container = builder.Build();
            var system = container.Resolve<SystemB>();
            var worldHelper = container.Resolve<WorldConfigurationHelper>();
            var world = worldHelper.World;

            Assert.That(world.IsCreated, Is.True);
            Assert.That(system.World, Is.EqualTo(world));
#if UNITY_2022_2_OR_NEWER
            Assert.That(world.GetExistingSystemManaged<SystemB>(), Is.EqualTo(system));
#else
            Assert.That(world.GetExistingSystem<SystemB>(), Is.EqualTo(system));
#endif

            container.Dispose();
            Assert.That(world.IsCreated, Is.False);
            Assert.That(system.Disposed, Is.True);
        }
#if UNITY_2022_2_OR_NEWER
        [Test]
        public void RegisterUnManagedSystemIntoWorld()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.RegisterInstance<NoDependencyUnmanagedServiceA>(default);
            builder.RegisterUnmanagedSystemIntoDefaultWorld<UnmanagedSystemB>();
            builder.RegisterNewWorld("My World 1", Lifetime.Scoped);
            builder.RegisterSystemIntoWorld<SystemC>("My World 1")
                .IntoGroup<InitializationSystemGroup>();

            var container = builder.Build();
            var system = container.Resolve<SystemC>();
            var worldHelper = container.Resolve<WorldConfigurationHelper>();
            worldHelper.SortSystems();
            var world = worldHelper.World;

            Assert.That(world.IsCreated, Is.True);
            Assert.That(system.World, Is.EqualTo(world));
            Assert.That(world.GetExistingSystemManaged<SystemC>(), Is.EqualTo(system));

            var systemRef = container.Resolve<UnmanagedSystemReference<UnmanagedSystemB>>();
            Assert.That(systemRef, Is.InstanceOf<UnmanagedSystemReference<UnmanagedSystemB>>());
            Assert.That(systemRef.Id, Is.EqualTo(system.UnmanagedSystem.Id));
            
            container.Dispose();
            Assert.That(world.IsCreated, Is.False);
            Assert.That(system.Disposed, Is.True);
            
        }
#endif
    }
}
#endif
