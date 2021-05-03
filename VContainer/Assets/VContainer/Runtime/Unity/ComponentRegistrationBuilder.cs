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
        readonly object instance;
        readonly Component prefab;
        readonly string gameObjectName;

        ComponentDestination destination;
        Scene scene;

        internal ComponentRegistrationBuilder(object instance)
            : base(instance.GetType(), Lifetime.Singleton)
        {
            this.instance = instance;
        }

        internal ComponentRegistrationBuilder(in Scene scene, Type implementationType)
            : base(implementationType, Lifetime.Scoped)
        {
            this.scene = scene;
        }

        internal ComponentRegistrationBuilder(
            Component prefab,
            Type implementationType,
            Lifetime lifetime)
            : base(implementationType, lifetime)
        {
            this.prefab = prefab;
        }

        internal ComponentRegistrationBuilder(
            string gameObjectName,
            Type implementationType,
            Lifetime lifetime)
            : base(implementationType, lifetime)
        {
            this.gameObjectName = gameObjectName;
        }

        public override IRegistration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);

            if (instance != null)
            {
                return new InstanceComponentRegistration(
                    instance,
                    ImplementationType,
                    InterfaceTypes,
                    Parameters,
                    injector);
            }
            if (scene.IsValid())
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