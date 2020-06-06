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
        public void MethodInjectable1()
        {
            Method1Called = true;
        }

        [Inject]
        public void MethodInjectable2()
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
        public ServiceA(I2 service2)
        {
            if (service2 is null)
            {
                throw new ArgumentNullException(nameof(service2));
            }
        }
    }

    class ServiceB : I5
    {
        public ServiceB(I3 service3)
        {
            if (service3 is null)
            {
                throw new ArgumentNullException(nameof(service3));
            }
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
}
