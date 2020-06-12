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

    public class HashTableRegistry : IRegistry
    {
        readonly object syncRoot = new object();
        readonly Hashtable registrations = new Hashtable();

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
            while (true)
            {
                // ReSharper disable once InconsistentlySynchronizedField
                registration = registrations[interfaceType] as IRegistration;

                // Auto fallback to collection..
                if (registration is null &&
                    interfaceType.IsGenericType &&
                    (typeof(IEnumerable).IsAssignableFrom(interfaceType) ||
                     interfaceType.GetGenericTypeDefinition().IsEquivalentTo(typeof(IReadOnlyList<>))))
                {
                    var elementType = interfaceType.GetGenericArguments()[0];
                    // ReSharper disable once InconsistentlySynchronizedField
                    if (registrations[elementType] is Registration elementRegistration)
                    {
                        registration = new CollectionRegistration(elementType) { elementRegistration };
                        lock (syncRoot)
                        {
                            foreach (var collectionType in registration.InterfaceTypes)
                            {
                                try
                                {
                                    registrations.Add(collectionType, registration);
                                }
                                catch (ArgumentException)
                                {
                                    throw new VContainerException($"Registration with the same key already exists: {collectionType} {registration}");
                                }
                            }
                        }
                    }
                }
                return registration != null;
            }
        }

        void Add(Type service, Registration registration)
        {
            lock (syncRoot)
            {
                switch (registrations[service])
                {
                    case CollectionRegistration existsCollection:
                        existsCollection.Add(registration);
                        break;
                    case Registration exists:
                        var collectionService = typeof(IEnumerable<>).MakeGenericType(service);
                        var collectionRegistration = registrations[collectionService] as CollectionRegistration ??
                                                     new CollectionRegistration(service) { exists, registration };
                        foreach (var collectionType in collectionRegistration.InterfaceTypes)
                        {
                            try
                            {
                                registrations.Add(collectionType, collectionRegistration);
                            }
                            catch (ArgumentException)
                            {
                                throw new VContainerException($"Registration with the same key already exists: {collectionType} {registration}");
                            }
                        }
                        break;
                    case null:
                        registrations.Add(service, registration);
                        break;
                }
            }
        }
    }
}
