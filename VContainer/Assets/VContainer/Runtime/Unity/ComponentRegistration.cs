using System;
using System.Collections.Generic;
using UnityEngine;

namespace VContainer.Unity
{
    public sealed class ComponentDestination
    {
        public readonly Component Prefab;
        public readonly string NewGameObjectName;

        readonly Transform parent;
        readonly Func<Transform> parentFinder;

        public ComponentDestination(
            Component prefab,
            Transform parent,
            Func<Transform> parentFinder,
            string newGameObjectName = null)
        {
            Prefab = prefab;
            NewGameObjectName = newGameObjectName;
            this.parent = parent;
            this.parentFinder = parentFinder;
        }

        public Transform GetParent() => parent != null ? parent : parentFinder?.Invoke();
    }

    public sealed class ComponentRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly IInjector injector;
        readonly ComponentDestination destination;

        internal ComponentRegistration(
            Type implementationType,
            IReadOnlyList<Type> interfaceTypes,
            Lifetime lifetime,
            IInjector injector,
            ComponentDestination destination)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;
            this.injector = injector;
            this.destination = destination;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            Component component;
            var parent = destination.GetParent();
            if (destination.Prefab != null)
            {
                component = UnityEngine.Object.Instantiate(destination.Prefab, parent);
            }
            else
            {
                var name = string.IsNullOrEmpty(destination.NewGameObjectName)
                    ? ImplementationType.Name
                    : destination.NewGameObjectName;
                var gameObject = new GameObject(name);
                if (parent != null)
                {
                    gameObject.transform.SetParent(parent);
                }
                component = gameObject.AddComponent(ImplementationType);
            }
            injector.Inject(component, resolver);
            return component;
        }
    }
}