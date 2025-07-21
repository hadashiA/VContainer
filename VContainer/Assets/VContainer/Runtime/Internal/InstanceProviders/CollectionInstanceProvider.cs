using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    struct RegistrationElement
    {
        public Registration Registration;
        public IObjectResolver RegisteredContainer;

        public RegistrationElement(Registration registration, IObjectResolver registeredContainer)
        {
            Registration = registration;
            RegisteredContainer = registeredContainer;
        }
    }

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
                if (x.Lifetime == Lifetime.Singleton && x.ImplementationType == registration.ImplementationType && x.Key == registration.Key)
                {
                    throw new VContainerException(registration.ImplementationType, $"Conflict implementation type : {registration}");
                }
            }
            registrations.Add(registration);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver)
        {
            if (resolver is IScopedObjectResolver scope)
            {
                using (ListPool<RegistrationElement>.Get(out var entirelyRegistrations))
                {
                    CollectFromParentScopes(scope, entirelyRegistrations);
                    return SpawnInstance(resolver, entirelyRegistrations);
                }
            }

            var array = Array.CreateInstance(ElementType, registrations.Count);
            for (var i = 0; i < registrations.Count; i++)
            {
                array.SetValue(resolver.Resolve(registrations[i]), i);
            }
            return array;
        }

        internal object SpawnInstance(IObjectResolver currentScope, IReadOnlyList<RegistrationElement> entirelyRegistrations)
        {
            var array = Array.CreateInstance(ElementType, entirelyRegistrations.Count);
            for (var i = 0; i < entirelyRegistrations.Count; i++)
            {
                var x = entirelyRegistrations[i];
                var resolver = x.Registration.Lifetime == Lifetime.Singleton
                    ? x.RegisteredContainer
                    : currentScope;
                array.SetValue(resolver.Resolve(x.Registration), i);
            }
            return array;
        }

        internal void CollectFromParentScopes(
            IScopedObjectResolver scope,
            List<RegistrationElement> registrationsBuffer,
            bool localScopeOnly = false)
        {
            foreach (var registration in registrations)
            {
                registrationsBuffer.Add(new RegistrationElement(registration, scope));
            }

            var finderType = InterfaceTypes[0];
            scope = scope.Parent;

            while (scope != null)
            {
                if (scope.TryGetRegistration(finderType, out var registration) &&
                    registration.Provider is CollectionInstanceProvider parentCollection)
                {
                    foreach (var x in parentCollection.registrations)
                    {
                        if (!localScopeOnly || x.Lifetime != Lifetime.Singleton)
                        {
                            registrationsBuffer.Add(new RegistrationElement(x, scope));
                        }
                    }
                }
                scope = scope.Parent;
            }
        }
    }
}