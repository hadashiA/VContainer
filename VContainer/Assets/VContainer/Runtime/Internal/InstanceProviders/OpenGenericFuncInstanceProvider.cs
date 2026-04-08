using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TypeParametersKey = VContainer.Internal.OpenGenericTypeParametersKey;

namespace VContainer.Internal
{
    public class OpenGenericFuncInstanceProvider : IInstanceProvider, IClosedRegistrationProvider
    {
        readonly Type implementationType;
        readonly Lifetime lifetime;
        readonly Func<IObjectResolver, Type[], object> factory;

        readonly ConcurrentDictionary<TypeParametersKey, Registration> constructedRegistrations = new ConcurrentDictionary<TypeParametersKey, Registration>();
        readonly Func<TypeParametersKey, Registration> createRegistrationFunc;

        public OpenGenericFuncInstanceProvider(Type implementationType, Lifetime lifetime, Func<IObjectResolver, Type[], object> factory)
        {
            this.implementationType = implementationType;
            this.lifetime = lifetime;
            this.factory = factory;
            createRegistrationFunc = CreateRegistration;
        }

        public Registration GetClosedRegistration(Type closedInterfaceType, Type[] typeParameters, object key = null)
        {
            var typeParametersKey = new TypeParametersKey(typeParameters, key);
            return constructedRegistrations.GetOrAdd(typeParametersKey, createRegistrationFunc);
        }

        Registration CreateRegistration(TypeParametersKey key)
        {
            var newType = implementationType.MakeGenericType(key.TypeParameters);
            var spawner = new FuncInstanceProvider(resolver => factory(resolver, key.TypeParameters));
            return new Registration(newType, lifetime, new List<Type>(1) { newType }, spawner, key.Key);
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            throw new InvalidOperationException();
        }
    }
}