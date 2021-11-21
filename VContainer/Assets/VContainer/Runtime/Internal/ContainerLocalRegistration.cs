using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    public sealed class ContainerLocalRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly IRegistration valueRegistration;

        public ContainerLocalRegistration(Type implementationType, IRegistration valueRegistration)
        {
            ImplementationType = implementationType;
            Lifetime = valueRegistration.Lifetime;
            this.valueRegistration = valueRegistration;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var value = resolver.Resolve(valueRegistration);
            var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(1);
            try
            {
                parameterValues[0] = value;
                return Activator.CreateInstance(ImplementationType, parameterValues);
            }
            finally
            {
                CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
            }
        }
    }
}