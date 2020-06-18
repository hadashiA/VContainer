using System;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Internal
{
    public sealed class FixedTypeKeyHashTableRegistry : IRegistry
    {
        readonly FixedTypeKeyHashtable<IRegistration> hashTable;

        public static FixedTypeKeyHashTableRegistry Build(IList<RegistrationBuilder> registrationBuilders)
        {
            var dictionary = new Dictionary<Type, IRegistration>(registrationBuilders.Count * 2);
            foreach (var registrationBuilder in registrationBuilders)
            {
                var registration = registrationBuilder.Build();
                if (registration.InterfaceTypes?.Count > 0)
                {
                    foreach (var interfaceType in registration.InterfaceTypes)
                    {
                        AddToDictionary(dictionary, interfaceType, registration);
                    }
                }
                else
                {
                    AddToDictionary(dictionary, registration.ImplementationType, registration);
                }
            }

            var hashTable = new FixedTypeKeyHashtable<IRegistration>(dictionary.ToArray());
            return new FixedTypeKeyHashTableRegistry(hashTable);
        }

        static void AddToDictionary(IDictionary<Type, IRegistration> dictionary, Type service, Registration registration)
        {
            if (dictionary.TryGetValue(service, out var exists))
            {
                switch (exists)
                {
                    case CollectionRegistration existsCollection:
                        existsCollection.Add(registration);
                        break;
                    case Registration existsRegistration:
                        var collectionService = typeof(IEnumerable<>).MakeGenericType(service);
                        if (dictionary.TryGetValue(collectionService, out var collectionRegistration))
                        {
                            ((CollectionRegistration)collectionRegistration).Add(registration);
                        }
                        else
                        {
                            collectionRegistration = new CollectionRegistration(service) { existsRegistration, registration };
                            AddCollectionToDictionary(dictionary, (CollectionRegistration)collectionRegistration);
                        }
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            else
            {
                dictionary.Add(service, registration);
            }
        }

        static void AddCollectionToDictionary(IDictionary<Type, IRegistration> dictionary, CollectionRegistration collectionRegistration)
        {
            foreach (var collectionType in collectionRegistration.InterfaceTypes)
            {
                try
                {
                    dictionary.Add(collectionType, collectionRegistration);
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
                    // ReSharper disable once InconsistentlySynchronizedField
                    if (hashTable.TryGet(elementType, out var elementRegistration))
                    {
                        registration = new CollectionRegistration(elementType) { (Registration)elementRegistration };
                        return true;
                    }
                }
            }
            return false;
        }
    }
}