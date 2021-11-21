using System;
using System.Collections;
using System.Collections.Generic;

namespace VContainer.Internal
{
    sealed class CollectionRegistration : IRegistration, IEnumerable<IRegistration>
    {
        public struct Enumerator : IEnumerator<IRegistration>
        {
            public IRegistration Current => listEnumerator.Current;
            object IEnumerator.Current => Current;

            List<IRegistration>.Enumerator listEnumerator;

            public Enumerator(CollectionRegistration owner)
            {
                listEnumerator = owner.registrations.GetEnumerator();
            }

            public bool MoveNext() => listEnumerator.MoveNext();
            public void Dispose() => listEnumerator.Dispose();
            void IEnumerator.Reset() => ((IEnumerator)listEnumerator).Reset();
        }

        public static bool Match(Type openGenericType) => openGenericType == typeof(IEnumerable<>) ||
                                                          openGenericType == typeof(IReadOnlyList<>);

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<IRegistration> IEnumerable<IRegistration>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes => interfaceTypes;
        public Lifetime Lifetime => Lifetime.Transient; // Collection reference is transient. So its members can have each lifetimes.

        readonly Type elementType;

        readonly List<Type> interfaceTypes;
        readonly List<IRegistration> registrations = new List<IRegistration>();

        public CollectionRegistration(Type elementType)
        {
            this.elementType = elementType;
            ImplementationType = elementType.MakeArrayType();
            interfaceTypes = new List<Type>
            {
                RuntimeTypeCache.EnumerableTypeOf(elementType),
                RuntimeTypeCache.ReadOnlyListTypeOf(elementType),
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

        public void Merge(CollectionRegistration other)
        {
            foreach (var x in other.registrations)
            {
                Add(x);
            }
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var array = Array.CreateInstance(elementType, registrations.Count);
            for (var i = 0; i < registrations.Count; i++)
            {
                array.SetValue(resolver.Resolve(registrations[i]), i);
            }
            return array;
        }
    }
}
