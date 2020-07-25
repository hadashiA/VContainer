using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Internal;

namespace VContainer.Unity
{
    public sealed class ComponentRegistrationBuilder : RegistrationBuilder
    {
        Transform parent;
        Func<Transform> parentFinder;
        string gameObjectName;
        Component prefab;

        internal ComponentRegistrationBuilder(
            Type implementationType,
            Lifetime lifetime,
            List<Type> interfaceTypes = null)
            : base(implementationType, lifetime, interfaceTypes)
        {
            InterfaceTypes = InterfaceTypes ?? new List<Type>();
            InterfaceTypes.Add(typeof(MonoBehaviour));
            InterfaceTypes.Add(ImplementationType);
        }

        internal ComponentRegistrationBuilder(
            Component prefab,
            Type implementationType,
            Lifetime lifetime,
            List<Type> interfaceTypes = null)
            : this(implementationType, lifetime, interfaceTypes)
        {
            this.prefab = prefab;
        }

        internal ComponentRegistrationBuilder(
            string gameObjectName,
            Type implementationType,
            Lifetime lifetime,
            List<Type> interfaceTypes = null)
            : this(implementationType, lifetime, interfaceTypes)
        {
            this.gameObjectName = gameObjectName;
        }

        public override IRegistration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);
            var destination = new ComponentDestination(prefab, parent, parentFinder, gameObjectName);

            return new ComponentRegistration(
                ImplementationType,
                Lifetime,
                InterfaceTypes,
                Parameters,
                injector,
                destination);
        }

        public ComponentRegistrationBuilder UnderTransform(Transform parent)
        {
            this.parent = parent;
            return this;
        }

        public ComponentRegistrationBuilder UnderTransform(Func<Transform> parentFinder)
        {
            this.parentFinder = parentFinder;
            return this;
        }
   }
}