using System;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Internal
{
    public sealed class Registry
    {
        [ThreadStatic]
        static IDictionary<Type, Registration> buildBuffer = new Dictionary<Type, Registration>(128);

        [ThreadStatic]
        static IDictionary<(Type, object), Registration> buildBufferWithId = new Dictionary<(Type, object), Registration>(128);

        readonly FixedTypeKeyHashtable<Registration> hashTable;
        readonly IDictionary<(Type, object), Registration> identifiedRegistrations;

        public static Registry Build(Registration[] registrations)
        {
            // ThreadStatic
            if (buildBuffer == null)
                buildBuffer = new Dictionary<Type, Registration>(128);
            buildBuffer.Clear();

            if (buildBufferWithId == null)
                buildBufferWithId = new Dictionary<(Type, object), Registration>(128);
            buildBufferWithId.Clear();

            foreach (var registration in registrations)
            {
                if (registration.InterfaceTypes is IReadOnlyList<Type> interfaceTypes)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < interfaceTypes.Count; i++)
                    {
                        AddToBuildBuffer(buildBuffer, buildBufferWithId, interfaceTypes[i], registration);
                    }

                    // Mark the ImplementationType with a guard because we need to check if it exists later.
                    if (registration.Identifier != null)
                    {
                        var key = (registration.ImplementationType, registration.Identifier);
                        if (!buildBufferWithId.ContainsKey(key))
                        {
                            buildBufferWithId.Add(key, registration);
                        }
                    }
                    else if (!buildBuffer.ContainsKey(registration.ImplementationType))
                    {
                        buildBuffer.Add(registration.ImplementationType, null);
                    }
                }
                else
                {
                    AddToBuildBuffer(buildBuffer, buildBufferWithId, registration.ImplementationType, registration);
                }
            }

            var hashTable = new FixedTypeKeyHashtable<Registration>(buildBuffer.ToArray());
            return new Registry(hashTable, buildBufferWithId);
        }

        static void AddToBuildBuffer(
            IDictionary<Type, Registration> buf, 
            IDictionary<(Type, object), Registration> bufWithId, 
            Type service, 
            Registration registration)
        {
            if (registration.Identifier != null)
            {
                var key = (service, registration.Identifier);
                if (bufWithId.TryGetValue(key, out var existsWithId) && existsWithId != null)
                {
                    // Overwritten by the later registration with same id
                    bufWithId[key] = registration;
                }
                else
                {
                    bufWithId.Add(key, registration);
                }
                return;
            }

            if (buf.TryGetValue(service, out var exists) && exists != null)
            {
                CollectionInstanceProvider collection;
                if (buf.TryGetValue(RuntimeTypeCache.EnumerableTypeOf(service), out var found) &&
                    found.Provider is CollectionInstanceProvider foundCollection)
                {
                    collection = foundCollection;
                }
                else
                {
                    collection = new CollectionInstanceProvider(service) { exists };
                    var newRegistration = new Registration(
                        RuntimeTypeCache.ArrayTypeOf(service),
                        Lifetime.Transient,
                        new List<Type>
                        {
                            RuntimeTypeCache.EnumerableTypeOf(service),
                            RuntimeTypeCache.ReadOnlyListTypeOf(service),
                        }, collection);
                    AddCollectionToBuildBuffer(buf, bufWithId, newRegistration);
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

        static void AddCollectionToBuildBuffer(
            IDictionary<Type, Registration> buf, 
            IDictionary<(Type, object), Registration> bufWithId, 
            Registration collectionRegistration)
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

        Registry(FixedTypeKeyHashtable<Registration> hashTable, IDictionary<(Type, object), Registration> identifiedRegistrations)
        {
            this.hashTable = hashTable;
            this.identifiedRegistrations = identifiedRegistrations;
        }

        public bool TryGet(Type interfaceType, out Registration registration) => 
            TryGet(interfaceType, null, out registration);

        public bool TryGet(Type interfaceType, object identifier, out Registration registration)
        {
            if (interfaceType == null)
            {
                registration = null;
                return false;
            }
            
            if (identifier != null)
            {
                var key = (interfaceType, identifier);
                if (identifiedRegistrations.TryGetValue(key, out registration))
                {
                    return true;
                }
            }

            if (hashTable.TryGet(interfaceType, out registration))
            {
                return registration != null;
            }

            if (interfaceType.IsConstructedGenericType)
            {
                var openGenericType = RuntimeTypeCache.OpenGenericTypeOf(interfaceType);
                var typeParameters = RuntimeTypeCache.GenericTypeParametersOf(interfaceType);
                return TryGetClosedGenericRegistration(interfaceType, openGenericType, typeParameters, identifier, out registration) ||
                       TryFallbackToSingleElementCollection(interfaceType, openGenericType, typeParameters, out registration) ||
                       TryFallbackToContainerLocal(interfaceType, openGenericType, typeParameters, out registration);
            }
            return false;
        }

        bool TryGetClosedGenericRegistration(
            Type interfaceType, 
            Type openGenericType,
            Type[] typeParameters,
            object identifier,
            out Registration registration)
        {
            if (openGenericType == null || typeParameters == null)
            {
                registration = null;
                return false;
            }
            
            if (identifier != null)
            {
                var key = (openGenericType, identifier);
                if (identifiedRegistrations.TryGetValue(key, out var openGenericRegistration) && 
                    openGenericRegistration != null && 
                    openGenericRegistration.Provider is OpenGenericInstanceProvider implementationRegistration)
                {
                    registration = implementationRegistration.GetClosedRegistration(interfaceType, typeParameters, identifier);
                    return true;
                }
            }

            if (hashTable.TryGet(openGenericType, out var openGenericReg) && 
                openGenericReg != null && 
                openGenericReg.Provider is OpenGenericInstanceProvider implementationReg)
            {
                registration = implementationReg.GetClosedRegistration(interfaceType, typeParameters);
                return true;
            }

            registration = null;
            return false;
        }

        public bool Exists(Type type) => Exists(type, null);

        public bool Exists(Type type, object identifier)
        {
            if (identifier != null)
            {
                var key = (type, identifier);
                if (identifiedRegistrations.ContainsKey(key))
                    return true;
            }

            if (hashTable.TryGet(type, out _))
                return true;

            if (type.IsConstructedGenericType)
            {
                type = RuntimeTypeCache.OpenGenericTypeOf(type);

                if (identifier != null)
                {
                    var key = (type, identifier);
                    if (identifiedRegistrations.ContainsKey(key))
                        return true;
                }
            }

            return hashTable.TryGet(type, out _);
        }

        bool TryFallbackToContainerLocal(
            Type closedGenericType,
            Type openGenericType,
            IReadOnlyList<Type> typeParameters,
            out Registration newRegistration)
        {
            if (openGenericType != null && typeParameters != null && openGenericType == typeof(ContainerLocal<>) && typeParameters.Count > 0)
            {
                var valueType = typeParameters[0];
                if (valueType != null && TryGet(valueType, null, out var valueRegistration))
                {
                    var spawner = new ContainerLocalInstanceProvider(closedGenericType, valueRegistration);
                    newRegistration = new Registration(closedGenericType, Lifetime.Scoped, null, spawner);
                    return true;
                }
            }
            newRegistration = null;
            return false;
        }

        bool TryFallbackToSingleElementCollection(
            Type closedGenericType,
            Type openGenericType,
            IReadOnlyList<Type> typeParameters,
            out Registration newRegistration)
        {
            if (openGenericType != null && typeParameters != null && CollectionInstanceProvider.Match(openGenericType) && typeParameters.Count > 0)
            {
                var elementType = typeParameters[0];
                if (elementType != null)
                {
                    var collection = new CollectionInstanceProvider(elementType);
                    // ReSharper disable once InconsistentlySynchronizedField
                    if (hashTable.TryGet(elementType, out var elementRegistration) && elementRegistration != null)
                    {
                        collection.Add(elementRegistration);
                    }
                    
                    newRegistration = new Registration(
                        RuntimeTypeCache.ArrayTypeOf(elementType),
                        Lifetime.Transient,
                        new List<Type>
                        {
                            RuntimeTypeCache.EnumerableTypeOf(elementType),
                            RuntimeTypeCache.ReadOnlyListTypeOf(elementType),
                        }, collection);
                    return true;
                }
            }
            newRegistration = null;
            return false;
        }
    }
}
