#if VCONTAINER_ECS_INTEGRATION
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using NUnit.Framework;
using UnityEngine.TestTools;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    class SystemA : ComponentSystemBase
    {
        public bool UpdateCalled;

        public override void Update() => UpdateCalled = true;
    }

    [DisableAutoCreation]
    class SystemB : ComponentSystemBase, IDisposable
    {
        public bool UpdateCalled;
        public bool Disposed;

        public override void Update() => UpdateCalled = true;
        public void Dispose() => Disposed = true;
    }

    public class EcsTest
    {
        [Test]
        public void RegisterSystemFromDefaultWorld()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSystemFromDefaultWorld<SystemA>();

            var container = builder.Build();
            var system = container.Resolve<SystemA>();
            Assert.That(system, Is.InstanceOf<SystemA>());
        }

        [Test]
        public void RegisterNewWorld()
        {
            var builder = new ContainerBuilder();
            builder.RegisterNewWorld("My World", Lifetime.Scoped);

            var container = builder.Build();
            var world = container.Resolve<World>();

            Assert.That(world.Name, Is.EqualTo("My World"));
            Assert.That(ScriptBehaviourUpdateOrder.IsWorldInPlayerLoop(world), Is.True);

            container.Dispose();

            Assert.That(world.IsCreated, Is.False);
        }

        [UnityTest]
        public IEnumerator RegisterSystemIntoWorld()
        {
            var builder = new ContainerBuilder();
            builder.RegisterNewWorld("My World 1", Lifetime.Scoped);
            builder.RegisterSystemIntoWorld<SystemB>("My World 1");

            var container = builder.Build();
            var world = container.Resolve<World>();
            var system = container.Resolve<SystemB>();

            Assert.That(system.World, Is.EqualTo(world));

            yield return null;

            Assert.That(system.UpdateCalled, Is.True);

            container.Dispose();
            Assert.That(system.Disposed, Is.True);
        }
    }
}
#endif