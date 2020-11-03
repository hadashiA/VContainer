using NUnit.Framework;

namespace VContainer.Tests
{
    [TestFixture]
    public class ScopeFactoryTest
    {
        [Test]
        public void ResolveScopeFactory()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();

            var scopeFactory1 = container.Resolve<IScopeFactory>();
            Assert.That(scopeFactory1, Is.InstanceOf<IScopeFactory>());
            Assert.That(scopeFactory1, Is.EqualTo(container));

            var childContainerr = container.CreateScope();
            var scopeFactory2 = childContainerr.Resolve<IScopeFactory>();
            Assert.That(scopeFactory2, Is.InstanceOf<IScopeFactory>());
            Assert.That(scopeFactory2, Is.Not.EqualTo(container));
            Assert.That(scopeFactory2, Is.EqualTo(childContainerr));
        }
    }
}
