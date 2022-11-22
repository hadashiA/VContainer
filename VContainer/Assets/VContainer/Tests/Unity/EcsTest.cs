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
#if VCONTAINER_ECS_INTEGRATION_1_0
            Assert.That(world.GetExistingSystemManaged<SystemB>(), Is.EqualTo(system));
#else
            Assert.That(world.GetExistingSystem<SystemB>(), Is.EqualTo(system));
#endif

            container.Dispose();
            Assert.That(world.IsCreated, Is.False);
            Assert.That(system.Disposed, Is.True);
        }
    }
}
#endif