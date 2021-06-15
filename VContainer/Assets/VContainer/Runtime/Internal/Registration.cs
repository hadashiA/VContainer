using System;
using System.Collections;
using System.Collections.Generic;

namespace VContainer.Internal
{
    sealed class Registration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> parameters;

        internal Registration(
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            IInjector injector)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;

            this.injector = injector;
            this.parameters = parameters;
        }

        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            return $"Registration {ImplementationType.Name} ContractTypes=[{contractTypes}] {Lifetime} {injector.GetType().Name}";
        }

        public object SpawnInstance(IObjectResolver resolver)
            => injector.CreateInstance(resolver, parameters);
    }

    sealed class CollectionRegistration : IRegistration, IEnumerable<IRegistration>
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes => interfaceTypes;
        public Lifetime Lifetime => Lifetime.Transient; // Collection refernce is transient. Members can have each lifetimes.

        readonly Type elementType;

        readonly List<Type> interfaceTypes;
        readonly IList<IRegistration> registrations = new List<IRegistration>();

        public CollectionRegistration(Type elementType) : this(
            typeof(List<>).MakeGenericType(elementType),
            elementType)
        {
        }

        public CollectionRegistration(Type listType, Type elementType)
        {
            this.elementType = elementType;
            ImplementationType = listType;
            interfaceTypes = new List<Type>
            {
                typeof(IEnumerable<>).MakeGenericType(elementType),
                typeof(IReadOnlyList<>).MakeGenericType(elementType),
            };
        }

        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            return $"CollectionRegistration {ImplementationType} ContractTypes=[{contractTypes}] {Lifetime}";
        }

        public void Add(IRegistration registration)
        {
            foreach (var x in registrations)
            {
                if (x.Lifetime == Lifetime.Singleton && x.ImplementationType == registration.ImplementationType)
                {
                    throw new VContainerException(registration.ImplementationType, $"Conflict implementation type : {registration}");
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
            try
            {
                foreach (var registration in registrations)
                {
                    list.Add(resolver.Resolve(registration));
                }
            }
            finally
            {
                CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
            }
            return list;
        }

        public IEnumerator<IRegistration> GetEnumerator() => registrations.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    sealed class ContainerRegistration : IRegistration
    {
        public static readonly ContainerRegistration Default = new ContainerRegistration();

        public Type ImplementationType => typeof(IObjectResolver);
        public IReadOnlyList<Type> InterfaceTypes => null;
        public Lifetime Lifetime => Lifetime.Transient;

        public object SpawnInstance(IObjectResolver resolver) => resolver;
    }
}
