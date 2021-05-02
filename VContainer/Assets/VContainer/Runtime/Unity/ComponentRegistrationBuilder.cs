using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Internal;

namespace VContainer.Unity
{
    struct ComponentDestination
    {
        public Transform Parent;
        public Func<Transform> ParentFinder;

        public Transform GetParent()
        {
            if (Parent != null)
                return Parent;
            if (ParentFinder != null)
                return ParentFinder();
            return null;
        }
    }

    public sealed class ComponentRegistrationBuilder : RegistrationBuilder
    {
        readonly bool finding;
        readonly Scene scene;
        readonly Component prefab;
        readonly string gameObjectName;
        
        ComponentDestination destination;

        internal ComponentRegistrationBuilder(
            Component prefab,
            Type implementationType,
            Lifetime lifetime)
            : this(false, default, implementationType, lifetime)
        {
            this.prefab = prefab;
        }

        internal ComponentRegistrationBuilder(
            string gameObjectName,
            Type implementationType,
            Lifetime lifetime)
            : this(false, default, implementationType, lifetime)
        {
            this.gameObjectName = gameObjectName;
        }

        internal ComponentRegistrationBuilder(Scene scene, Type implementationType)
            : this(true, scene, implementationType, Lifetime.Scoped)
        {
        }

        ComponentRegistrationBuilder(
            bool finding,
            Scene scene,
            Type implementationType,
            Lifetime lifetime)
            : base(implementationType, lifetime)
        {
            this.finding = finding;
            this.scene = scene;
        }

        public override IRegistration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);

            if (finding)
            {
                return new FindComponentRegistration(
                    ImplementationType,
                    InterfaceTypes,
                    Parameters,
                    injector,
                    scene,
                    destination);
            }
            if (prefab != null)
            {
                return new PrefabComponentRegistration(
                    prefab,
                    ImplementationType,
                    Lifetime,
                    InterfaceTypes,
                    Parameters,
                    injector,
                    destination);
            }

            return new NewGameObjectComponentRegistration(
                ImplementationType,
                Lifetime,
                InterfaceTypes,
                Parameters,
                injector,
                destination,
                gameObjectName);
        }

        public ComponentRegistrationBuilder UnderTransform(Transform parent)
        {
            destination.Parent = parent;
            return this;
        }

        public ComponentRegistrationBuilder UnderTransform(Func<Transform> parentFinder)
        {
            destination.ParentFinder = parentFinder;
            return this;
        }
   }
}