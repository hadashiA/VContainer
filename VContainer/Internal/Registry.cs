using System;
using System.Collections;

namespace VContainer.Internal
{
    public readonly struct LookUpKey // TODO:
    {
    }

    interface IRegistry
    {
        void Add(Type service, Registration registration);
    }

    public class HashTableRegistry : IRegistry
    {
        readonly Hashtable hashtable = new Hashtable();

        public void Add(Type service, Registration registration)
        {
            try
            {
                hashtable.Add(service, registration);
            }
            catch (Exception ex)
            {
                hashtable.Clear();
                throw new VContainerException($"Registration with the same key already exists: {registration}");
            }
        }
    }
}
