using System;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Internal
{
    public sealed class FixedTypeKeyHashTableRegistry : IRegistry
    {
        [ThreadStatic]
        static IDictionary<Type, IRegistration> buildBuffer = new Dictionary<Type, IRegistration>(128);

        readonly FixedTypeKeyHashtable<IRegistration> table;

        public static FixedTypeKeyHashTableRegistry Build(IEnumerable<IRegistration> registrations)
        {
            // ThreadStatic
            if (buildBuffer == null)
                buildBuffer = new Dictionary<Type, IRegistration>(128);
            buildBuffer.Clear();

            foreach (var registration in registrations)
            {
                if (registration.InterfaceTypes?.Count > 0)
                {
                    foreach (var interfaceType in registration.InterfaceTypes)
                    {
                        AddToBuildBuffer(buildBuffer, interfaceType, registration);
                    }

                    // Mark the ImplementationType with a guard because we need to check if it exists later.
                    if (!buildBuffer.ContainsKey(registration.ImplementationType))
                    {
                        buildBuffer.Add(registration.ImplementationType, null);
                    }
                }
                else
                {
                    AddToBuildBuffer(buildBuffer, registration.ImplementationType, registration);
                }
            }

            var hashTable = new FixedTypeKeyHashtable<IRegistration>(buildBuffer.ToArray());
            return new FixedTypeKeyHashTableRegistry(hashTable);
        }

        static void AddToBuildBuffer(IDictionary<Type, IRegistration> buf, Type service, IRegistration registration)
        {
            if (buf.TryGetValue(service, out var exists))
            {
                var collectionService = typeof(IEnumerable<>).MakeGenericType(service);
                CollectionRegistration collection;
                if (buf.TryGetValue(collectionService, out var found))
                {
                    collection = (CollectionRegistration)found;
                }
                else
                {
                    collection = new CollectionRegistration(collectionService, service) { exists };
                    AddCollectionToBuildBuffer(buf, collection);
                }
                collection.Add(registration);

                // Overwritten by the later registration
                buf[service] = registration;
            }
            else
            {
                buf.Add(service, registration);
            }
        }

        static void AddCollectionToBuildBuffer(IDictionary<Type, IRegistration> buf, CollectionRegistration collectionRegistration)
        {
            foreach (var collectionType in collectionRegistration.InterfaceTypes)
            {
                try
                {
                    buf.Add(collectionType, collectionRegistration);
                }
                catch (ArgumentException)
                {
                    throw new VContainerException(collectionType, $"Registration with the same key already exists: {collectionType} {collectionRegistration}");
                }
            }
        }

        FixedTypeKeyHashTableRegistry(FixedTypeKeyHashtable<IRegistration> table)
        {
            this.table = table;
        }

        public bool TryGet(Type interfaceType, out IRegistration registration)
        {
            if (table.TryGet(interfaceType, out registration))
                return registration != null;

            if (interfaceType.IsConstructedGenericType)
            {
                var openGenericType = interfaceType.GetGenericTypeDefinition();
                return TryFallbackSingleCollection(interfaceType, openGenericType, out registration) ||
                       TryFallbackGeneric(interfaceType, openGenericType, out registration);
            }
            return false;
        }

        public bool Exists(Type type) => table.TryGet(type, out _);

        bool TryFallbackSingleCollection(Type interfaceType, Type openGenericType, out IRegistration registration)
        {
            if (openGenericType == typeof(IEnumerable<>) ||
                openGenericType == typeof(IReadOnlyList<>))
            {
                var elementType = interfaceType.GetGenericArguments()[0];
                var collectionRegistration = new CollectionRegistration(elementType);
                // ReSharper disable once InconsistentlySynchronizedField
                if (table.TryGet(elementType, out var elementRegistration) && elementRegistration != null)
                {
                    collectionRegistration.Add(elementRegistration);
                }
                registration = collectionRegistration;
                return true;
            }
            registration = null;
            return false;
        }

        bool TryFallbackGeneric(Type interfaceType, Type interfaceOpenGenericType, out IRegistration registration)
        {
            if (table.TryGet(interfaceOpenGenericType, out var x) &&
                x is GenericFallbackRegistration openGenericRegistration)
            {
                registration = openGenericRegistration.BuildGenericRegistration(
                    interfaceOpenGenericType,
                    interfaceType);
                return true;
            }

            registration = null;
            return false;
        }
    }
}
