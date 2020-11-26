using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    [TestFixture]
    public class UnityObjectResolverTest
    {
        [Test]
        public void InjectGameObject()
        {
            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Singleton);
            var container = builder.Build();

            var gameObject = new GameObject();
            var component = gameObject.AddComponent<SampleMonoBehaviour>();

            Assert.That(component.ServiceA, Is.Null);

            container.InjectGameObject(gameObject);
            Assert.That(component.ServiceA, Is.InstanceOf<ServiceA>());
        }

        [Test]
        public void InjectGameObjectAllMonoBehaviour()
        {
            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Singleton);
            builder.Register<ServiceB>(Lifetime.Singleton);
            var container = builder.Build();

            var gameObject = new GameObject();
            var component1 = gameObject.AddComponent<SampleMonoBehaviour>();
            var component2 = gameObject.AddComponent<SampleMonoBehaviour2>();

            Assert.That(component1.ServiceA, Is.Null);
            Assert.That(component2.ServiceB, Is.Null);

            container.InjectGameObject(gameObject);
            Assert.That(component1.ServiceA, Is.InstanceOf<ServiceA>());
            Assert.That(component2.ServiceB, Is.InstanceOf<ServiceB>());
        }

        [Test]
        public void InjectGameObjectWithChildren()
        {
            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Singleton);
            builder.Register<ServiceB>(Lifetime.Singleton);
            var container = builder.Build();

            var gameObject = new GameObject("Parent");
            var component1 = gameObject.AddComponent<SampleMonoBehaviour>();
            var component2 = gameObject.AddComponent<SampleMonoBehaviour2>();

            var child1 = new GameObject("Child 1");
            child1.transform.SetParent(gameObject.transform);

            var child2 = new GameObject("Child 2");
            child2.transform.SetParent(child1.transform);
            var child2Component = child2.AddComponent<SampleMonoBehaviour>();

            container.InjectGameObject(gameObject);
            Assert.That(component1.ServiceA, Is.InstanceOf<ServiceA>());
            Assert.That(component2.ServiceB, Is.InstanceOf<ServiceB>());
            Assert.That(child2Component.ServiceA, Is.InstanceOf<ServiceA>());
        }
    }
}