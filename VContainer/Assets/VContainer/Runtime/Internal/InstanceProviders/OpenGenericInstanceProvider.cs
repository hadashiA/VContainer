using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VContainer.Internal
{
    public class OpenGenericInstanceProvider : IInstanceProvider
    {
        readonly Type implementationType;
        readonly Lifetime lifetime;
        readonly IReadOnlyList<IInjectParameter> customParameters;
        readonly ConcurrentDictionary<Type, Registration> registrations;

        public OpenGenericInstanceProvider(Type implementationType, Lifetime lifetime,
            List<IInjectParameter> injectParameters)
        {
            this.implementationType = implementationType;
            this.lifetime = lifetime;
            customParameters = injectParameters;
            registrations = new ConcurrentDictionary<Type, Registration>();
        }

        public Registration GetClosedRegistration(Type interfaceType)
        {
            var registrationArgs = new RegistrationArguments()
            {
                ImplementationType = implementationType,
                Lifetime = lifetime,
                CustomParameters = customParameters,
            };
            return registrations.GetOrAdd(interfaceType, static (type, args) =>
                CreateRegistration(type, args), registrationArgs);
        }

        private static Registration CreateRegistration(Type type, RegistrationArguments args)
        {
            var genericArguments = type.GetGenericArguments();
            var newType = args.ImplementationType.MakeGenericType(genericArguments);

            var injector = InjectorCache.GetOrBuild(newType);
            var spawner = new InstanceProvider(injector, args.CustomParameters);
            return new Registration(newType, args.Lifetime, new List<Type>(1) { type }, spawner);
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            throw new InvalidOperationException();
        }

        private struct RegistrationArguments
        {
            public Type ImplementationType;
            public Lifetime Lifetime;
            public IReadOnlyList<IInjectParameter> CustomParameters;
        }
    }
}