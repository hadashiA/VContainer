using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer.Runtime.Internal.InstanceProviders
{
    public class OpenGenericInstanceProvider : IInstanceProvider
    {
        readonly Type ImplementationType;
        readonly Lifetime Lifetime;
        readonly IReadOnlyList<IInjectParameter> customParameters;

        private readonly ConcurrentDictionary<Type, Registration> _registrations =
            new ConcurrentDictionary<Type, Registration>();

        public OpenGenericInstanceProvider(Type implementationType, Lifetime lifetime,
            List<IInjectParameter> injectParameters)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
            customParameters = injectParameters;
        }

        public Registration GetClosedRegistration(Type interfaceType)
        {
            var registration = _registrations.GetOrAdd(interfaceType, _ =>
            {
                var genericArguments = interfaceType.GetGenericArguments();
                var newType = ImplementationType.MakeGenericType(genericArguments);

                var injector = InjectorCache.GetOrBuild(newType);
                var spawner = new InstanceProvider(injector, customParameters);
                return new Registration(
                    newType,
                    Lifetime,
                    new List<Type>(1) {interfaceType},
                    spawner);
            });

            return registration;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            throw new InvalidOperationException();
        }
    }
}