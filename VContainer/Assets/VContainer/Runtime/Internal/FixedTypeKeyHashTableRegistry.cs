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
                if (registration.InterfaceTypes?.Count > 0)
                {
                    foreach (var interfaceType in registration.InterfaceTypes)
                    {
                        AddToBuildBuffer(buildBuffer, interfaceType, registration);
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
                if (buf.TryGetValue(collectionService, out var collection))
                {
                    ((CollectionRegistration)collection).Add(registration);
                }
                else
                {
                    var newCollection = new CollectionRegistration(service) { exists, registration };
                    AddCollectionToBuildBuffer(buf, newCollection);
                }
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

        FixedTypeKeyHashTableRegistry(FixedTypeKeyHashtable<IRegistration> hashTable)
        {
            this.hashTable = hashTable;
        }

        public bool TryGet(Type interfaceType, out IRegistration registration)
        {
            if (hashTable.TryGet(interfaceType, out registration))
                return true;

            // Auto falling back to collection from one
            if (interfaceType.IsGenericType)
            {
                // TODO:
                var genericType = interfaceType.GetGenericTypeDefinition();
                if (genericType == typeof(IEnumerable<>) ||
                    genericType == typeof(IReadOnlyList<>))
                {
                    var elementType = interfaceType.GetGenericArguments()[0];
                    var collectionRegistration = new CollectionRegistration(elementType);
                    // ReSharper disable once InconsistentlySynchronizedField
                    if (hashTable.TryGet(elementType, out var elementRegistration))
                    {
                        collectionRegistration.Add(elementRegistration);
                    }
                    registration = collectionRegistration;
                    return true;
                }
            }
            return false;
        }
    }
}