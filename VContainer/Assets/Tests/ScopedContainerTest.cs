using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.That(singleton1.Disposed, Is.True);
            Assert.That(singleton2.Disposed, Is.True);
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
            builder.Register<DisposableServiceA>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            var container = builder.Build();
            var scopedContainer = container.CreateScope(childBuilder =>
            {
                childBuilder.Register<DisposableServiceB>(Lifetime.Singleton)
                    .AsImplementedInterfaces()
                    .AsSelf();
            });

            var singleton = container.Resolve<DisposableServiceA>();
            var singleton2 = scopedContainer.Resolve<DisposableServiceA>();
            Assert.That(singleton, Is.InstanceOf<DisposableServiceA>());
            Assert.That(singleton2, Is.InstanceOf<DisposableServiceA>());
            Assert.That(singleton, Is.EqualTo(singleton2));
        }

        [Test]
        public void CreateScopeWithRegisterSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register<DisposableServiceA>(Lifetime.Singleton);

            var parentContainer = builder.Build();

            DisposableServiceB childSingletonDisposable;
            DisposableServiceB grandChildSingletonDisposable;

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
                childBuilder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
                childBuilder.Register<ServiceA>(Lifetime.Singleton);
                childBuilder.Register<DisposableServiceB>(Lifetime.Singleton);
            });
            {
                var parentSingleton = childContainer.Resolve<DisposableServiceA>();
                Assert.That(parentSingleton, Is.InstanceOf<DisposableServiceA>());
                Assert.That(parentSingleton, Is.EqualTo(parentContainer.Resolve<DisposableServiceA>()));

                var childSingleton = childContainer.Resolve<ServiceA>();
                Assert.That(childSingleton, Is.InstanceOf<ServiceA>());
                Assert.That(childSingleton.Service2, Is.InstanceOf<I2>());
            }

            var grandChildContainer = childContainer.CreateScope(grandChildBuilder =>
            {
                grandChildBuilder.Register<I3, NoDependencyServiceB>(Lifetime.Singleton);
                grandChildBuilder.Register<ServiceB>(Lifetime.Singleton);
                grandChildBuilder.Register<DisposableServiceB>(Lifetime.Singleton);
            });
            {
                var parentSingleton = grandChildContainer.Resolve<DisposableServiceA>();
                Assert.That(parentSingleton, Is.InstanceOf<DisposableServiceA>());
                Assert.That(parentSingleton, Is.EqualTo(parentContainer.Resolve<DisposableServiceA>()));

                var childSingleton = grandChildContainer.Resolve<ServiceA>();
                Assert.That(childSingleton, Is.InstanceOf<ServiceA>());
                Assert.That(childSingleton.Service2, Is.InstanceOf<I2>());
                Assert.That(childSingleton, Is.EqualTo(childContainer.Resolve<ServiceA>()));

                var grandChildSingleton = grandChildContainer.Resolve<ServiceB>();
                Assert.That(grandChildSingleton, Is.InstanceOf<ServiceB>());
                Assert.That(grandChildSingleton.Service3, Is.InstanceOf<I3>());
            }

            childSingletonDisposable = childContainer.Resolve<DisposableServiceB>();
            childContainer.Dispose();
            Assert.That(childSingletonDisposable.Disposed, Is.True);

            grandChildSingletonDisposable = grandChildContainer.Resolve<DisposableServiceB>();
            grandChildContainer.Dispose();
            Assert.That(grandChildSingletonDisposable.Disposed, Is.True);
        }


        [Test]
        public void CreateScopeWithRegisterOnlyInterfaces()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var child = container.CreateScope(childBuilder =>
            {
                childBuilder.Register<NoDependencyServiceA>(Lifetime.Singleton).AsImplementedInterfaces();
                childBuilder.Register<ServiceA>(Lifetime.Singleton).AsImplementedInterfaces();
            });

            var singletonAsInterface = child.Resolve<I4>();
            Assert.That(singletonAsInterface, Is.InstanceOf<ServiceA>());
        }

        [Test]
        public void ResolveFromParent()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<I3, NoDependencyServiceB>(Lifetime.Singleton);
            var parentContainer = builder.Build();

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
                // childBuilder.Register<ServiceA>(Lifetime.Singleton);
                childBuilder.Register<ServiceB>(Lifetime.Scoped);
            });

            //var singletonService = childContainer.Resolve<ServiceA>();
            var scopedService = childContainer.Resolve<ServiceB>();
            //Assert.That(singletonService, Is.InstanceOf<ServiceA>());
            Assert.That(scopedService, Is.InstanceOf<ServiceB>());
        }
        
        [Test]
        public void ResolveCollectionFromParent()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Scoped);
            builder.Register<I1, MultipleInterfaceServiceB>(Lifetime.Scoped);
            var parentContainer = builder.Build();

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
                childBuilder.Register<I1CollectionService>(Lifetime.Scoped);
            });
            
            var scopedService = childContainer.Resolve<I1CollectionService>();
            Assert.That(scopedService.enumerable.Count(), Is.EqualTo(2));
        }
        
        [Test]
        public void ResolveCollectionFromParentByContinuous()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Singleton);
            builder.Register<I1, MultipleInterfaceServiceB>(Lifetime.Singleton);
            var parentContainer = builder.Build();

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
                childBuilder.Register<I1, MultipleInterfaceServiceC>(Lifetime.Singleton);
                childBuilder.Register<I1, MultipleInterfaceServiceD>(Lifetime.Singleton);
            });
            
            var scopedService = childContainer.Resolve<IReadOnlyList<I1>>();
            var moreScopedService = childContainer.Resolve<IReadOnlyList<I1>>();
            Assert.That(moreScopedService.Count(), Is.EqualTo(4));
        }
        
        [Test]
        public void ResolveCollectionByLazyInstance()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Scoped);
            builder.Register<I1, MultipleInterfaceServiceB>(Lifetime.Scoped);
            builder.Register<I1CollectionService>(Lifetime.Scoped);
            
            var parentContainer = builder.Build();

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
            });
            
            var scopedService = childContainer.Resolve<I1CollectionService>();
            Assert.That(scopedService.enumerable.Count(), Is.EqualTo(2));
        }
        
        public class I1CollectionService
        {
            public readonly IReadOnlyList<I1> enumerable;

            public I1CollectionService(IReadOnlyList<I1> enumerable)
            {
                this.enumerable = enumerable;
            }
        }

        [Test]
        public void Inject()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var childContainer = container.CreateScope(childBuilder =>
            {
                childBuilder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            });

            var methodInjectable = new HasMethodInjection();
            Assert.That(methodInjectable.Service2, Is.Null);

            childContainer.Inject(methodInjectable);
            Assert.That(methodInjectable.Service2, Is.InstanceOf<I2>());

            var noInjectable = new NoDependencyServiceA();
            Assert.DoesNotThrow(() => childContainer.Inject(noInjectable));

            var ctorInjectable = new ServiceA(new NoDependencyServiceA());
            Assert.DoesNotThrow(() => childContainer.Inject(ctorInjectable));
        }


        [Test]
        public void InstanceRegistrationDoesNotDisposal()
        {
            var instance1 = new DisposableServiceA();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance1);

            var container = builder.Build();
            var childContainer = container.CreateScope();

            var resolveFromParent = container.Resolve<DisposableServiceA>();
            var resolveFromChild = childContainer.Resolve<DisposableServiceA>();

            Assert.That(resolveFromParent, Is.InstanceOf<DisposableServiceA>());
            Assert.That(resolveFromChild, Is.InstanceOf<DisposableServiceA>());
            Assert.That(resolveFromParent, Is.EqualTo(resolveFromChild));

            childContainer.Dispose();
            Assert.That(resolveFromParent.Disposed, Is.False);

            container.Dispose();
            Assert.That(resolveFromParent.Disposed, Is.False);
        }
    }
}
