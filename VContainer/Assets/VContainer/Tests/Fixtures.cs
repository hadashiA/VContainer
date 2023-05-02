using System;

namespace VContainer.Tests
{
    public interface I1
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
        public bool MethodCalledAfterFieldInjected;
        public bool MethodCalledAfterPropertyInjected;

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
            MethodCalledAfterFieldInjected = privateFieldInjectable != null;
        }

        [Inject]
        public void MethodInjectable2(I5 service5, I6 service6)
        {
            Method2Called = true;
            MethodCalledAfterPropertyInjected = privateProperty != null;
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

    class MultipleInterfaceServiceC : I1, I2, I3
    {
    }

    class MultipleInterfaceServiceD : I1, I2, I3
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

    class HasAbstractCircularDependency1
    {
        public HasAbstractCircularDependency1(I2 dependency2)
        {
            if (dependency2 == null)
            {
                throw new ArgumentException();
            }
        }
    }

    class HasAbstractCircularDependency2 : I2
    {
        public HasAbstractCircularDependency2(HasAbstractCircularDependency1 dependency1)
        {
            if (dependency1 == null)
            {
                throw new ArgumentException();
            }
        }
    }

    class HasCircularDependencyMsg1
    {
        public HasCircularDependencyMsg1(HasCircularDependencyMsg2 dependency2)
        {
            if (dependency2 == null)
            {
                throw new ArgumentException();
            }
        }
    }

    class HasCircularDependencyMsg2
    {
        [Inject]
        public void Method(HasCircularDependencyMsg3 dependency3)
        {
            if (dependency3 == null)
            {
                throw new ArgumentException();
            }
        }
    }

    class HasCircularDependencyMsg3
    {
        [Inject]
        public HasCircularDependencyMsg4 Field;
    }

    class HasCircularDependencyMsg4
    {
        [Inject]
        public HasCircularDependencyMsg1 Prop { get; set; }
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

    class GenericsService<T>
    {
        public readonly T ParameterService;

        public GenericsService(T parameterService)
        {
            ParameterService = parameterService;
        }
    }

    class GenericsArgumentService
    {
        public readonly GenericsService<I2> GenericsService;

        public GenericsArgumentService(GenericsService<I2> genericsService)
        {
            GenericsService = genericsService;
        }
    }

    class BaseClassWithInjectAttribute
    {
        public int InjectPropertySetterCalls;
        public int InjectVirtualPropertySetterCalls;
        public int InjectVirtualMethodCalls;

        int inejctPropertyValue;
        int injectVirtualPropertyValue;

        [Inject]
        public int InjectProperty
        {
            get => inejctPropertyValue;
            set
            {
                inejctPropertyValue = value;
                InjectPropertySetterCalls++;
            }
        }

        [Inject]
        public virtual int InjectVirtualProperty
        {
            get => injectVirtualPropertyValue;
            set
            {
                injectVirtualPropertyValue = value;
                InjectVirtualPropertySetterCalls++;
            }
        }

        [Inject]
        public virtual void InjectMethod(int value)
        {
            InjectVirtualMethodCalls++;
        }
    }

    class SubClassWithOverrideInjectMembers : BaseClassWithInjectAttribute
    {
        public override int InjectVirtualProperty
        {
            get => base.InjectVirtualProperty;
            set => base.InjectVirtualProperty = value;
        }

        public override void InjectMethod(int value)
        {
            base.InjectMethod(value);
        }
    }

    class SubClassWithoutOverrideInjectMembers : BaseClassWithInjectAttribute
    {
    }

    class SubClassOverrideWithInjectAttribute : BaseClassWithInjectAttribute
    {
        [Inject]
        public override int InjectVirtualProperty
        {
            get => base.InjectVirtualProperty;
            set => base.InjectVirtualProperty = value;
        }

        [Inject]
        public override void InjectMethod(int value)
        {
            base.InjectMethod(value);
        }
    }

    class SampleAttribute : Attribute
    {
    }
}
