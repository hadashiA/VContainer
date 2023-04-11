using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace VContainer.Tests
{
    [TestFixture]
    class VirtualMemberInjectOnceTest
    {
        [Test]
        public void SubClassWithOverrideInjectMembers()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(1);
            builder.RegisterBuildCallback(resolver =>
            {
                var subClass = new SubClassWithOverrideInjectMembers();
                resolver.Inject(subClass);
                subClass.AreInjectedOnce();
            });
            builder.Build();
        }

        [Test]
        public void SubClassWithoutOverrideInjectMembers()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(1);
            builder.RegisterBuildCallback(resolver =>
            {
                var subClass = new SubClassWithoutOverrideInjectMembers();
                resolver.Inject(subClass);
                subClass.AreInjectedOnce();
            });
            builder.Build();
        }
    }

    class BaseClassWithInjectAttribute
    {
        private int _privatePropertyValue;
        private int _injectPrivatePropertyCount;
        [Inject]
        private int PrivatePropertyValue
        {
            get => _privatePropertyValue;
            set
            {
                _privatePropertyValue = value;
                _injectPrivatePropertyCount++;
            }
        }

        private int _virtualPropertyValue;
        private int _injectVirtualPropertyCount;
        [Inject]
        public virtual int VirtualPropertyValue
        {
            get => _virtualPropertyValue;
            set
            {
                _virtualPropertyValue = value;
                _injectVirtualPropertyCount++;
            }
        }

        private int _injectPrivateMethodCount;
        [Inject]
        private void InjectPrivate(int value)
        {
            _injectPrivateMethodCount++;
        }

        private int _injectVirtualMethodCount;
        [Inject]
        public virtual void InjectPublic(int value)
        {
            _injectVirtualMethodCount++;
        }

        public void AreInjectedOnce()
        {
            Assert.AreEqual(_injectPrivateMethodCount, 1);
            Assert.AreEqual(_injectVirtualMethodCount, 1);
            Assert.AreEqual(_injectPrivatePropertyCount, 1);
            Assert.AreEqual(_injectVirtualPropertyCount, 1);
        }
    }

    class SubClassWithOverrideInjectMembers : BaseClassWithInjectAttribute
    {
        public override void InjectPublic(int value)
        {
            base.InjectPublic(value);
        }

        public override int VirtualPropertyValue { get => base.VirtualPropertyValue; set => base.VirtualPropertyValue = value; }
    }

    class SubClassWithoutOverrideInjectMembers : BaseClassWithInjectAttribute
    {
    }

    // For this test, we first need to find a solution
    // class SubClassOverrideWithInjectAttribute : BaseClassWithInjectAttribute
    // {
    //     [Inject]
    //     public override void InjectPublic(int value)
    //     {
    //         base.InjectPublic(value);
    //     }
    //
    //     [Inject]
    //     public override int VirtualPropertyValue { get => base.VirtualPropertyValue; set => base.VirtualPropertyValue = value; }
    // }
}
