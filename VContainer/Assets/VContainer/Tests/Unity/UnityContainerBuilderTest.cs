using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    [TestFixture]
    public class UnityContainerBuilderTest
    {
        [Test]
        public void RegisterEntryPoint()
        {
            var builder = new ContainerBuilder();
            builder.RegisterEntryPoint<SampleEntryPoint>(Lifetime.Singleton);

            var container = builder.Build();
            Assert.That(container.Resolve<IInitializable>(), Is.InstanceOf<IInitializable>());
            Assert.That(container.Resolve<IPostInitializable>(), Is.InstanceOf<IPostInitializable>());
            Assert.That(container.Resolve<IFixedTickable>(), Is.InstanceOf<IFixedTickable>());
            Assert.That(container.Resolve<IPostFixedTickable>(), Is.InstanceOf<IPostFixedTickable>());
            Assert.That(container.Resolve<ITickable>(), Is.InstanceOf<ITickable>());
            Assert.That(container.Resolve<IPostTickable>(), Is.InstanceOf<IPostTickable>());
            Assert.That(container.Resolve<ILateTickable>(), Is.InstanceOf<ILateTickable>());
            Assert.That(container.Resolve<IPostLateTickable>(), Is.InstanceOf<IPostLateTickable>());
        }

        [Test]
        public void RegisterComponent()
        {
            var component = new GameObject("SampleBehaviour").AddComponent<SampleMonoBehaviour>();
            {
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Transient);
                builder.RegisterComponent(component);

                var container = builder.Build();

                Assert.That(container.Resolve<SampleMonoBehaviour>(), Is.EqualTo(component));
                Assert.That(container.Resolve<MonoBehaviour>(), Is.InstanceOf<MonoBehaviour>());
            }
            {
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Transient);
                builder.RegisterComponent<IComponent>(component);

                var container = builder.Build();

                Assert.That(container.Resolve<IComponent>(), Is.EqualTo(component));
                Assert.That(container.Resolve<MonoBehaviour>(), Is.InstanceOf<MonoBehaviour>());
            }
        }

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
            builder.Register<ServiceA>(Lifetime.Transient);
            builder.RegisterComponentInHierarchy<SampleMonoBehaviour>();

            var container = builder.Build();
            var resolved = container.Resolve<SampleMonoBehaviour>();
            Assert.That(resolved, Is.InstanceOf<SampleMonoBehaviour>());
            Assert.That(resolved.transform, Is.EqualTo(go3.transform));

            Assert.That(container.Resolve<MonoBehaviour>(), Is.InstanceOf<MonoBehaviour>());
        }

        [Test]
        public void RegisterComponentOnNewGameObject()
        {
            {
                var go1 = new GameObject("Parent");
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Transient);
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

                Assert.That(container.Resolve<MonoBehaviour>(), Is.InstanceOf<MonoBehaviour>());
            }

            {
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Transient);
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


            {
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Transient);
                builder.RegisterComponentOnNewGameObject<SampleMonoBehaviour>(Lifetime.Scoped, "hoge");

                var container = builder.Build();
                Assert.That(container.Resolve<MonoBehaviour>(), Is.InstanceOf<MonoBehaviour>());
            }
        }

        [Test]
        public void RegisterComponentInNewPrefab()
        {
            var prefab = new GameObject("Sample").AddComponent<SampleMonoBehaviour>();
            {
                var go1 = new GameObject("Parent");
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Transient);
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
                builder.Register<ServiceA>(Lifetime.Transient);
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

            {
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Transient);
                builder.RegisterComponentInNewPrefab(prefab, Lifetime.Scoped);

                var container = builder.Build();
                Assert.That(container.Resolve<MonoBehaviour>(), Is.InstanceOf<MonoBehaviour>());
            }
        }

        [UnityTest]
        public IEnumerator DispatchMonoBehaviour()
        {
            var lifetimeScope = default(LifetimeScope);
            var component = new GameObject("SampleBehaviour").AddComponent<SampleMonoBehaviour>();
            using (LifetimeScope.Push(bulder =>
            {
                bulder.Register<ServiceA>(Lifetime.Transient);
                bulder.RegisterComponent(component);
            }))
            {
                lifetimeScope = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();
            }

            yield return null;

            Assert.That(component.ServiceA, Is.InstanceOf<ServiceA>());
            Assert.That(component.StartCalled, Is.True);
            Assert.That(component.UpdateCalls, Is.GreaterThanOrEqualTo(1));
        }
    }
}
