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

        public override Registration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);
            IInstanceSpawner spawner;

            if (instance != null)
            {
                spawner = new ExistingComponentSpawner(instance, injector, Parameters);
            }
            else if (scene.IsValid())
            {
                spawner = new FindComponentSpawner(ImplementationType, injector, Parameters, in scene, in destination);
            }
            else if (prefab != null)
            {
                spawner = new PrefabComponentSpawner(prefab, injector, Parameters, in destination);
            }
            else
            {
                spawner = new NewGameObjectSpawner(ImplementationType, injector, Parameters, in destination, gameObjectName);
            }
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, spawner);
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