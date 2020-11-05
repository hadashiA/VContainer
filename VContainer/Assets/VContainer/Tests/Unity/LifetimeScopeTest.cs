using System.Collections;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
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

        [UnityTest]
        public IEnumerator ActivateEntryPoints()
        {
            using (LifetimeScope.Push(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>(Lifetime.Scoped).AsSelf();
            }))
            {
                var parentLifetimeScope = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();
                yield return null;

                var parentEntryPoint = parentLifetimeScope.Container.Resolve<SampleEntryPoint>();
                Assert.That(parentEntryPoint.InitializeCalled, Is.True);

                var childLifetimeScope = parentLifetimeScope.CreateChild();
                var childEntryPoint = childLifetimeScope.Container.Resolve<SampleEntryPoint>();
                yield return null;

                Assert.That(childEntryPoint.InitializeCalled, Is.True);
                Assert.That(childEntryPoint, Is.Not.EqualTo(parentEntryPoint));

                var scopeFactory = childLifetimeScope.Container.Resolve<IScopeFactory>();
                var instantScope = scopeFactory.CreateScope();
                yield return null;

                var instantEntryPoint = instantScope.Resolve<SampleEntryPoint>();
                Assert.That(instantEntryPoint.InitializeCalled, Is.True);
                Assert.That(instantEntryPoint, Is.Not.EqualTo(childEntryPoint));
            }
        }
    }
}