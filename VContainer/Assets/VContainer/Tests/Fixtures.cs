using System;

namespace VContainer.Tests
{
    interface I1
    {
    }

    interface I2
    {
    }

    interface I3
    {
    }

    interface I4
    {
    }

    interface I5
    {
    }

    interface I6
    {
    }

    interface I7
    {
    }

    interface IOpenGenericService<T>
    {
        T InnerService1 { get; }
        I2 InnerService2 { get; }
    }

    interface IOpenGenericService2<T1, T2>
    {
        T1 InnerService1 { get; }
        T2 InnerService2 { get; }
        I4 InnerService3 { get; }
    }

    interface IRequestHandler<TRequest>
    {
        void Execute(TRequest request);
    }

    interface IRequestHandler<TRequest, TResponse>
    {
        TResponse Execute(TRequest request);
    }

    class AllInjectionFeatureService : I1
    {
        public bool ConstructorCalled;
        public bool Method1Called;
        public bool Method2Called;

        I2 privateProperty;

        [Inject]
        I2 PrivatePropertyInjectable
        {
            set => privateProperty = value;
        }

        I3 publicProperty;

        [Inject]
        public I3 PublicPropertyInjectable
        {
            get => publicProperty;
            set => publicProperty = value;
        }

        [Inject]
        I4 privateFieldInjectable;

        [Inject]
        public I5 PublicFieldInjectable;

        public I6 FromConstructor1;
        public I7 FromConstructor2;

        public AllInjectionFeatureService(int x, int y, int z)
        {
        }

        [Inject]
        public AllInjectionFeatureService(I6 fromConstructor1, I7 fromConstructor2)
        {
            ConstructorCalled = true;
            FromConstructor1 = fromConstructor1;
            FromConstructor2 = fromConstructor2;
        }

        [Inject]
        public void MethodInjectable1(I3 service3, I4 service4)
        {
            Method1Called = true;
        }

        [Inject]
        public void MethodInjectable2(I5 service5, I6 service6)
        {
            Method2Called = true;
        }

        public I4 GetPrivateFieldInjectable() => privateFieldInjectable;
        public I2 GetPrivateProperty() => privateProperty;
    }

    class DisposableServiceA : I1, IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    class DisposableServiceB : I2, IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    class NoDependencyServiceA : I2
    {
    }

    class NoDependencyServiceB : I3
    {
    }

    class ServiceA : I4
    {
        public readonly I2 Service2;

        public ServiceA(I2 service2)
        {
            if (service2 is null)
            {
                throw new ArgumentNullException(nameof(service2));
            }
            Service2 = service2;
        }
    }

    class ServiceB : I5
    {
        public readonly I3 Service3;

        public ServiceB(I3 service3)
        {
            if (service3 is null)
            {
                throw new ArgumentNullException(nameof(service3));
            }
            Service3 = service3;
        }
    }

    class ServiceC : I6
    {
        public ServiceC(
            I2 service2,
            I3 service3,
            I4 service4,
            I5 service5)
        {
            if (service2 is null)
            {
                throw new ArgumentNullException(nameof(service2));
            }

            if (service3 is null)
            {
                throw new ArgumentNullException(nameof(service3));
            }

            if (service4 is null)
            {
                throw new ArgumentNullException(nameof(service4));
            }

            if (service5 is null)
            {
                throw new ArgumentNullException(nameof(service5));
            }
        }
    }

    class ServiceD : I7
    {
    }

    class MultipleInterfaceServiceA : I1, I2, I3
    {
    }

    class MultipleInterfaceServiceB : I1, I2, I3
    {
    }

    class HasDefaultValue
    {
        [Inject]
        I2 privateFieldHasDefault = new NoDependencyServiceA();

        [Inject]
        I3 privatePropertyHasDefault { get; set; } = new NoDependencyServiceB();

        public I2 GetPrivateFieldHasDefault() => privateFieldHasDefault;
        public I3 GetPrivatePropertyHasDefault() => privatePropertyHasDefault;
    }

    class HasCircularDependency1
    {
        public HasCircularDependency1(HasCircularDependency2 dependency2)
        {
            if (dependency2 == null)
            {
                throw new ArgumentException();
            }
        }
    }

    class HasCircularDependency2
    {
        public HasCircularDependency2(HasCircularDependency1 dependency1)
        {
            if (dependency1 == null)
            {
                throw new ArgumentException();
            }
        }
    }

    class HasMethodInjection : I1
    {
        public I2 Service2;

        [Inject]
        public void MethodInjectable1(I2 service2)
        {
            Service2 = service2;
        }
    }

    class GenericService<T> : IOpenGenericService<T>
    {
        public T InnerService1 { get; }
        public I2 InnerService2 { get; }

        public GenericService(T innerService, I2 innerService2)
        {
            InnerService1 = innerService;
            InnerService2 = innerService2;
        }
    }

    class GenericService2<T1, T2> : IOpenGenericService2<T1, T2>
    {
        public T1 InnerService1 { get; }
        public T2 InnerService2 { get; }
        public I4 InnerService3 { get; }

        public GenericService2(T1 innerService1, T2 innerService2, I4 innerService3)
        {
            InnerService1 = innerService1;
            InnerService2 = innerService2;
            InnerService3 = innerService3;
        }
    }

    class GenericsArgumentService
    {
        public readonly GenericService<I2> GenericService;

        public GenericsArgumentService(GenericService<I2> genericService)
        {
            GenericService = genericService;
        }
    }

    class Foo
    {
        public int Param1 { get; set; }
        public int Param2 { get; set; }
        public int Param3 { get; set; }
        public int Param4 { get; set; }
        public I2 Service2 { get; set; }
        public I3 Service3 { get; set; }
    }

    class Bar
    {
        public int X { get; set; }
    }

    class FooHandler : IRequestHandler<Foo>
    {
        public void Execute(Foo request)
        {
        }
    }

    class FooHandler2 : IRequestHandler<Foo>
    {
        public readonly I2 ParameterService;

        public FooHandler2(I2 service2)
        {
            ParameterService = service2;
        }

        public void Execute(Foo request)
        {
        }
    }

    class FooBarHandler : IRequestHandler<Foo, Bar>
    {
        public Bar Execute(Foo request) => new Bar { X = 100 };
    }

    class FooBarHandler2 : IRequestHandler<Foo, Bar>
    {
        public readonly I2 service2;

        public FooBarHandler2(I2 service2)
        {
            this.service2 = service2;
        }

        public Bar Execute(Foo request) => new Bar { X = 200 };
    }
}
