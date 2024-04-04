using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VContainer.Internal;

namespace VContainer.Tests
{
    [TestFixture]
    public class ContainerTest
    {
        [Test]
        public void ResolveTransient()
        {
            var builder = new ContainerBuilder();
            builder.Register<NoDependencyServiceA>(Lifetime.Transient);

            var container = builder.Build();
            var obj1 = container.Resolve<NoDependencyServiceA>();
            var obj2 = container.Resolve<NoDependencyServiceA>();

            Assert.That(obj1, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj2, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void ResolveSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register<NoDependencyServiceA>(Lifetime.Singleton);

            var container = builder.Build();
            var obj1 = container.Resolve<NoDependencyServiceA>();
            var obj2 = container.Resolve<NoDependencyServiceA>();

            Assert.That(obj1, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj2, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void ResolveScoped()
        {
            var builder = new ContainerBuilder();
            builder.Register<DisposableServiceA>(Lifetime.Scoped);

            var container = builder.Build();
            var obj1 = container.Resolve<DisposableServiceA>();
            var obj2 = container.Resolve<DisposableServiceA>();

            Assert.That(obj1, Is.TypeOf<DisposableServiceA>());
            Assert.That(obj2, Is.TypeOf<DisposableServiceA>());
            Assert.That(obj1, Is.EqualTo(obj2));

            container.Dispose();

            Assert.That(obj1.Disposed, Is.True);
        }

        [Test]
        public void ResolveAsInterfaces()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<MultipleInterfaceServiceA>(Lifetime.Singleton)
                .As<I1>()
                .As<I3>();

            var container = builder.Build();

            var obj1 = container.Resolve<I1>();
            var obj2 = container.Resolve<I2>();
            var obj3 = container.Resolve<I3>();

            Assert.That(obj1, Is.InstanceOf<I1>());
            Assert.That(obj2, Is.InstanceOf<I2>());
            Assert.That(obj3, Is.InstanceOf<I3>());
            Assert.That(obj1, Is.EqualTo(obj3));
            Assert.Throws<VContainerException>(() => container.Resolve<MultipleInterfaceServiceA>());
        }

        [Test]
        public void ResolveAsInterfacesAndSelf()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<MultipleInterfaceServiceA>(Lifetime.Singleton)
                .AsSelf()
                .As<I1>()
                .As<I3>();

            var container = builder.Build();

            var obj1 = container.Resolve<I1>();
            var obj2 = container.Resolve<I2>();
            var obj3 = container.Resolve<I3>();
            var obj4 = container.Resolve<MultipleInterfaceServiceA>();

            Assert.That(obj1, Is.InstanceOf<I1>());
            Assert.That(obj2, Is.InstanceOf<I2>());
            Assert.That(obj3, Is.InstanceOf<I3>());
            Assert.That(obj4, Is.InstanceOf<MultipleInterfaceServiceA>());
            Assert.That(obj4, Is.EqualTo(obj1));
            Assert.That(obj4, Is.EqualTo(obj3));
        }

        [Test]
        public void ResolveDependencies()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<I3, NoDependencyServiceB>(Lifetime.Singleton);
            builder.Register<I4, ServiceA>(Lifetime.Singleton);
            builder.Register<I5, ServiceB>(Lifetime.Singleton);
            builder.Register<I6, ServiceC>(Lifetime.Singleton);

            var container = builder.Build();

            var obj2 = container.Resolve<I2>();
            var obj3 = container.Resolve<I3>();
            var obj4 = container.Resolve<I4>();
            var obj5 = container.Resolve<I5>();
            var obj6 = container.Resolve<I6>();

            Assert.That(obj2, Is.InstanceOf<I2>());
            Assert.That(obj3, Is.InstanceOf<I3>());
            Assert.That(obj4, Is.InstanceOf<I4>());
            Assert.That(obj5, Is.InstanceOf<I5>());
            Assert.That(obj6, Is.InstanceOf<I6>());
        }

