using System;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Internal
{
    public sealed class Registry
    {
        [ThreadStatic]
        static IDictionary<Type, Registration> buildBuffer = new Dictionary<Type, Registration>(128);

        readonly FixedTypeKeyHashtable<Registration> hashTable;

        public static Registry Build(Registration[] registrations)
        {
            // ThreadStatic
            if (buildBuffer == null)
                buildBuffer = new Dictionary<Type, Registration>(128);
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

            var hashTable = new FixedTypeKeyHashtable<Registration>(buildBuffer.ToArray());
            return new Registry(hashTable);
        }

        static void AddToBuildBuffer(IDictionary<Type, Registration> buf, Type service, Registration registration)
        {
            if (buf.TryGetValue(service, out var exists))
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
                    AddCollectionToBuildBuffer(buf, newRegistration);
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

        static void AddCollectionToBuildBuffer(IDictionary<Type, Registration> buf, Registration collectionRegistration)
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

        Registry(FixedTypeKeyHashtable<Registration> hashTable)
        {
            this.hashTable = hashTable;
        }

        public bool TryGet(Type interfaceType, out Registration registration)
        {
            if (hashTable.TryGet(interfaceType, out registration))
                return registration != null;

            if (interfaceType.IsConstructedGenericType)
            {
                var openGenericType = RuntimeTypeCache.OpenGenericTypeOf(interfaceType);
                var typeParameters = RuntimeTypeCache.GenericTypeParametersOf(interfaceType);
                return TryGetClosedGenericRegistration(interfaceType, openGenericType, typeParameters, out registration) ||
                       TryFallbackToSingleElementCollection(interfaceType, openGenericType, typeParameters, out registration) ||
                       TryFallbackToContainerLocal(interfaceType, openGenericType, typeParameters, out registration);
            }
            return false;
        }

        bool TryGetClosedGenericRegistration(Type interfaceType, Type openGenericType,
            Type[] typeParameters,
            out Registration registration)
        {
            if (hashTable.TryGet(openGenericType, out var openGenericRegistration))
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

        public bool Exists(Type type)
        {
            if (hashTable.TryGet(type, out _))
                return true;

            if (type.IsConstructedGenericType)
            {
                type = RuntimeTypeCache.OpenGenericTypeOf(type);
            }

            return hashTable.TryGet(type, out _);
        }

        bool TryFallbackToContainerLocal(
            Type closedGenericType,
            Type openGenericType,
            IReadOnlyList<Type> typeParameters,
            out Registration newRegistration)
        {
            if (openGenericType == typeof(ContainerLocal<>))
            {
                var valueType = typeParameters[0];
                if (TryGet(valueType, out var valueRegistration))
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
            newRegistration = null;
            return false;
        }
    }
}
