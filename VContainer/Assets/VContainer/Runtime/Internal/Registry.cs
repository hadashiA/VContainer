using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Internal
{
    public readonly struct LookUpKey // TODO:
    {
    }

    interface IRegistry
    {
        void Add(Registration registration);
        IRegistration Get(Type interfaceType);
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

        public IRegistration Get(Type interfaceType)
        {
            if (registrations[interfaceType] is IRegistration registration)
            {
                return registration;
            }
            throw new VContainerException($"No such registration of type: {interfaceType.FullName}");
        }

        public bool TryGet(Type interfaceType, out IRegistration registration)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            registration = registrations[interfaceType] as IRegistration;
            return registration != null;
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
                            catch (ArgumentException _)
                            {
                                throw new VContainerException($"Registration with the same key already exists: {registration}");
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
