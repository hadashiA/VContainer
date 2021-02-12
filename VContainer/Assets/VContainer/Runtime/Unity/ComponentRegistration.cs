using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VContainer.Unity
{
    sealed class ComponentDestination
    {
        public readonly bool Find;
        public readonly Component Prefab;
        public readonly string NewGameObjectName;
        public Scene Scene;

        readonly Transform parent;
        readonly Func<Transform> parentFinder;

        public ComponentDestination(
            bool find,
            Scene scene,
            Transform parent,
            Func<Transform> parentFinder,
            Component prefab,
            string newGameObjectName = null)
        {
            Find = find;
            Prefab = prefab;
            NewGameObjectName = newGameObjectName;
            Scene = scene;
            this.parent = parent;
            this.parentFinder = parentFinder;
        }

        public Transform GetParent() => parent != null ? parent : parentFinder?.Invoke();
    }

    sealed class ComponentRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public Lifetime Lifetime { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public IReadOnlyList<IInjectParameter> Parameters;

        readonly IInjector injector;
        readonly ComponentDestination destination;

        internal ComponentRegistration(
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            IInjector injector,
            ComponentDestination destination)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
            InterfaceTypes = interfaceTypes;
            Parameters = parameters;
            this.injector = injector;
            this.destination = destination;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            if (destination.Find)
            {
                return FindComponent(resolver);
            }
            if (destination.Prefab != null)
            {
                return InstantiatePrefab(resolver);
            }
            return InstantiateNewGameObject(resolver);
        }

        Component FindComponent(IObjectResolver resolver)
        {
            Component component = null;
            var parent = destination.GetParent();
            if (parent != null)
            {
                component = parent.GetComponentInChildren(ImplementationType);
                if (component == null)
                {
                    throw new VContainerException(ImplementationType, $"Component {ImplementationType} is not in the parent {parent.name}");
                }
            }
            else if (destination.Scene.IsValid())
            {
                var gameObjectBuffer = UnityEngineObjectListBuffer<GameObject>.Get();
                destination.Scene.GetRootGameObjects(gameObjectBuffer);
                foreach (var gameObject in gameObjectBuffer)
                {
                    component = gameObject.GetComponentInChildren(ImplementationType, true);
                    if (component != null) break;
                }
                if (component == null)
                {
                    throw new VContainerException(ImplementationType, $"Component {ImplementationType} is not in this scene {destination.Scene.path}");
                }
            }
            else
            {
                throw new VContainerException(ImplementationType, "Invalid Component find target");
            }

            if (component is MonoBehaviour monoBehaviour)
            {
                injector.Inject(monoBehaviour, resolver, Parameters);
            }
            return component;
        }

        Component InstantiatePrefab(IObjectResolver resolver)
        {
            var parent = destination.GetParent();
            var wasActive = destination.Prefab.gameObject.activeSelf;
            if (wasActive)
            {
                destination.Prefab.gameObject.SetActive(false);
            }

            var component = parent != null
                ? UnityEngine.Object.Instantiate(destination.Prefab, parent)
                : UnityEngine.Object.Instantiate(destination.Prefab);

            injector.Inject(component, resolver, Parameters);

            if (wasActive)
            {
                destination.Prefab.gameObject.SetActive(true);
                component.gameObject.SetActive(true);
            }
            return component;
        }

        Component InstantiateNewGameObject(IObjectResolver resolver)
        {
            var parent = destination.GetParent();
            var name = string.IsNullOrEmpty(destination.NewGameObjectName)
                ? ImplementationType.Name
                : destination.NewGameObjectName;
            var gameObject = new GameObject(name);
            gameObject.SetActive(false);
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            var component = gameObject.AddComponent(ImplementationType);

            injector.Inject(component, resolver, Parameters);
            component.gameObject.SetActive(true);
            return component;
        }
    }
}