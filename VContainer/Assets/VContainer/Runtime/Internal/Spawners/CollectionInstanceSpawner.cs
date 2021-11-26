using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    sealed class CollectionInstanceSpawner : IInstanceSpawner, IEnumerable<Registration>
    {
        public struct Enumerator : IEnumerator<Registration>
        {
            public Registration Current => listEnumerator.Current;
            object IEnumerator.Current => Current;

            List<Registration>.Enumerator listEnumerator;

            public Enumerator(CollectionInstanceSpawner owner)
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
        IEnumerator<Registration> IEnumerable<Registration>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes => interfaceTypes;
        public Lifetime Lifetime => Lifetime.Transient; // Collection reference is transient. So its members can have each lifetimes.

        readonly Type elementType;

        readonly List<Type> interfaceTypes;
        readonly List<Registration> registrations = new List<Registration>();

        public CollectionInstanceSpawner(Type elementType)
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

        public void Add(Registration registration)
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

        public void Merge(CollectionInstanceSpawner other)
        {
            foreach (var x in other.registrations)
            {
                Add(x);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Spawn(IObjectResolver resolver)
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
