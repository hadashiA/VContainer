using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Internal;

namespace VContainer.Unity
{
    public sealed class ComponentRegistrationBuilder : RegistrationBuilder
    {
        readonly bool find;
        readonly Scene scene;
        readonly Component prefab;

        Transform parent;
        Func<Transform> parentFinder;
        string gameObjectName;

        internal ComponentRegistrationBuilder(
            Component prefab,
            Type implementationType,
            Lifetime lifetime,
            List<Type> interfaceTypes = null)
            : this(false, default, implementationType, lifetime, interfaceTypes)
        {
            this.prefab = prefab;
        }

        internal ComponentRegistrationBuilder(
            string gameObjectName,
            Type implementationType,
            Lifetime lifetime,
            List<Type> interfaceTypes = null)
            : this(false, default, implementationType, lifetime, interfaceTypes)
        {
            this.gameObjectName = gameObjectName;
        }

        internal ComponentRegistrationBuilder(
            Scene scene,
            Type implementationType,
            List<Type> interfaceTypes = null)
            : this(true, scene, implementationType, Lifetime.Scoped, interfaceTypes)
        {
        }

        ComponentRegistrationBuilder(
            bool find,
            Scene scene,
            Type implementationType,
            Lifetime lifetime,
            List<Type> interfaceTypes = null)
            : base(implementationType, lifetime, interfaceTypes)
        {
            this.find = find;
            this.scene = scene;
        }

        public override IRegistration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);
            var destination = new ComponentDestination(find, scene, parent, parentFinder, prefab, gameObjectName);

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