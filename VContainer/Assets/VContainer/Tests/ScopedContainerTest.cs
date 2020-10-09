using NUnit.Framework;

namespace VContainer.Tests
{
    [TestFixture]
    public class ScopedContainerTest
    {
        [Test]
        public void CreateScope()
        {
            var builder = new ContainerBuilder();
            builder.Register<DisposableServiceA>(Lifetime.Singleton);
            builder.Register<DisposableServiceB>(Lifetime.Scoped);

            DisposableServiceA singleton1;
            DisposableServiceA singleton2;

            DisposableServiceB rootScopeObj;
            DisposableServiceB localScope1Obj;
            DisposableServiceB localScope2Obj;

            using (var container = builder.Build())
            {
                singleton1 = container.Resolve<DisposableServiceA>();
                rootScopeObj = container.Resolve<DisposableServiceB>();

                using (var scopedContainer1 = container.CreateScope())
                {
                    localScope1Obj = scopedContainer1.Resolve<DisposableServiceB>();

                    using (var scopedContaienr2 = scopedContainer1.CreateScope())
                    {
                        singleton2 = container.Resolve<DisposableServiceA>();
                        localScope2Obj = scopedContaienr2.Resolve<DisposableServiceB>();
                    }
                    Assert.That(rootScopeObj.Disposed, Is.False);
                    Assert.That(localScope1Obj.Disposed, Is.False);
                    Assert.That(localScope2Obj.Disposed, Is.True);
                }

                Assert.That(rootScopeObj.Disposed, Is.False);
                Assert.That(localScope1Obj.Disposed, Is.True);
            }
            Assert.That(rootScopeObj.Disposed, Is.True);

            Assert.That(singleton1.Disposed, Is.False);
            Assert.That(singleton2.Disposed, Is.False);
            Assert.That(singleton1, Is.EqualTo(singleton2));
        }

        [Test]
        public void CreateScopeAndRegister()
        {
            var builder = new ContainerBuilder();
            builder.Register<DisposableServiceA>(Lifetime.Scoped);

            DisposableServiceA rootScopeObj;
            DisposableServiceB localScopeObj;

            using (var container = builder.Build())
            {
                rootScopeObj = container.Resolve<DisposableServiceA>();

                using (var scopedContainer = container.CreateScope(localBuilder =>
                {
                    localBuilder.Register<DisposableServiceB>(Lifetime.Scoped);
                    localBuilder.Register<NoDependencyServiceA>(Lifetime.Singleton);
                }))
                {
                    localScopeObj = scopedContainer.Resolve<DisposableServiceB>();
                }

                Assert.That(rootScopeObj.Disposed, Is.False);
                Assert.That(localScopeObj.Disposed, Is.True);
                Assert.Throws<VContainerException>(() => container.Resolve<DisposableServiceB>());
            }
        }

        [Test]
        public void CreateScopeWithResolveSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register<NoDependencyServiceA>(Lifetime.Singleton);

            var container = builder.Build();

            var scopedContainer = container.CreateScope();

            var singleton = scopedContainer.Resolve<NoDependencyServiceA>();
            Assert.That(singleton, Is.InstanceOf<NoDependencyServiceA>());
        }

        [Test]
        public void CreateScopeWithRegisterSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register<NoDependencyServiceA>(Lifetime.Singleton);

            var parentContainer = builder.Build();

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
                childBuilder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
                childBuilder.Register<ServiceA>(Lifetime.Singleton);
            });
            {
                var parentSingleton = childContainer.Resolve<NoDependencyServiceA>();
                Assert.That(parentSingleton, Is.InstanceOf<NoDependencyServiceA>());

                var childSingleton = childContainer.Resolve<ServiceA>();
                Assert.That(childSingleton, Is.InstanceOf<ServiceA>());
                Assert.That(childSingleton.Service2, Is.InstanceOf<I2>());
            }

            var grandChildContainer = childContainer.CreateScope(grandChildBuilder =>
            {
                grandChildBuilder.Register<I3, NoDependencyServiceB>(Lifetime.Singleton);
                grandChildBuilder.Register<ServiceB>(Lifetime.Singleton);
            });
            {
                var parentSingleton = grandChildContainer.Resolve<NoDependencyServiceA>();
                Assert.That(parentSingleton, Is.InstanceOf<NoDependencyServiceA>());

                var childSingleton = grandChildContainer.Resolve<ServiceA>();
                Assert.That(childSingleton, Is.InstanceOf<ServiceA>());
                Assert.That(childSingleton.Service2, Is.InstanceOf<I2>());

                var grandChildSingleton = grandChildContainer.Resolve<ServiceB>();
                Assert.That(grandChildSingleton, Is.InstanceOf<ServiceB>());
                Assert.That(grandChildSingleton.Service3, Is.InstanceOf<I3>());
            }
        }
    }
}
