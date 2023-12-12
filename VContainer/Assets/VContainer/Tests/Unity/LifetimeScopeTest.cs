using System.Collections;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    public class LifetimeScopeTest
    {
        [UnityTest]
        public IEnumerator Create()
        {
            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>().AsSelf();
                builder.Register<DisposableServiceA>(Lifetime.Scoped);
            });

            yield return null;
            yield return null;

            var entryPoint = lifetimeScope.Container.Resolve<SampleEntryPoint>();
            Assert.That(entryPoint, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(entryPoint.InitializeCalled, Is.EqualTo(1));
            Assert.That(entryPoint.TickCalls, Is.EqualTo(2));

            var disposable = lifetimeScope.Container.Resolve<DisposableServiceA>();
            lifetimeScope.Dispose();

            yield return null;
            Assert.That(disposable.Disposed, Is.True);
            Assert.That(entryPoint.InitializeCalled, Is.EqualTo(1));
            Assert.That(entryPoint.TickCalls, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator CreateChild()
        {
            var parentLifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>().AsSelf();
            });

            yield return null;
            yield return null;

            var parentEntryPoint = parentLifetimeScope.Container.Resolve<SampleEntryPoint>();
            var parentLifetimeScopeFromContainer = parentLifetimeScope.Container.Resolve<LifetimeScope>();

            Assert.That(parentLifetimeScopeFromContainer, Is.EqualTo(parentLifetimeScope));
            Assert.That(parentLifetimeScopeFromContainer.transform.childCount, Is.Zero);
            Assert.That(parentEntryPoint, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(parentEntryPoint.InitializeCalled, Is.EqualTo(1));
            Assert.That(parentEntryPoint.TickCalls, Is.EqualTo(2));

            var childLifetimeScope = parentLifetimeScope.CreateChild(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>().AsSelf();
                builder.Register<DisposableServiceA>(Lifetime.Scoped);
            });

            yield return null;
            yield return null;

            var childEntryPoint = childLifetimeScope.Container.Resolve<SampleEntryPoint>();
            var childDisposable = childLifetimeScope.Container.Resolve<DisposableServiceA>();
            var childLifetimeScopeFromContainer = childLifetimeScope.Container.Resolve<LifetimeScope>();

            Assert.That(childLifetimeScopeFromContainer, Is.EqualTo(childLifetimeScope));
            Assert.That(childEntryPoint, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(childEntryPoint, Is.Not.EqualTo(parentEntryPoint));
            Assert.That(childEntryPoint.InitializeCalled, Is.EqualTo(1));
            Assert.That(childEntryPoint.TickCalls, Is.EqualTo(2));

            childLifetimeScope.Dispose();
            yield return null;
            Assert.That(childDisposable.Disposed, Is.True);
            Assert.That(childLifetimeScope == null, Is.True);
            Assert.That(parentLifetimeScope.transform.childCount, Is.Zero);
        }

        [Test]
        public void CreateChildFromPrefab()
        {
            var parent = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>().AsSelf();
            });

            var childPrefab = new GameObject("Child").AddComponent<SampleChildLifetimeScope>();

            var child = parent.CreateChildFromPrefab(childPrefab);

            var parentResolved = parent.Container.Resolve<SampleEntryPoint>();
            var childResolved = child.Container.Resolve<SampleEntryPoint>();
            Assert.That(parentResolved, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(childResolved, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(childResolved, Is.EqualTo(parentResolved));
            Assert.That(child.parentReference.Object, Is.EqualTo(parent));
            Assert.That(childPrefab.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void CreateChildFromPrefabWithInActive()
        {
            var parent = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>().AsSelf();
            });

            var childPrefab = new GameObject("Child").AddComponent<SampleChildLifetimeScope>();
            childPrefab.gameObject.SetActive(false);

            var child = parent.CreateChildFromPrefab(childPrefab);
            Assert.That(childPrefab.gameObject.activeSelf, Is.False);
            Assert.That(child.gameObject.activeSelf, Is.False);

            child.Build();

            var parentResolved = parent.Container.Resolve<SampleEntryPoint>();
            var childResolved = child.Container.Resolve<SampleEntryPoint>();
            Assert.That(parentResolved, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(childResolved, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(childResolved, Is.EqualTo(parentResolved));
            Assert.That(child.parentReference.Object, Is.EqualTo(parent));
        }

        [Test]
        public void CreateScopeWithSingleton()
        {
            var parentLifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>().AsSelf();
            });

            var parentEntryPoint = parentLifetimeScope.Container.Resolve<SampleEntryPoint>();
            Assert.That(parentEntryPoint, Is.InstanceOf<SampleEntryPoint>());

            var childLifetimeScope = parentLifetimeScope.CreateChild();
            var childEntryPoint = childLifetimeScope.Container.Resolve<SampleEntryPoint>();

            Assert.That(childEntryPoint, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(childEntryPoint, Is.EqualTo(parentEntryPoint));
        }

        [Test]
        public void EnqueueParent()
        {
            var parent = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();

            using (LifetimeScope.EnqueueParent(parent))
            {
                var child = new GameObject("LifetimeScope Child 1").AddComponent<LifetimeScope>();
                Assert.That(child.Parent, Is.EqualTo(parent));
            }

            var child2 = new GameObject("LifetimeScope Child 2").AddComponent<LifetimeScope>();
            Assert.That(child2.Parent, Is.Null);
        }

        [Test]
        public void EnqueueParent_Multiply()
        {
            var parent1 = new GameObject("LifetimeScope Parent 1").AddComponent<LifetimeScope>();

            using (LifetimeScope.EnqueueParent(parent1))
            {
                var parent2 = new GameObject("LifetimeScope Parent 2").AddComponent<LifetimeScope>();
                Assert.That(parent2.Parent, Is.EqualTo(parent1));

                using (LifetimeScope.EnqueueParent(parent2))
                {
                    var child = new GameObject("LifetimeScope Child").AddComponent<LifetimeScope>();
                    Assert.That(child.Parent, Is.EqualTo(parent2));
                }
            }
        }

        [Test]
        public void EnqueueExtraInstaller()
        {
            using (LifetimeScope.Enqueue(builder => builder.Register<NoDependencyServiceA>(Lifetime.Scoped)))
            {
                var scope1 = new GameObject("LifetimeScope 1").AddComponent<LifetimeScope>();
                Assert.That(scope1.Container.Resolve<NoDependencyServiceA>(), Is.InstanceOf<NoDependencyServiceA>());

                using (LifetimeScope.Enqueue(builder => builder.Register<NoDependencyServiceB>(Lifetime.Scoped)))
                {
                    var scope2 = new GameObject("LifetimeScope 2").AddComponent<LifetimeScope>();
                    Assert.That(scope2.Container.Resolve<NoDependencyServiceA>(), Is.InstanceOf<NoDependencyServiceA>());
                    Assert.That(scope2.Container.Resolve<NoDependencyServiceB>(), Is.InstanceOf<NoDependencyServiceB>());
                }

                var scope3 = new GameObject("LifetimeScope 3").AddComponent<LifetimeScope>();
                Assert.That(scope3.Container.Resolve<NoDependencyServiceA>(), Is.InstanceOf<NoDependencyServiceA>());
                Assert.Throws<VContainerException>(() =>
                {
                    scope3.Container.Resolve<NoDependencyServiceB>();
                });
            }

            var scope4 = new GameObject("LifetimeScope 4").AddComponent<LifetimeScope>();
            Assert.Throws<VContainerException>(() =>
            {
                scope4.Container.Resolve<NoDependencyServiceA>();
            });
            Assert.Throws<VContainerException>(() =>
            {
                scope4.Container.Resolve<NoDependencyServiceB>();
            });
        }

        [Test]
        [Ignore("Resources/ will be included in the build and we want to use a different approch")]
        public void ParentTypeReference()
        {
            var prefab = Resources.Load<GameObject>("ParentChildRelationship");
            var root = UnityEngine.Object.Instantiate(prefab);
            var parent = root.GetComponentInChildren<SampleLifetimeScope>();
            var child = root.GetComponentInChildren<SampleChildLifetimeScope>();

            Assert.That(parent != null, Is.True);
            Assert.That(child != null, Is.True);
            Assert.That(child.Parent, Is.EqualTo(parent));
        }

        // [Test]
        // public void ParentTypeReferenceInvalid()
        // {
        //     var prefab = Resources.Load<GameObject>("ParentChildRelationshipInvalid");
        //     Assert.Throws<LifetimeScope.ParentTypeNotFoundException>(() =>
        //     {
        //         UnityEngine.Object.Instantiate(prefab);
        //     });
        // }
    }
}
