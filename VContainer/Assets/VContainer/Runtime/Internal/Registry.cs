using System;
using System.Collections;
using System.Collections.Generic;

namespace VContainer.Internal
{
    public readonly struct LookUpKey // TODO:
    {
    }

    interface IRegistry
    {
        void Add(Registration registration);
        bool TryGet(Type interfaceType, out IRegistration registration);
    }

    public sealed class HashTableRegistry : IRegistry
    {
        readonly object syncRoot = new object();
        readonly Hashtable registrations = new Hashtable(64);

        public void Add(Registration registration)
        {
            if (registration.InterfaceTypes?.Count > 0)
            {
                foreach (var contractType in registration.InterfaceTypes)
                {
                    Add(contractType, registration);
                }
            }
            else
            {
                Add(registration.ImplementationType, registration);
            }
        }

        public bool TryGet(Type interfaceType, out IRegistration registration)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            registration = registrations[interfaceType] as IRegistration;
            if (registration != null) return true;

            // Auto falling back to collection..
            if (interfaceType.IsGenericType &&
                // (interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                //  interfaceType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
                // Optimize for mono runtime
                interfaceType.GetInterface("IEnumerable") != null
                )
            {
                var elementType = interfaceType.GetGenericArguments()[0];
                // ReSharper disable once InconsistentlySynchronizedField
                if (registrations[elementType] is Registration elementRegistration)
                {
                    var collectionRegistration = new CollectionRegistration(elementType) { elementRegistration };
                    AddCollection(collectionRegistration);
                    return TryGet(interfaceType, out registration);
                }
            }
            return false;
        }

        void Add(Type service, Registration registration)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            switch (registrations[service])
            {
                case CollectionRegistration existsCollection:
                    lock (syncRoot)
                    {
                        existsCollection.Add(registration);
                    }
                    break;
                case Registration exists:
                    var collectionService = typeof(IEnumerable<>).MakeGenericType(service);
                    // ReSharper disable once InconsistentlySynchronizedField
                    var collectionRegistration = registrations[collectionService] as CollectionRegistration ??
                                                 new CollectionRegistration(service) { exists, registration };
                    AddCollection(collectionRegistration);
                    break;
                case null:
                    lock (syncRoot)
                    {
                        registrations.Add(service, registration);
                    }
                    break;
            }
        }

        void AddCollection(CollectionRegistration collectionRegistration)
        {
            lock (syncRoot)
            {
                foreach (var collectionType in collectionRegistration.InterfaceTypes)
                {
                    try
                    {
                        registrations.Add(collectionType, collectionRegistration);
                    }
                    catch (ArgumentException)
                    {
                        throw new VContainerException($"Registration with the same key already exists: {collectionType} {collectionRegistration}");
                    }
                }
            }
        }
    }
}