        [Test]
        public void ResolveAllInjectableFeatures()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, AllInjectionFeatureService>(Lifetime.Singleton);
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<I3, NoDependencyServiceB>(Lifetime.Singleton);
            builder.Register<I4, ServiceA>(Lifetime.Singleton);
            builder.Register<I5, ServiceB>(Lifetime.Singleton);
            builder.Register<I6, ServiceC>(Lifetime.Singleton);
            builder.Register<I7, ServiceD>(Lifetime.Singleton);

            var container = builder.Build();

            var obj1 = container.Resolve<I1>();
            var obj2 = container.Resolve<I2>();
            var obj3 = container.Resolve<I3>();
            var obj4 = container.Resolve<I4>();
            var obj5 = container.Resolve<I5>();
            var obj6 = container.Resolve<I6>();
            var obj7 = container.Resolve<I7>();

            Assert.That(obj1, Is.InstanceOf<I1>());
            Assert.That(obj2, Is.InstanceOf<I2>());
            Assert.That(obj3, Is.InstanceOf<I3>());
            Assert.That(obj4, Is.InstanceOf<I4>());
            Assert.That(obj5, Is.InstanceOf<I5>());
            Assert.That(obj6, Is.InstanceOf<I6>());
            Assert.That(obj7, Is.InstanceOf<I7>());

            Assert.That(((AllInjectionFeatureService)obj1).ConstructorCalled, Is.True);
            Assert.That(((AllInjectionFeatureService)obj1).Method1Called, Is.True);
            Assert.That(((AllInjectionFeatureService)obj1).Method2Called, Is.True);
            Assert.That(((AllInjectionFeatureService)obj1).MethodCalledAfterFieldInjected, Is.True);
            Assert.That(((AllInjectionFeatureService)obj1).MethodCalledAfterPropertyInjected, Is.True);
            Assert.That(((AllInjectionFeatureService)obj1).GetPrivateProperty(), Is.InstanceOf<I2>());
            Assert.That(((AllInjectionFeatureService)obj1).PublicPropertyInjectable, Is.InstanceOf<I3>());
            Assert.That(((AllInjectionFeatureService)obj1).GetPrivateFieldInjectable(), Is.InstanceOf<I4>());
            Assert.That(((AllInjectionFeatureService)obj1).PublicFieldInjectable, Is.InstanceOf<I5>());
            Assert.That(((AllInjectionFeatureService)obj1).FromConstructor1, Is.InstanceOf<I6>());
            Assert.That(((AllInjectionFeatureService)obj1).FromConstructor2, Is.InstanceOf<I7>());
        }

