using NUnit.Framework;

namespace VContainer.Tests
{
    [TestFixture]
    public class ContainerBuilderTest
    {
        [Test]
        public void Exists()
        {
            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            Assert.That(builder.Exists(typeof(ServiceA)), Is.True);
            Assert.That(builder.Exists(typeof(I4)), Is.False);
            Assert.That(builder.Exists(typeof(I4), includeInterfaceTypes: true), Is.True);
        }

        [Test]
        public void Exists_FromChild()
        {
            var builder = new ContainerBuilder();
            builder.Register<ServiceA>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            var parentScope = builder.Build();
            var childScope = parentScope.CreateScope(childBuilder =>
            {
                childBuilder.Register<ServiceB>(Lifetime.Singleton);

                Assert.That(childBuilder.Exists(typeof(ServiceA)), Is.False);
                Assert.That(childBuilder.Exists(typeof(I4)), Is.False);
                Assert.That(childBuilder.Exists(typeof(I4), includeInterfaceTypes: true), Is.False);

                Assert.That(childBuilder.Exists(typeof(ServiceA), findParentScopes: true), Is.True);
                Assert.That(childBuilder.Exists(typeof(I4), findParentScopes: true), Is.False);
                Assert.That(childBuilder.Exists(typeof(I4), findParentScopes: true, includeInterfaceTypes: true), Is.True);

                Assert.That(childBuilder.Exists(typeof(ServiceB)), Is.True);
            });
       }
    }
}