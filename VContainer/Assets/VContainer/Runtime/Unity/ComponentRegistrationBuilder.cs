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
        public bool DontDestroyOnLoad;

        public Transform GetParent()
        {
            if (Parent != null)
                return Parent;
            if (ParentFinder != null)
                return ParentFinder();
            return null;
        }

        public void ApplyDontDestroyOnLoadIfNeeded(Component component)
        {
            if (DontDestroyOnLoad)
            {
                UnityEngine.Object.DontDestroyOnLoad(component);
            }
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
            IInstanceProvider provider;

            if (instance != null)
            {
                var injector = InjectorCache.GetOrBuild(ImplementationType);
                provider = new ExistingComponentProvider(instance, injector, Parameters, destination.DontDestroyOnLoad);
            }
            else if (scene.IsValid())
            {
                provider = new FindComponentProvider(ImplementationType, Parameters, in scene, in destination);
            }
            else if (prefab != null)
            {
                var injector = InjectorCache.GetOrBuild(prefab.GetType());
                provider = new PrefabComponentProvider(prefab, injector, Parameters, in destination);
            }
            else
            {
                var injector = InjectorCache.GetOrBuild(ImplementationType);
                provider = new NewGameObjectProvider(ImplementationType, injector, Parameters, in destination, gameObjectName);
            }
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, provider);
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

        public ComponentRegistrationBuilder DontDestroyOnLoad()
        {
            destination.DontDestroyOnLoad = true;
            return this;
        }
   }
}