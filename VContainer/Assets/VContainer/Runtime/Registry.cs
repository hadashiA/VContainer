using System;
using System.Collections.Generic;
using System.Linq;
using Unity.IL2CPP.CompilerServices;

namespace VContainer.Internal
{
    [Il2CppSetOptionAttribute(Option.NullChecks, false)]
    public sealed class Registry
    {
        [ThreadStatic]
        static IDictionary<Type, IRegistration> buildBuffer = new Dictionary<Type, IRegistration>(128);

        readonly FixedTypeKeyHashtable<IRegistration> hashTable;

        public static Registry Build(IRegistration[] registrations)
        {
            // ThreadStatic
            if (buildBuffer == null)
                buildBuffer = new Dictionary<Type, IRegistration>(128);
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

            var hashTable = new FixedTypeKeyHashtable<IRegistration>(buildBuffer.ToArray());
            return new Registry(hashTable);
        }

        static void AddToBuildBuffer(IDictionary<Type, IRegistration> buf, Type service, IRegistration registration)
        {
            if (buf.TryGetValue(service, out var exists))
            {
                CollectionRegistration collection;
                if (buf.TryGetValue(RuntimeTypeCache.EnumerableTypeOf(service), out var found))
                {
                    collection = (CollectionRegistration)found;
                }
                else
                {
                    collection = new CollectionRegistration(service) { exists };
                    AddCollectionToBuildBuffer(buf, collection);
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

        static void AddCollectionToBuildBuffer(IDictionary<Type, IRegistration> buf, CollectionRegistration collectionRegistration)
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

        Registry(FixedTypeKeyHashtable<IRegistration> hashTable)
        {
            this.hashTable = hashTable;
        }

        public bool TryGet(Type interfaceType, out IRegistration registration)
        {
            if (hashTable.TryGet(interfaceType, out registration))
                return registration != null;

            if (interfaceType.IsConstructedGenericType)
            {
                var openGenericType = RuntimeTypeCache.OpenGenericTypeOf(interfaceType);
                var typeParameters = RuntimeTypeCache.GenericTypeParametersOf(interfaceType);
                return TryFallbackToSingleElementCollection(interfaceType, openGenericType, typeParameters, out registration) ||
                       TryFallbackToContainerLocal(interfaceType, openGenericType, typeParameters, out registration);
            }
            return false;
        }

        public bool Exists(Type type) => hashTable.TryGet(type, out _);

        bool TryFallbackToContainerLocal(
            Type closedGenericType,
            Type openGenericType,
            IReadOnlyList<Type> typeParameters,
            out IRegistration newRegistration)
        {
            if (openGenericType == typeof(ContainerLocal<>))
            {
                var valueType = typeParameters[0];
                if (TryGet(valueType, out var valueRegistration))
                {
                    newRegistration = new ContainerLocalRegistration(closedGenericType, valueRegistration);
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
            out IRegistration newRegistration)
        {
            if (CollectionRegistration.Match(openGenericType))
            {
                var elementType = typeParameters[0];
                var collectionRegistration = new CollectionRegistration(elementType);
                // ReSharper disable once InconsistentlySynchronizedField
                if (hashTable.TryGet(elementType, out var elementRegistration) && elementRegistration != null)
                {
                    collectionRegistration.Add(elementRegistration);
                }
                newRegistration = collectionRegistration;
                return true;
            }
            newRegistration = null;
            return false;
        }
    }
}
