using NUnit.Framework;

namespace VContainer.Tests
{
    [TestFixture]
    public class VirtualMemberTest
    {
        [Test]
        public void SubClassWithOverrideInjectMembers()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(1);
            builder.Register<SubClassWithOverrideInjectMembers>(Lifetime.Scoped);

            var container = builder.Build();
            var subClass = container.Resolve<SubClassWithOverrideInjectMembers>();

            Assert.That(subClass.InjectPropertySetterCalls, Is.EqualTo(1));
            Assert.That(subClass.InjectVirtualPropertySetterCalls, Is.EqualTo(1));
            Assert.That(subClass.InjectVirtualMethodCalls, Is.EqualTo(1));
        }

        [Test]
        public void SubClassWithoutOverrideInjectMembers()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(1);
            builder.Register<SubClassWithoutOverrideInjectMembers>(Lifetime.Scoped);

            var container = builder.Build();
            var subClass = container.Resolve<SubClassWithoutOverrideInjectMembers>();

            Assert.That(subClass.InjectPropertySetterCalls, Is.EqualTo(1));
            Assert.That(subClass.InjectVirtualPropertySetterCalls, Is.EqualTo(1));
            Assert.That(subClass.InjectVirtualMethodCalls, Is.EqualTo(1));
        }

        [Test]
        public void SubClassOverrideWithInjectAttribute()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(1);
            builder.Register<SubClassOverrideWithInjectAttribute>(Lifetime.Scoped);

            var container = builder.Build();
            var subClass = container.Resolve<SubClassOverrideWithInjectAttribute>();

            Assert.That(subClass.InjectPropertySetterCalls, Is.EqualTo(1));
            Assert.That(subClass.InjectVirtualPropertySetterCalls, Is.EqualTo(1));
            Assert.That(subClass.InjectVirtualMethodCalls, Is.EqualTo(1));
        }
    }
}