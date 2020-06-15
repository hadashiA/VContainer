using System;
using System.Collections;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public interface IRegistration
    {
        Type ImplementationType { get; }
        IReadOnlyList<Type> InterfaceTypes { get; }
        Lifetime Lifetime { get; }
        object SpawnInstance(IObjectResolver resolver);
    }

    public sealed class CollectionRegistration : IRegistration, IEnumerable<IRegistration>
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes => interfaceTypes;
        public Lifetime Lifetime => Lifetime.Transient; // Collection refernce is transient. Members can have each lifetimes.

        readonly Type elementType;

        readonly List<Type> interfaceTypes;
        readonly IList<Registration> registrations = new List<Registration>();

        public CollectionRegistration(Type elementType)
        {
            this.elementType = elementType;
            ImplementationType = typeof(List<>).MakeGenericType(elementType);
            interfaceTypes = new List<Type>
            {
                typeof(IEnumerable<>).MakeGenericType(elementType),
                typeof(IReadOnlyList<>).MakeGenericType(elementType),
            };
        }

        public void Add(Registration registration)
        {
            foreach (var x in registrations)
            {
                if (x.ImplementationType == registration.ImplementationType)
                {
                    throw new VContainerException($"Conflict implementation type : {registration}");
                }
            }
            registrations.Add(registration);
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var genericType = typeof(List<>).MakeGenericType(elementType);
            var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(1);
            parameterValues[0] = registrations.Count;
            var list = (IList)Activator.CreateInstance(genericType, parameterValues);
            foreach (var registration in registrations)
            {
                list.Add(resolver.Resolve(registration));
            }
            CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
            return list;
        }

        public IEnumerator<IRegistration> GetEnumerator() => registrations.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class Registration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly IInjector injector;
        readonly object specificInstance;

        internal Registration(
            Type implementationType,
            IReadOnlyList<Type> interfaceTypes,
            Lifetime lifetime,
            IInjector injector,
            object specificInstance = null)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;

            this.injector = injector;
            this.specificInstance = specificInstance;
        }

        public override string ToString() => $"ConcreteType={ImplementationType.Name} ContractTypes={string.Join(", ", InterfaceTypes)} {Lifetime} {injector.GetType().Name}";

        public object SpawnInstance(IObjectResolver resolver)
        {
            if (specificInstance != null)
            {
                injector.Inject(specificInstance, resolver);
                return specificInstance;
            }
            return injector.CreateInstance(resolver);
        }
    }

    public sealed class RegistrationBuilder
    {
        readonly Type implementationType;
        List<Type> interfaceTypes;
        Lifetime lifetime;
        readonly object specificInstance;

        public RegistrationBuilder(Type implementationType, Lifetime lifetime, List<Type> interfaceTypes = null)
        {
            this.implementationType = implementationType;
            this.interfaceTypes = interfaceTypes;
            this.lifetime = lifetime;
        }

        public RegistrationBuilder(Type implementationType, object instance)
        {
            this.implementationType = implementationType;
            lifetime = Lifetime.Scoped;
            specificInstance = instance;
        }

        public Registration Build()
        {
            var injector = ReflectionInjectorBuilder.Default.Build(implementationType, specificInstance != null);

            return new Registration(
                implementationType,
                interfaceTypes,
                lifetime,
                injector,
                specificInstance);
        }

        public RegistrationBuilder As<TInterface>()
        {
            (interfaceTypes ??= new List<Type>()).Add(typeof(TInterface));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2>()
        {
            var list = (interfaceTypes ??= new List<Type>());
            list.Add(typeof(TInterface1));
            list.Add(typeof(TInterface2));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3>()
        {
            var list = (interfaceTypes ??= new List<Type>());
            list.Add(typeof(TInterface1));
            list.Add(typeof(TInterface2));
            list.Add(typeof(TInterface3));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3, TInterface4>()
        {
            var list = (interfaceTypes ??= new List<Type>());
            list.Add(typeof(TInterface1));
            list.Add(typeof(TInterface2));
            list.Add(typeof(TInterface3));
            list.Add(typeof(TInterface4));
            return this;
        }

        public RegistrationBuilder AsSelf()
        {
            (interfaceTypes ??= new List<Type>()).Add(implementationType);
            return this;
        }

        public RegistrationBuilder AsImplementedInterfaces()
        {
            interfaceTypes.AddRange(implementationType.GetInterfaces());
            return this;
        }
    }
}
