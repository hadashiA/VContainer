using System;
using System.Collections;
using System.Collections.Generic;

namespace VContainer.Internal
{
    // Currentry using FixedTypeKeyHashTable !
    // public sealed class HashTableRegistry : IRegistry
    // {
    //     readonly object syncRoot = new object();
    //     readonly Hashtable registrations = new Hashtable(64);
    //
    //     public void Add(Registration registration)
    //     {
    //         if (registration.InterfaceTypes?.Count > 0)
    //         {
    //             foreach (var contractType in registration.InterfaceTypes)
    //             {
    //                 Add(contractType, registration);
    //             }
    //         }
    //         else
    //         {
    //             Add(registration.ImplementationType, registration);
    //         }
    //     }
    //
    //     public bool TryGet(Type interfaceType, out IRegistration registration)
    //     {
    //         // ReSharper disable once InconsistentlySynchronizedField
    //         registration = registrations[interfaceType] as IRegistration;
    //         if (registration != null) return true;
    //
    //         // Auto falling back to collection..
    //         if (interfaceType.IsGenericType)
    //         {
    //             // TODO:
    //             var genericType = interfaceType.GetGenericTypeDefinition();
    //             if (genericType == typeof(IEnumerable<>) ||
    //                 genericType == typeof(IReadOnlyList<>))
    //             {
    //                 var elementType = interfaceType.GetGenericArguments()[0];
    //                 // ReSharper disable once InconsistentlySynchronizedField
    //                 if (registrations[elementType] is Registration elementRegistration)
    //                 {
    //                     var collectionRegistration = new CollectionRegistration(elementType) { elementRegistration };
    //                     AddCollection(collectionRegistration);
    //                     return TryGet(interfaceType, out registration);
    //                 }
    //             }
    //         }
    //         return false;
    //     }
    //
    //     void Add(Type service, Registration registration)
    //     {
    //         // ReSharper disable once InconsistentlySynchronizedField
    //         switch (registrations[service])
    //         {
    //             case CollectionRegistration existsCollection:
    //                 lock (syncRoot)
    //                 {
    //                     existsCollection.Add(registration);
    //                 }
    //                 break;
    //             case Registration exists:
    //                 var collectionService = typeof(IEnumerable<>).MakeGenericType(service);
    //                 // ReSharper disable once InconsistentlySynchronizedField
    //                 if (registrations[collectionService] is CollectionRegistration collectionRegistration)
    //                 {
    //                     collectionRegistration.Add(registration);
    //                 }
    //                 else
    //                 {
    //                     collectionRegistration = new CollectionRegistration(service) { exists, registration };
    //                     AddCollection(collectionRegistration);
    //                 }
    //                 break;
    //             case null:
    //                 lock (syncRoot)
    //                 {
    //                     registrations.Add(service, registration);
    //                 }
    //                 break;
    //         }
    //     }
    //
    //     void AddCollection(CollectionRegistration collectionRegistration)
    //     {
    //         lock (syncRoot)
    //         {
    //             foreach (var collectionType in collectionRegistration.InterfaceTypes)
    //             {
    //                 try
    //                 {
    //                     registrations.Add(collectionType, collectionRegistration);
    //                 }
    //                 catch (ArgumentException)
    //                 {
    //                     throw new VContainerException(collectionType, $"Registration with the same key already exists: {collectionType} {collectionRegistration}");
    //                 }
    //             }
    //         }
    //     }
    // }
}
