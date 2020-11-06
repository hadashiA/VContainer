using UnityEngine;
using NUnit.Framework;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    public class LifetimeScopeTest
    {
        [Test]
        public void PushParent()
        {
            var parent = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();

            using (LifetimeScope.PushParent(parent))
            {
                var child = new GameObject("LifetimeScope Child 1").AddComponent<LifetimeScope>();
                Assert.That(child.Parent, Is.EqualTo(parent));
            }

            var child2 = new GameObject("LifetimeScope Child 2").AddComponent<LifetimeScope>();
            Assert.That(child2.Parent, Is.Null);
        }

        [Test]
        public void CreateChild()
        {
            LifetimeScope parentLifetimeScope;

            using (LifetimeScope.Push(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>(Lifetime.Scoped).AsSelf();
            }))
            {
                parentLifetimeScope = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();
            }

            var parentEntryPoint = parentLifetimeScope.Container.Resolve<SampleEntryPoint>();
            Assert.That(parentEntryPoint, Is.Not.Null);

            var childLifetimeScope = parentLifetimeScope.CreateChild();
            var childEntryPoint = childLifetimeScope.Container.Resolve<SampleEntryPoint>();

            Assert.That(childEntryPoint, Is.Not.Null);
            Assert.That(childEntryPoint, Is.Not.EqualTo(parentEntryPoint));

            var scopeFactory = childLifetimeScope.Container.Resolve<IScopeFactory>();
            var instantScope = scopeFactory.CreateScope();
            var instantEntryPoint = instantScope.Container.Resolve<SampleEntryPoint>();

            Assert.That(instantEntryPoint, Is.Not.Null);
            Assert.That(instantEntryPoint, Is.Not.EqualTo(childEntryPoint));
        }

        [Test]
        public void CreateScopeWithSingleton()
        {
            LifetimeScope parentLifetimeScope;

            using (LifetimeScope.Push(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>(Lifetime.Singleton).AsSelf();
            }))
            {
                parentLifetimeScope = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();
            }

            var parentEntryPoint = parentLifetimeScope.Container.Resolve<SampleEntryPoint>();
            Assert.That(parentEntryPoint, Is.InstanceOf<SampleEntryPoint>());

            var childLifetimeScope = parentLifetimeScope.CreateChild();
            var childEntryPoint = childLifetimeScope.Container.Resolve<SampleEntryPoint>();

            Assert.That(childEntryPoint, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(childEntryPoint, Is.EqualTo(parentEntryPoint));

            var scopeFactory = childLifetimeScope.Container.Resolve<IScopeFactory>();
            var instantScope = scopeFactory.CreateScope();
            var instantEntryPoint = instantScope.Container.Resolve<SampleEntryPoint>();
            Assert.That(instantEntryPoint, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(instantEntryPoint, Is.EqualTo(childEntryPoint));
        }
    }
}
