using System.Collections;
using System.Collections.Generic;
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
            builder.RegisterEntryPoint<SampleEntryPoint>();

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
        public void RegisterEntryPointMultiple()
        {
            var builder = new ContainerBuilder();
            builder.RegisterEntryPoint<SampleEntryPoint>();
            builder.RegisterEntryPoint<SampleEntryPoint2>();
            var container = builder.Build();
            Assert.That(container.Resolve<IReadOnlyList<EntryPointDispatcher>>().Count, Is.EqualTo(1));
        }

        [Test]
        public void RegisterComponent()
        {
            var component = new GameObject("SampleBehaviour").AddComponent<SampleMonoBehaviour>();
            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Transient);
            builder.RegisterComponent(component);

            var container = builder.Build();

            Assert.That(container.Resolve<SampleMonoBehaviour>(), Is.EqualTo(component));
            Assert.That(container.Resolve<SampleMonoBehaviour>().ServiceA, Is.InstanceOf<ServiceA>());
        }

        [Test]
        public void RegisterComponentAsInterface()
        {
            var component = new GameObject("SampleBehaviour").AddComponent<SampleMonoBehaviour>();
            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Transient);
            builder.RegisterComponent<IComponent>(component);

            var container = builder.Build();

            Assert.That(container.Resolve<IComponent>(), Is.EqualTo(component));
        }

        [Test]
        public void RegisterComponentWithAutoInjection()
        {
            var component = new GameObject("SampleBehaviour").AddComponent<SampleMonoBehaviour>();
            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Transient);
            builder.RegisterComponent<IComponent>(component);

            var container = builder.Build();

            Assert.That(component.ServiceA, Is.InstanceOf<ServiceA>());
        }

        [Test]
        public void RegisterComponentInHierarchy()
        {
            var go1 = new GameObject("Parent");
            var go2 = new GameObject("Child A");
            var go3 = new GameObject("Child B");

            go3.transform.SetParent(go2.transform);
            go2.transform.SetParent(go1.transform);

            go3.AddComponent<SampleMonoBehaviour>();

            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.Register<ServiceA>(Lifetime.Transient);
                builder.RegisterComponentInHierarchy<SampleMonoBehaviour>();
            });

            var resolved = lifetimeScope.Container.Resolve<SampleMonoBehaviour>();

            Assert.That(resolved, Is.InstanceOf<SampleMonoBehaviour>());
            Assert.That(resolved.ServiceA, Is.InstanceOf<ServiceA>());
            Assert.That(resolved.transform, Is.EqualTo(go3.transform));
        }

        [Test]
        public void RegisterComponentInHierarchyAsInterfaces()
        {
            var go = new GameObject("Parent");
            go.AddComponent<SampleMonoBehaviour>();

            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.Register<ServiceA>(Lifetime.Transient);
                builder.RegisterComponentInHierarchy<SampleMonoBehaviour>()
                    .AsImplementedInterfaces();
            });

            var resolved = lifetimeScope.Container.Resolve<IComponent>();

            Assert.That(resolved, Is.InstanceOf<SampleMonoBehaviour>());
            Assert.That(((SampleMonoBehaviour)resolved).transform, Is.EqualTo(go.transform));
        }

        [Test]
        public void RegisterComponentInHierarchyUnderTransform()
        {
            var lifetimeScope = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();
            var go1 = new GameObject("Parent");
            var go2 = new GameObject("Child A");
            var go3 = new GameObject("Child B");

            go3.transform.SetParent(go2.transform);
            go2.transform.SetParent(go1.transform);

            var go1Component = go1.AddComponent<SampleMonoBehaviour>();
            var go2Component = go2.AddComponent<SampleMonoBehaviour>();

            {
                var builder = new ContainerBuilder { ApplicationOrigin = lifetimeScope };
                builder.Register<ServiceA>(Lifetime.Transient);
                builder.RegisterComponentInHierarchy<SampleMonoBehaviour>()
                    .UnderTransform(go2Component.transform);

                var container = builder.Build();
                var resolved = container.Resolve<SampleMonoBehaviour>();

                Assert.That(resolved, Is.InstanceOf<SampleMonoBehaviour>());
                Assert.That(resolved, Is.EqualTo(go2Component));
            }

            {
                var builder = new ContainerBuilder { ApplicationOrigin = lifetimeScope };
                builder.Register<ServiceA>(Lifetime.Transient);
                builder.RegisterComponentInHierarchy<SampleMonoBehaviour>()
                    .UnderTransform(go3.transform);

                Assert.Throws<VContainerException>(() =>
                {
                    var container = builder.Build();
                });
            }
        }

        [Test]
        public void RegisterComponentInHierarchyUnderInactive()
        {
            var lifetimeScope = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();
            var parent = new GameObject("Parent");
            var child = new GameObject("Child A");

            child.transform.SetParent(parent.transform);
            child.gameObject.SetActive(false);

            var component = child.AddComponent<SampleMonoBehaviour>();

            var builder = new ContainerBuilder { ApplicationOrigin = lifetimeScope };
            builder.Register<ServiceA>(Lifetime.Transient);
            builder.RegisterComponentInHierarchy<SampleMonoBehaviour>()
                .UnderTransform(parent.transform);

            var container = builder.Build();
            var resolved = container.Resolve<SampleMonoBehaviour>();
            Assert.That(resolved, Is.InstanceOf<SampleMonoBehaviour>());
            Assert.That(resolved.gameObject, Is.EqualTo(child));
        }

        [Test]
        public void RegisterComponentInHierarchyWithBuiltinType()
        {
            var go1 = new GameObject("Parent");
            go1.AddComponent<BoxCollider>();

            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.Register<ServiceA>(Lifetime.Transient);
                builder.RegisterComponentInHierarchy<BoxCollider>();
            });

            var resolved = lifetimeScope.Container.Resolve<BoxCollider>();
            Assert.That(resolved, Is.InstanceOf<BoxCollider>());
        }

        [Test]
        public void RegisterComponentInHierarchyAutoInject()
        {
            var go1 = new GameObject("SampleMonoBehaviour");
            var target = go1.AddComponent<SampleMonoBehaviour>();

            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.Register<ServiceA>(Lifetime.Transient);
                builder.RegisterComponentInHierarchy<SampleMonoBehaviour>()
                    .UnderTransform(go1.transform);
            });

            Assert.That(target.ServiceA, Is.InstanceOf<ServiceA>());
        }

        [Test]
        public void RegisterComponentOnNewGameObject()
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

        [Test]
        public void RegisterComponentOnNewGameObjectUnderTransform()
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
        }

        [Test]
        public void RegisterComponentInNewPrefab()
        {
            var prefab = new GameObject("Sample").AddComponent<SampleMonoBehaviour>();

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
            Assert.That(prefab.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void RegisterComponentInNewPrefabScoped()
        {
            var prefab = new GameObject("Sample").AddComponent<SampleMonoBehaviour>();

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
            Assert.That(prefab.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void RegisterComponentInNewPrefabWithAwake()
        {
            var prefab = new GameObject("Sample").AddComponent<SampleMonoBehaviour>();

            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Transient);
            builder.RegisterComponentInNewPrefab(prefab, Lifetime.Singleton);

            var container = builder.Build();
            var resolved1 = container.Resolve<SampleMonoBehaviour>();
            Assert.That(resolved1.gameObject.name, Is.EqualTo("Sample(Clone)"));
            Assert.That(resolved1, Is.InstanceOf<SampleMonoBehaviour>());
            Assert.That(resolved1.transform.parent, Is.Null);
            Assert.That(prefab.gameObject.activeSelf, Is.True);
            Assert.That(resolved1.gameObject.activeSelf, Is.True);
            Assert.That(resolved1.ServiceA, Is.InstanceOf<ServiceA>());
            Assert.That(resolved1.ServiceAInAwake, Is.InstanceOf<ServiceA>());
        }

        [Test]
        public void RegisterComponentInNewPrefabWithInActive()
        {
            var prefab = new GameObject("Sample").AddComponent<SampleMonoBehaviour>();
            prefab.gameObject.SetActive(false);

            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Transient);
            builder.RegisterComponentInNewPrefab(prefab, Lifetime.Singleton);

            var container = builder.Build();
            var resolved = container.Resolve<SampleMonoBehaviour>();
            Assert.That(resolved.gameObject.name, Is.EqualTo("Sample(Clone)"));
            Assert.That(resolved, Is.InstanceOf<SampleMonoBehaviour>());
            Assert.That(resolved.transform.parent, Is.Null);

            Assert.That(prefab.gameObject.activeSelf, Is.False);
            Assert.That(resolved.gameObject.activeSelf, Is.False);
            Assert.That(resolved.ServiceA, Is.InstanceOf<ServiceA>());
            Assert.That(resolved.ServiceAInAwake, Is.Null);
        }

        [Test]
        public void RegisterComponentInNewPrefabWithFailedResolve()
        {
            var prefab = new GameObject("Sample").AddComponent<SampleMonoBehaviour>();

            var builder = new ContainerBuilder();
            builder.RegisterComponentInNewPrefab(prefab, Lifetime.Singleton);

            var container = builder.Build();
            Assert.Catch(() =>
            {
                container.Resolve<SampleMonoBehaviour>();
            });
            Assert.That(prefab.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void RegisterComponentInNewPrefab_DontDestroyOnLoad()
        {
            var prefab = new GameObject("Sample").AddComponent<SampleMonoBehaviour>();

            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Transient);
            builder.RegisterComponentInNewPrefab(prefab, Lifetime.Singleton)
                .DontDestroyOnLoad();

            var container = builder.Build();
            var instance = container.Resolve<SampleMonoBehaviour>();
            Assert.That(instance.gameObject.scene.name, Is.EqualTo("DontDestroyOnLoad"));
        }

        [Test]
        public void UseComponentsWithParentTransform()
        {
            var go1 = new GameObject("Parent");
            var go2 = new GameObject("Child A");
            var go3 = new GameObject("Child B");

            go3.transform.SetParent(go2.transform);
            go2.transform.SetParent(go1.transform);

            go1.AddComponent<SampleMonoBehaviour>();
            go3.AddComponent<SampleMonoBehaviour>();

            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.Register<ServiceA>(Lifetime.Scoped);
                builder.Register<ServiceB>(Lifetime.Scoped);

                builder.UseComponents(go2.transform, components =>
                {
                    components.AddInHierarchy<SampleMonoBehaviour>();
                });

                builder.UseComponents(go3.transform, components =>
                {
                    components.AddOnNewGameObject<SampleMonoBehaviour2>(Lifetime.Scoped);
                });
            });

            var found = lifetimeScope.Container.Resolve<SampleMonoBehaviour>();
            Assert.That(found, Is.InstanceOf<SampleMonoBehaviour>());
            Assert.That(found.transform.parent, Is.EqualTo(go2.transform));

            var created = lifetimeScope.Container.Resolve<SampleMonoBehaviour2>();
            Assert.That(created, Is.InstanceOf<SampleMonoBehaviour2>());
            Assert.That(created.transform.parent, Is.EqualTo(go3.transform));
        }

        [UnityTest]
        public IEnumerator DispatchMonoBehaviour()
        {
            var component = new GameObject("SampleBehaviour").AddComponent<SampleMonoBehaviour>();
            using (LifetimeScope.Enqueue(bulder =>
            {
                bulder.Register<ServiceA>(Lifetime.Transient);
                bulder.RegisterComponent(component);
            }))
            {
                new GameObject("LifetimeScope").AddComponent<LifetimeScope>();
            }

            yield return null;

            Assert.That(component.ServiceA, Is.InstanceOf<ServiceA>());
            Assert.That(component.StartCalled, Is.True);
            Assert.That(component.UpdateCalls, Is.GreaterThanOrEqualTo(1));
        }
    }
}
