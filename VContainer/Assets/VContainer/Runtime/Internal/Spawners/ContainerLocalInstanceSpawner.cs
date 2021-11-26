using System;

namespace VContainer.Internal
{
    public sealed class ContainerLocalInstanceSpawner : IInstanceSpawner
    {
        readonly Type wrappedType;
        readonly Registration valueRegistration;

        public ContainerLocalInstanceSpawner(Type wrappedType, Registration valueRegistration)
        {
            this.wrappedType = wrappedType;
            this.valueRegistration = valueRegistration;
        }

        public object Spawn(IObjectResolver resolver)
        {
            var value = resolver.Resolve(valueRegistration);
            var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(1);
            try
            {
                parameterValues[0] = value;
                return Activator.CreateInstance(wrappedType, parameterValues);
            }
            finally
            {
                CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
            }
        }
    }
}