using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VContainer.Internal
{
    public class OpenGenericInstanceProvider : IInstanceProvider
    {
        readonly Lifetime lifetime;
        readonly Type implementationType;
        readonly IReadOnlyList<IInjectParameter> customParameters;

        readonly ConcurrentDictionary<int, Registration> registrations;
        readonly ConcurrentDictionary<Type, int> typeParametersHashes;

        public OpenGenericInstanceProvider(Type implementationType, Lifetime lifetime,
            List<IInjectParameter> injectParameters)
        {
            this.implementationType = implementationType;
            this.lifetime = lifetime;
            customParameters = injectParameters;
            typeParametersHashes = new ConcurrentDictionary<Type, int>();
            registrations = new ConcurrentDictionary<int, Registration>();
        }

        public Registration GetClosedRegistration(Type closedInterfaceType, Type[] typeParameters)
        {
            var typeParametersHash = typeParametersHashes.GetOrAdd(closedInterfaceType, (_, arg) =>
                ((IStructuralEquatable)arg).GetHashCode(EqualityComparer<Type>.Default), typeParameters);

            var registrationArgs = new RegistrationArguments
            {
                ImplementationType = implementationType,
                Lifetime = lifetime,
                CustomParameters = customParameters,
                TypeParameters = typeParameters
            };

            return registrations.GetOrAdd(typeParametersHash, (_, args) => CreateRegistration(args), registrationArgs);
        }

        private static Registration CreateRegistration(RegistrationArguments args)
        {
            var newType = args.ImplementationType.MakeGenericType(args.TypeParameters);
            var injector = InjectorCache.GetOrBuild(newType);
            var spawner = new InstanceProvider(injector, args.CustomParameters);
            return new Registration(newType, args.Lifetime, new List<Type>(1) { newType }, spawner);
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
            public Type[] TypeParameters;
        }
    }
}