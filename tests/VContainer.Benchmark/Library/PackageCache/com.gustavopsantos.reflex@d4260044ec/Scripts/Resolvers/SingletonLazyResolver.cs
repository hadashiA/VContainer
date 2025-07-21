using System;

namespace Reflex
{
    internal class SingletonLazyResolver : Resolver
    {
        internal override object Resolve(Type contract, Container container)
        {
            if (container.TryGetSingletonInstance(contract, out var instance))
            {
                return instance;
            }

            return CreateAndRegisterSingletonInstance(contract, container);
        }

        private static object CreateAndRegisterSingletonInstance(Type contract, Container container)
        {
            var concrete = container.GetConcreteTypeFor(contract);
            var instance = container.Construct(concrete);
            return container.RegisterSingletonInstance(contract, instance);
        }
    }
}