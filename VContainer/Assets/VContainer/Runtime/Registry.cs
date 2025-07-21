using System;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Internal
{
    public sealed class Registry
    {
        private static readonly object AnyKey = new { Any = true };
        
        [ThreadStatic]
        static IDictionary<(Type, object), Registration> buildBuffer = new Dictionary<(Type, object), Registration>(256);

        readonly FixedTypeObjectKeyHashtable<Registration> hashTable;

        public static Registry Build(Registration[] registrations)
        {
            // ThreadStatic
            if (buildBuffer == null)
                buildBuffer = new Dictionary<(Type, object), Registration>(256);
            buildBuffer.Clear();

            foreach (var registration in registrations)
            {
                var key = (registration.ImplementationType, key: registration.Key);
                
                if (registration.InterfaceTypes is IReadOnlyList<Type> interfaceTypes)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < interfaceTypes.Count; i++)
                    {
                        AddToBuildBuffer(buildBuffer, interfaceTypes[i], registration);
                    }

                    // Mark the ImplementationType with a guard because we need to check if it exists later.
                    if (!buildBuffer.ContainsKey(key))
                    {
                        buildBuffer.Add(key, null);
                    }
                }
                else
                {
                    AddToBuildBuffer(buildBuffer, registration.ImplementationType, registration);
                }
            }

            var hashTable = new FixedTypeObjectKeyHashtable<Registration>(buildBuffer.ToArray());
            return new Registry(hashTable);
        }

        static void AddToBuildBuffer(IDictionary<(Type, object), Registration> buf, Type service, Registration registration)
        {
            var key = (service, key: registration.Key);
            var collectionKey = (service, AnyKey);

            if (buf.TryGetValue(collectionKey, out var exists) && exists != null)
            {
                CollectionInstanceProvider collection;
                if (buf.TryGetValue((RuntimeTypeCache.EnumerableTypeOf(service), null), out var found) &&
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
                    AddCollectionToBuildBuffer(buf, newRegistration);
                }
                collection.Add(registration);

                // Overwritten by the later registration
                buf[collectionKey] = registration;
                buf[key] = registration;
            }
            else
            {
                buf.Add(key, registration);
                buf.Add(collectionKey, registration);
            }
        }

        static void AddCollectionToBuildBuffer(IDictionary<(Type, object), Registration> buf, Registration collectionRegistration)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < collectionRegistration.InterfaceTypes.Count; i++)
            {
                var collectionType = collectionRegistration.InterfaceTypes[i];
                try
                {
                    buf.Add((collectionType, null), collectionRegistration);
                }
                catch (ArgumentException)
                {
                    throw new VContainerException(collectionType, $"Registration with the same key already exists: {collectionRegistration}");
                }
            }
        }

        Registry(FixedTypeObjectKeyHashtable<Registration> hashTable)
        {
            this.hashTable = hashTable;
        }

        public bool TryGet(Type interfaceType, object key, out Registration registration)
        {
            if (hashTable.TryGet(interfaceType, key, out registration))
                return registration != null;

            if (interfaceType.IsConstructedGenericType)
            {
                var openGenericType = RuntimeTypeCache.OpenGenericTypeOf(interfaceType);
                var typeParameters = RuntimeTypeCache.GenericTypeParametersOf(interfaceType);
                return TryGetClosedGenericRegistration(interfaceType, key, openGenericType, typeParameters, out registration) ||
                       TryFallbackToSingleElementCollection(interfaceType, openGenericType, typeParameters, out registration) ||
                       TryFallbackToContainerLocal(interfaceType, openGenericType, key, typeParameters, out registration);
            }
            return false;
        }

        bool TryGetClosedGenericRegistration(Type interfaceType, object key, Type openGenericType,
            Type[] typeParameters,
            out Registration registration)
        {
            if (hashTable.TryGet(openGenericType, key, out var openGenericRegistration))
            {
                if (openGenericRegistration.Provider is OpenGenericInstanceProvider implementationRegistration)
                {
                    registration = implementationRegistration.GetClosedRegistration(interfaceType, typeParameters);
                    return true;
                }
            }

            registration = null;
            return false;
        }

        public bool Exists(Type type, object key)
        {
            if (hashTable.TryGet(type, key, out _))
                return true;

            if (type.IsConstructedGenericType)
            {
                type = RuntimeTypeCache.OpenGenericTypeOf(type);
            }

            return hashTable.TryGet(type, key, out _);
        }

        bool TryFallbackToContainerLocal(
            Type closedGenericType,
            Type openGenericType, 
            object key,
            IReadOnlyList<Type> typeParameters,
            out Registration newRegistration)
        {
            if (openGenericType == typeof(ContainerLocal<>))
            {
                var valueType = typeParameters[0];
                if (TryGet(valueType, key, out var valueRegistration))
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
            if (CollectionInstanceProvider.Match(openGenericType))
            {
                var elementType = typeParameters[0];
                var collection = new CollectionInstanceProvider(elementType);
                
                if (hashTable.TryGet(elementType, AnyKey, out var elementRegistration) && elementRegistration != null)
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
            newRegistration = null;
            return false;
        }
    }
}
