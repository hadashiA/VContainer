using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VContainer.Internal
{
    public class OpenGenericInstanceProvider : IInstanceProvider
    {
        class TypeParametersKey
        {
            public readonly Type[] TypeParameters;
            public readonly object Key;

            public TypeParametersKey(Type[] typeParameters, object key)
            {
                TypeParameters = typeParameters;
                Key = key;
            }

            public override bool Equals(object obj)
            {
                if (obj is TypeParametersKey other)
                {
                    if (Key != other.Key)
                        return false;

                    if (TypeParameters.Length != other.TypeParameters.Length)
                        return false;

                    for (var i = 0; i < TypeParameters.Length; i++)
                    {
                        if (TypeParameters[i] != other.TypeParameters[i])
                            return false;
                    }
                    return true;
                }
                return false;
            }

            public override int GetHashCode()
            {
                var hash = 5381;
                foreach (var typeParameter in TypeParameters)
                {
                    hash = ((hash << 5) + hash) ^ typeParameter.GetHashCode();
                }
                hash = ((hash << 5) + hash) ^ (Key?.GetHashCode() ?? 0);
                return hash;
            }
        }

        readonly Lifetime lifetime;
        readonly Type implementationType;
        readonly IReadOnlyList<IInjectParameter> customParameters;

        readonly ConcurrentDictionary<TypeParametersKey, Registration> constructedRegistrations = new ConcurrentDictionary<TypeParametersKey, Registration>();
        readonly Func<TypeParametersKey, Registration> createRegistrationFunc;

        public OpenGenericInstanceProvider(Type implementationType, Lifetime lifetime, List<IInjectParameter> injectParameters)
        {
            this.implementationType = implementationType;
            this.lifetime = lifetime;
            customParameters = injectParameters;
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
            var injector = InjectorCache.GetOrBuild(newType);
            var spawner = new InstanceProvider(injector, customParameters);
            return new Registration(newType, lifetime, new List<Type>(1) { newType }, spawner, key.Key);
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            throw new InvalidOperationException();
        }
   }
}