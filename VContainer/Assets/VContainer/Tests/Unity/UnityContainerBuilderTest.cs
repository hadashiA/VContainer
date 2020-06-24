using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    [TestFixture]
    public class UnityContainerBuilderTest
    {
        [Test]
        public void RegisterComponentInHierarchy()
        {
            var lifetimeScope = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();
            var go1 = new GameObject("Parent");
            var go2 = new GameObject("Child A");
            var go3 = new GameObject("Child B");

            go3.transform.SetParent(go2.transform);
            go2.transform.SetParent(go1.transform);

            go3.AddComponent<SampleMonoBehaviour>();

            var builder = new ContainerBuilder { ApplicationOrigin = lifetimeScope };
            builder.RegisterComponentInHierarchy<SampleMonoBehaviour>();

            var container = builder.Build();
            var resolved = container.Resolve<SampleMonoBehaviour>();
            Assert.That(resolved, Is.InstanceOf<SampleMonoBehaviour>());
            Assert.That(resolved.gameObject, Is.EqualTo(go3));
        }

        [Test]
        public void RegisterComponentOnNewGameObject()
        {
            {
                var go1 = new GameObject("Parent");
                var builder = new ContainerBuilder();
                builder.RegisterComponentOnNewGameObject<SampleMonoBehaviour>(Lifetime.Transient, "hoge")
                    .UnderTransform(go1.transform);

                var container = builder.Build();
                var resolved1 = container.Resolve<SampleMonoBehaviour>();
                var resolved2 = container.Resolve<SampleMonoBehaviour>();
                Assert.That(resolved1.gameObject.name, Is.EqualTo("hoge"));
                Assert.That(resolved2.gameObject.name, Is.EqualTo("hoge"));
                Assert.That(resolved1, Is.InstanceOf<SampleMonoBehaviour>());
                Assert.That(resolved2, Is.InstanceOf<SampleMonoBehaviour>());
                Assert.That(resolved1.transform.parent, Is.EqualTo(go1.transform));
                Assert.That(resolved2.transform.parent, Is.EqualTo(go1.transform));
                Assert.That(resolved1, Is.Not.EqualTo(resolved2));
            }

            {
                var builder = new ContainerBuilder();
                builder.RegisterComponentOnNewGameObject<SampleMonoBehaviour>(Lifetime.Scoped, "hoge");

                var container = builder.Build();
                var resolved1 = container.Resolve<SampleMonoBehaviour>();
                var resolved2 = container.Resolve<SampleMonoBehaviour>();
                Assert.That(resolved1.gameObject.name, Is.EqualTo("hoge"));
                Assert.That(resolved2.gameObject.name, Is.EqualTo("hoge"));
                Assert.That(resolved1, Is.InstanceOf<SampleMonoBehaviour>());
                Assert.That(resolved2, Is.InstanceOf<SampleMonoBehaviour>());
                Assert.That(resolved1.transform.parent, Is.Null);
                Assert.That(resolved2.transform.parent, Is.Null);
                Assert.That(resolved1, Is.EqualTo(resolved2));
            }
        }


        [Test]
        public void RegisterComponentInNewPrefab()
        {
            var prefab = new GameObject("Sample").AddComponent<SampleMonoBehaviour>();
            {
                var go1 = new GameObject("Parent");
                var builder = new ContainerBuilder();
                builder.RegisterComponentInNewPrefab(prefab, Lifetime.Transient)
                    .UnderTransform(go1.transform);

                var container = builder.Build();
                var resolved1 = container.Resolve<SampleMonoBehaviour>();
                var resolved2 = container.Resolve<SampleMonoBehaviour>();
                Assert.That(resolved1.gameObject.name, Is.EqualTo("Sample(Clone)"));
                Assert.That(resolved2.gameObject.name, Is.EqualTo("Sample(Clone)"));
                Assert.That(resolved1, Is.InstanceOf<SampleMonoBehaviour>());
                Assert.That(resolved2, Is.InstanceOf<SampleMonoBehaviour>());
                Assert.That(resolved1.transform.parent, Is.EqualTo(go1.transform));
                Assert.That(resolved2.transform.parent, Is.EqualTo(go1.transform));
                Assert.That(resolved1, Is.Not.EqualTo(resolved2));
            }

            {
                var builder = new ContainerBuilder();
                builder.RegisterComponentInNewPrefab(prefab, Lifetime.Scoped);

                var container = builder.Build();
                var resolved1 = container.Resolve<SampleMonoBehaviour>();
                var resolved2 = container.Resolve<SampleMonoBehaviour>();
                Assert.That(resolved1.gameObject.name, Is.EqualTo("Sample(Clone)"));
                Assert.That(resolved2.gameObject.name, Is.EqualTo("Sample(Clone)"));
                Assert.That(resolved1, Is.InstanceOf<SampleMonoBehaviour>());
                Assert.That(resolved2, Is.InstanceOf<SampleMonoBehaviour>());
                Assert.That(resolved1.transform.parent, Is.Null);
                Assert.That(resolved2.transform.parent, Is.Null);
                Assert.That(resolved1, Is.EqualTo(resolved2));
            }
        }
    }
}
