using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    interface IRegistry
    {
        // void Add(Registration registration);
        bool TryGet(Type interfaceType, out IRegistration registration);
    }

    static class IRegistryExtensions
    {
    //     public static void AddSystemRegistration(
    //         this IRegistry registry,
    //         IObjectResolver container,
    //         bool includesContainer)
    //     {
    //         if (includesContainer)
    //         {
    //             var containerType = container.GetType();
    //             var injector = new NullInjector(container);
    //             registry.Add(new Registration(
    //                 containerType,
    //                 new List<Type>{ typeof(IObjectResolver) },
    //                 Lifetime.Transient,
    //                 injector,
    //                 container));
    //         }
    //     }
    }
}