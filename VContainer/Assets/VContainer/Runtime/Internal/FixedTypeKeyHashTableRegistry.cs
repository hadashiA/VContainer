using System;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Internal
{
    public sealed class FixedTypeKeyHashTableRegistry : IRegistry
    {
        [ThreadStatic]
        static IDictionary<Type, IRegistration> buildBuffer = new Dictionary<Type, IRegistration>(128);

        readonly FixedTypeKeyHashtable<IRegistration> hashTable;

        public static FixedTypeKeyHashTableRegistry Build(IReadOnlyList<IRegistration> registrations)
        {
            // ThreadStatic
            if (buildBuffer == null)
                buildBuffer = new Dictionary<Type, IRegistration>(128);
            buildBuffer.Clear();

            foreach (var registration in registrations)
            {
                if (registration.InterfaceTypes is IReadOnlyList<Type> interfaceTypes)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < interfaceTypes.Count; i++)
                    {
                        AddToBuildBuffer(buildBuffer, interfaceTypes[i], registration);
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
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < collectionRegistration.InterfaceTypes.Count; i++)
            {
                var collectionType = collectionRegistration.InterfaceTypes[i];
                try
                {
                    buf.Add(collectionType, collectionRegistration);
                }
                catch (ArgumentException)
                {
                    throw new VContainerException(collectionType, $"Registration with the same key already exists: {collectionRegistration}");
                }
            }
        }

        FixedTypeKeyHashTableRegistry(FixedTypeKeyHashtable<IRegistration> hashTable)
        {
            this.hashTable = hashTable;
        }

        public bool TryGet(Type interfaceType, out IRegistration registration)
        {
            if (hashTable.TryGet(interfaceType, out registration))
                return registration != null;

            if (interfaceType.IsConstructedGenericType)
            {
                var genericType = interfaceType.GetGenericTypeDefinition();
                return TryFallbackSingleCollection(interfaceType, genericType, out registration);
            }
            return false;
        }

        public bool Exists(Type type) => hashTable.TryGet(type, out _);

        bool TryFallbackSingleCollection(Type interfaceType, Type genericType, out IRegistration registration)
        {
            if (genericType == typeof(IEnumerable<>) ||
                genericType == typeof(IReadOnlyList<>))
            {
                var elementType = interfaceType.GetGenericArguments()[0];
                var collectionRegistration = new CollectionRegistration(elementType);
                // ReSharper disable once InconsistentlySynchronizedField
                if (hashTable.TryGet(elementType, out var elementRegistration) && elementRegistration != null)
                {
                    collectionRegistration.Add(elementRegistration);
                }
                registration = collectionRegistration;
                return true;
            }
            registration = null;
            return false;
        }
    }
}
