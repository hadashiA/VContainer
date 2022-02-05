using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    sealed class CollectionInstanceProvider : IInstanceProvider, IEnumerable<Registration>
    {
        public static bool Match(Type openGenericType) => openGenericType == typeof(IEnumerable<>) ||
                                                          openGenericType == typeof(IReadOnlyList<>);

        public List<Registration>.Enumerator GetEnumerator() => registrations.GetEnumerator();
        IEnumerator<Registration> IEnumerable<Registration>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes => interfaceTypes;
        public Lifetime Lifetime => Lifetime.Transient; // Collection reference is transient. So its members can have each lifetimes.

        public Type ElementType { get; }

        readonly List<Type> interfaceTypes;
        readonly List<Registration> registrations = new List<Registration>();

        public CollectionInstanceProvider(Type elementType)
        {
            ElementType = elementType;
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

        public void Merge(CollectionInstanceProvider other)
        {
            foreach (var x in other.registrations)
            {
                Add(x);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver)
        {
            var array = Array.CreateInstance(ElementType, registrations.Count);
            for (var i = 0; i < registrations.Count; i++)
            {
                array.SetValue(resolver.Resolve(registrations[i]), i);
            }
            return array;
        }
    }
}
