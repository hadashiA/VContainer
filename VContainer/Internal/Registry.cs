using System;
using System.Collections;

namespace VContainer.Internal
{
    public readonly struct LookUpKey // TODO:
    {
    }

    interface IRegistry
    {
        void Add(Type interfaceType, Registration registration);
        Registration Get(Type interfaceType);
        bool TryGet(Type interfaceType, out Registration registration);
    }

    public class HashTableRegistry : IRegistry
    {
        readonly object syncRoot = new object();
        readonly Hashtable registrations = new Hashtable();

        public void Add(Type service, Registration registration)
        {
            try
            {
                lock (syncRoot)
                {
                    registrations.Add(service, registration);
                }
            }
            catch (Exception ex)
            {
                throw new VContainerException($"Registration with the same key already exists: {registration}");
            }
        }

        public Registration Get(Type interfaceType)
        {
            if (registrations[interfaceType] is Registration registration)
            {
                return registration;
            }
            throw new VContainerException($"No such registration of type: {interfaceType.FullName}");
        }

        public bool TryGet(Type interfaceType, out Registration registration)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            registration = registrations[interfaceType] as Registration;
            return registration != null;
        }
    }
}