        [Test]
        public void ResolveCollection()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Singleton);
            builder.Register<I1, MultipleInterfaceServiceB>(Lifetime.Transient);

            var container = builder.Build();
            var enumerable = container.Resolve<IEnumerable<I1>>();
            var e0 = enumerable.ElementAt(0);
            var e1 = enumerable.ElementAt(1);
            Assert.That(e0, Is.TypeOf<MultipleInterfaceServiceA>());
            Assert.That(e1, Is.TypeOf<MultipleInterfaceServiceB>());

            var list = container.Resolve<IReadOnlyList<I1>>();
            Assert.That(list[0], Is.TypeOf<MultipleInterfaceServiceA>());
            Assert.That(list[1], Is.TypeOf<MultipleInterfaceServiceB>());

            // Singleton
            Assert.That(list[0], Is.EqualTo(e0));

            // Empty
            var empty = container.Resolve<IEnumerable<I7>>();
            Assert.That(empty, Is.Empty);
        }

        [Test]
        public void ResolveLastOneWhenConflicted()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Transient);
            builder.Register<I1, MultipleInterfaceServiceB>(Lifetime.Transient);

            builder.Register<I3, MultipleInterfaceServiceB>(Lifetime.Transient);
            builder.Register<I3, MultipleInterfaceServiceA>(Lifetime.Transient);
            builder.Register<I3, MultipleInterfaceServiceB>(Lifetime.Transient);

            var container = builder.Build();
            var i1 = container.Resolve<I1>();
            var i3 = container.Resolve<I3>();
            Assert.That(i1, Is.InstanceOf<MultipleInterfaceServiceB>());
            Assert.That(i3, Is.InstanceOf<MultipleInterfaceServiceB>());
        }

        [Test]
        public void ResolveOnceAsCollection()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Singleton);

            var container = builder.Build();
            var enumerable = container.Resolve<IEnumerable<I1>>();
            var e0 = enumerable.ElementAt(0);
            Assert.That(e0, Is.TypeOf<MultipleInterfaceServiceA>());
            Assert.That(enumerable.Count(), Is.EqualTo(1));

            var list = container.Resolve<IReadOnlyList<I1>>();
            Assert.That(list[0], Is.TypeOf<MultipleInterfaceServiceA>());
            Assert.That(list.Count, Is.EqualTo(1));
        }

        [Test]
        public void ResolveGenerics()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<GenericsService<I2>>(Lifetime.Singleton);
            builder.Register<GenericsArgumentService>(Lifetime.Singleton);

            var container = builder.Build();

            var resolved = container.Resolve<GenericsArgumentService>();
            Assert.That(resolved.GenericsService, Is.InstanceOf<GenericsService<I2>>());
            Assert.That(resolved.GenericsService.ParameterService, Is.InstanceOf<NoDependencyServiceA>());
        }

        [Test]
        public void ResolveOpenGeneric()
        {
            var builder = new ContainerBuilder();

            builder.Register<I2, NoDependencyServiceA>(Lifetime.Transient).AsSelf();
            builder.Register(typeof(GenericsService<>), Lifetime.Transient)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.Register(typeof(GenericsService2<,>), Lifetime.Singleton)
                .As(typeof(IGenericService<,>))
                .AsSelf();
            builder.Register<HasGenericDependency>(Lifetime.Singleton);

            var container = builder.Build();
            var obj1 = container.Resolve<IGenericService<NoDependencyServiceA>>();
            var obj2 = container.Resolve<IGenericService<string, NoDependencyServiceA>>();
            var obj3 = container.Resolve<IGenericService<string, NoDependencyServiceA>>();
            var obj4 = container.Resolve<GenericsService2<string, NoDependencyServiceA>>();
            var obj5 = container.Resolve<GenericsService<NoDependencyServiceA>>();
            var obj6 = container.Resolve<HasGenericDependency>();

            Assert.That(obj1, Is.TypeOf<GenericsService<NoDependencyServiceA>>());
            Assert.That(obj2, Is.TypeOf<GenericsService2<string, NoDependencyServiceA>>());
            Assert.AreSame(obj2, obj3);
            Assert.AreSame(obj3, obj4);
            Assert.AreNotSame(obj1, obj5);
            Assert.That(obj6.Service, Is.TypeOf<GenericsService<NoDependencyServiceA>>());
        }

        [Test]
        public void RegisterInstance()
        {
            var builder = new ContainerBuilder();
            var instance1 = new NoDependencyServiceB();
            var instance2 = new MultipleInterfaceServiceA();
            builder.RegisterInstance(instance1);
            builder.RegisterInstance<I2>(instance2);

            var container = builder.Build();

            var resolve1a = container.Resolve<NoDependencyServiceB>();
            var resolve1b = container.Resolve<NoDependencyServiceB>();
            var resolve2a = container.Resolve<I2>();
            var resolve2b = container.Resolve<I2>();
            Assert.That(resolve1a, Is.EqualTo(instance1));
            Assert.That(resolve1b, Is.EqualTo(instance1));
            Assert.That(resolve2a, Is.EqualTo(instance2));
            Assert.That(resolve2b, Is.EqualTo(instance2));
            Assert.Throws<VContainerException>(() => container.Resolve<I3>());
        }

        [Test]
        public void RegisterInstanceWithEnum()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(Lifetime.Scoped);

            var container = builder.Build();
            var enumResolved = container.Resolve<Lifetime>();
            Assert.That(enumResolved, Is.EqualTo(Lifetime.Scoped));
        }

        [Test]
        public void RegisterFromFunc()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Transient);

            builder.Register(c =>
            {
                return new ServiceA(c.Resolve<I2>());
            }, Lifetime.Scoped);

            var container = builder.Build();
            var resolved = container.Resolve<ServiceA>();
            Assert.That(resolved, Is.InstanceOf<ServiceA>());
            Assert.That(resolved.Service2, Is.InstanceOf<NoDependencyServiceA>());
        }

        [Test]
        public void RegisterValueTypeFromFunc()
        {
            var builder = new ContainerBuilder();
            builder.Register<GenericsService<bool>>(Lifetime.Transient);

            builder.Register(_ => true, Lifetime.Scoped);

            var container = builder.Build();
            var resolved = container.Resolve<GenericsService<bool>>();
            Assert.That(resolved.ParameterService, Is.True);
        }

        [Test]
        public void RegisterFromFuncWithInterface()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Transient);

            builder.Register<I4>(c =>
            {
                return new ServiceA(c.Resolve<I2>());
            }, Lifetime.Scoped);

            var container = builder.Build();
            var resolved = container.Resolve<I4>();
            Assert.That(resolved, Is.InstanceOf<ServiceA>());
            Assert.Throws<VContainerException>(() => container.Resolve<ServiceA>());
        }

        [Test]
        public void RegisterFromFuncWithDisposable()
        {
            var builder = new ContainerBuilder();
            builder.Register(_ =>
            {
                return new DisposableServiceA();
            }, Lifetime.Scoped);

            var container = builder.Build();
            var resolved = container.Resolve<DisposableServiceA>();
            Assert.That(resolved, Is.InstanceOf<DisposableServiceA>());
            Assert.That(resolved.Disposed, Is.False);

            container.Dispose();
            Assert.That(resolved.Disposed, Is.True);
        }

        [Test]
        public void RegisterMultipleDisposables()
        {
            var builder = new ContainerBuilder();
            builder.Register<IDisposable, DisposableServiceA>(Lifetime.Singleton);
            builder.Register<IDisposable, DisposableServiceB>(Lifetime.Scoped);

            var container = builder.Build();
            var disposables = container.Resolve<IReadOnlyList<IDisposable>>();
            container.Dispose();

            Assert.That(disposables[0], Is.TypeOf<DisposableServiceA>());
            Assert.That(disposables[1], Is.TypeOf<DisposableServiceB>());
            Assert.That(disposables[0], Is.InstanceOf<IDisposable>());
            Assert.That(disposables[1], Is.InstanceOf<IDisposable>());
            Assert.That(((DisposableServiceA)disposables[0]).Disposed, Is.True);
            Assert.That(((DisposableServiceB)disposables[1]).Disposed, Is.True);
        }

        [Test]
        public void RegisterWithParameter()
        {
            {
                var paramValue = new NoDependencyServiceA();
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Scoped)
                    .WithParameter<I2>(paramValue);

                var container = builder.Build();
                var resolved = container.Resolve<ServiceA>();
                Assert.That(resolved.Service2, Is.EqualTo(paramValue));
            }

            {
                var paramValue = new NoDependencyServiceA();
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Scoped)
                    .WithParameter("service2", paramValue);

                var container = builder.Build();
                var resolved = container.Resolve<ServiceA>();
                Assert.That(resolved.Service2, Is.EqualTo(paramValue));
            }

            {
                var paramValue = new NoDependencyServiceA();
                var builder = new ContainerBuilder();
                builder.Register<HasMethodInjection>(Lifetime.Scoped)
                    .WithParameter<I2>(paramValue);

                var container = builder.Build();
                var resolved = container.Resolve<HasMethodInjection>();
                Assert.That(resolved.Service2, Is.EqualTo(paramValue));
            }

            {
                var paramValue = new NoDependencyServiceA();
                var builder = new ContainerBuilder();
                builder.Register<HasMethodInjection>(Lifetime.Scoped)
                    .WithParameter("service2", paramValue);

                var container = builder.Build();
                var resolved = container.Resolve<HasMethodInjection>();
                Assert.That(resolved.Service2, Is.EqualTo(paramValue));
            }

            {
                var builder = new ContainerBuilder();
                builder.Register<I2, NoDependencyServiceA>(Lifetime.Scoped);
                builder.Register<HasMethodInjection>(Lifetime.Scoped)
                    .WithParameter(resolver => resolver.Resolve<I2>());

                var container = builder.Build();
                var resolved = container.Resolve<HasMethodInjection>();
                Assert.That(resolved.Service2, Is.Not.Null);
            }
        }

        [Test]
        public void RegisterSystem()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.RegisterContainer();

            var container = builder.Build();
            var resolve = container.Resolve<IObjectResolver>();
            Assert.That(resolve, Is.EqualTo(container));
        }

        // [Test]
        // public void RegisterConflictImplementations()
        // {
        //     var builder = new ContainerBuilder();
        //     builder.Register<IDisposable, DisposableServiceA>(Lifetime.Scoped);
        //     builder.Register<IDisposable, DisposableServiceA>(Lifetime.Scoped);
        //
        //     Assert.Throws<VContainerException>(() => builder.Build());
        // }

        [Test]
        public void RegisterInvalidInterface()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<VContainerException>(() => builder.Register<NoDependencyServiceA>(Lifetime.Scoped).As<I1>());
        }

        [Test]
        public void RegisterInvalidOpenGeneric()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<VContainerException>(() =>
                builder.Register(typeof(GenericsService<int>), Lifetime.Transient)
                    .As(typeof(IGenericService<>))
            );
            Assert.Throws<VContainerException>(() =>
                builder.Register(typeof(GenericsService<>), Lifetime.Transient)
                    .As(typeof(IGenericService<int>))
            );
            Assert.Throws<VContainerException>(() =>
                builder.Register(typeof(GenericsService<>), Lifetime.Transient)
                    .As(typeof(IGenericService<int>))
            );
        }

        [Test]
        public void ErrorMessageIncludesDependency()
        {
            var builder = new ContainerBuilder();
            builder.Register<AllInjectionFeatureService>(Lifetime.Scoped);

            var container = builder.Build();
            Assert.Throws<VContainerException>(() => container.Resolve<AllInjectionFeatureService>(),
                "Failed to resolve VContainer.Tests.AllInjectionFeatureService : No such registration of type: VContainer.Tests.I6");
       }

        [Test]
        public void CircularDependency()
        {
            var builder = new ContainerBuilder();
            builder.Register<HasCircularDependency1>(Lifetime.Transient);
            builder.Register<HasCircularDependency2>(Lifetime.Transient);

            var injector = InjectorCache.GetOrBuild(typeof(HasCircularDependency1));

            // Only reflection mode can detect circular dependency errors at runtime.
            if (injector is ReflectionInjector)
            {
                Assert.Throws<VContainerException>(() => builder.Build());
            }
        }

        [Test]
        public void CircularDependencyWithAbstract()
        {
            var builder = new ContainerBuilder();
            builder.Register<HasAbstractCircularDependency1>(Lifetime.Transient);
            builder.Register<HasAbstractCircularDependency2>(Lifetime.Transient).As<I2>();

            var injector = InjectorCache.GetOrBuild(typeof(HasCircularDependency1));

            // Only reflection mode can detect circular dependency errors at runtime.
            if (injector is ReflectionInjector)
            {
                Assert.Throws<VContainerException>(() => builder.Build());
            }
        }

        [Test]
        public void CircularDependencyMsg()
        {
            var builder = new ContainerBuilder();
            builder.Register<HasCircularDependencyMsg1>(Lifetime.Transient);
            builder.Register<HasCircularDependencyMsg2>(Lifetime.Transient);
            builder.Register<HasCircularDependencyMsg3>(Lifetime.Transient);
            builder.Register<HasCircularDependencyMsg4>(Lifetime.Transient);

            var injector = InjectorCache.GetOrBuild(typeof(HasCircularDependencyMsg1));

            // Only reflection mode can detect circular dependency errors at runtime.
            if (injector is ReflectionInjector)
            {
                var ex = Assert.Throws<VContainerException>(() => builder.Build());
                string expected =
                    "Circular dependency detected!\n" +
                    "    [1] VContainer.Tests.HasCircularDependencyMsg1..ctor(dependency2) --> VContainer.Tests.HasCircularDependencyMsg2\n" +
                    "    [2] VContainer.Tests.HasCircularDependencyMsg2.Method(dependency3) --> VContainer.Tests.HasCircularDependencyMsg3\n" +
                    "    [3] VContainer.Tests.HasCircularDependencyMsg3.Field --> VContainer.Tests.HasCircularDependencyMsg4\n" +
                    "    [4] VContainer.Tests.HasCircularDependencyMsg4.Prop --> VContainer.Tests.HasCircularDependencyMsg1";
                Assert.That(ex.Message, Is.EqualTo(expected));
            }
        }

        [Test]
        public void Inject()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);

            var container = builder.Build();

            var methodInjectable = new HasMethodInjection();
            Assert.That(methodInjectable.Service2, Is.Null);

            container.Inject(methodInjectable);
            Assert.That(methodInjectable.Service2, Is.InstanceOf<I2>());

            var noInjectable = new NoDependencyServiceA();
            Assert.DoesNotThrow(() => container.Inject(noInjectable));

            var ctorInjectable = new ServiceA(new NoDependencyServiceA());
            Assert.DoesNotThrow(() => container.Inject(ctorInjectable));
        }

        [Test]
        public void OnContainerDisposeCallback()
        {
            NoDependencyServiceA resolvedJustBeforeDispose = null;
            NoDependencyServiceB resolvedJustBeforeDispose2 = null;

            var builder = new ContainerBuilder();

            builder.Register<NoDependencyServiceA>(Lifetime.Scoped);
            builder.Register<NoDependencyServiceB>(Lifetime.Scoped);
            builder.RegisterDisposeCallback(resolver =>
                resolvedJustBeforeDispose = resolver.Resolve<NoDependencyServiceA>());
            builder.RegisterDisposeCallback(resolver =>
                resolvedJustBeforeDispose2 = resolver.Resolve<NoDependencyServiceB>());

            var container = builder.Build();

            Assert.That(resolvedJustBeforeDispose, Is.Null);
            Assert.That(resolvedJustBeforeDispose2, Is.Null);

            container.Dispose();

            Assert.That(resolvedJustBeforeDispose, Is.Not.Null);
            Assert.That(resolvedJustBeforeDispose2, Is.Not.Null);
        }

        [Test]
        public void TryResolveTransient()
        {
            var builder = new ContainerBuilder();
            builder.Register<NoDependencyServiceA>(Lifetime.Transient);

            var container = builder.Build();

            Assert.That(container.TryResolve<NoDependencyServiceA>(out var obj1), Is.True);
            Assert.That(container.TryResolve<NoDependencyServiceA>(out var obj2), Is.True);
            Assert.That(container.TryResolve<NoDependencyServiceB>(out var obj3), Is.False);

            Assert.That(obj1, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj2, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj1, Is.Not.EqualTo(obj2));
            Assert.That(obj3, Is.Null);
        }

        [Test]
        public void TryResolveSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register<NoDependencyServiceA>(Lifetime.Singleton);

            var container = builder.Build();
            Assert.That(container.TryResolve<NoDependencyServiceA>(out var obj1), Is.True);
            Assert.That(container.TryResolve<NoDependencyServiceA>(out var obj2), Is.True);
            Assert.That(container.TryResolve<NoDependencyServiceB>(out var obj3), Is.False);

            Assert.That(obj1, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj2, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj1, Is.EqualTo(obj2));
            Assert.That(obj3, Is.Null);
        }

        [Test]
        public void TryResolveScoped()
        {
            var builder = new ContainerBuilder();
            builder.Register<DisposableServiceA>(Lifetime.Scoped);

            var container = builder.Build();
            Assert.That(container.TryResolve<DisposableServiceA>(out var obj1), Is.True);
            Assert.That(container.TryResolve<DisposableServiceA>(out var obj2), Is.True);
            Assert.That(container.TryResolve<DisposableServiceB>(out var obj3), Is.False);

            Assert.That(obj1, Is.TypeOf<DisposableServiceA>());
            Assert.That(obj2, Is.TypeOf<DisposableServiceA>());
            Assert.That(obj1, Is.EqualTo(obj2));
            Assert.That(obj3, Is.Null);

            container.Dispose();

            Assert.That(obj1.Disposed, Is.True);
        }
    }
}
