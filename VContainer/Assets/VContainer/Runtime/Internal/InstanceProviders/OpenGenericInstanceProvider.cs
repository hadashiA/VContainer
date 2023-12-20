using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VContainer.Internal
{
    public class OpenGenericInstanceProvider : IInstanceProvider
    {
        class TypeParametersEqualityComparer : IEqualityComparer<Type[]>
        {
            public bool Equals(Type[] x, Type[] y)
            {
                if (x == null || y == null) return x == y;
                if (x.Length != y.Length) return false;

                for (var i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i]) return false;
                }
                return true;
            }

            public int GetHashCode(Type[] typeParameters)
            {
                var hash = 5381;
                foreach (var typeParameter in typeParameters)
                {
                    hash = ((hash << 5) + hash) ^ typeParameter.GetHashCode();
                }
                return hash;
            }
        }

        readonly Lifetime lifetime;
        readonly Type implementationType;
        readonly IReadOnlyList<IInjectParameter> customParameters;

        readonly ConcurrentDictionary<Type[], Registration> constructedRegistrations = new ConcurrentDictionary<Type[], Registration>(new TypeParametersEqualityComparer());
        readonly Func<Type[], Registration> createRegistrationFunc;

        public OpenGenericInstanceProvider(Type implementationType, Lifetime lifetime, List<IInjectParameter> injectParameters)
        {
            this.implementationType = implementationType;
            this.lifetime = lifetime;
            customParameters = injectParameters;
            createRegistrationFunc = CreateRegistration;
        }

        public Registration GetClosedRegistration(Type closedInterfaceType, Type[] typeParameters)
        {
            return constructedRegistrations.GetOrAdd(typeParameters, createRegistrationFunc);
        }

        Registration CreateRegistration(Type[] typeParameters)
        {
            var newType = implementationType.MakeGenericType(typeParameters);
            var injector = InjectorCache.GetOrBuild(newType);
            var spawner = new InstanceProvider(injector, customParameters);
            return new Registration(newType, lifetime, new List<Type>(1) { newType }, spawner);
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            throw new InvalidOperationException();
        }
   }
}