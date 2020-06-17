using System;
using System.Collections.Generic;
using UnityEngine;

namespace VContainer.Unity
{
    // Decorate Unity features
    public sealed class ContainerBuilderUnity : IContainerBuilder
    {
        readonly IContainerBuilder builder;
        readonly LifetimeScope lifetimeScope;

        GameObject[] RootGameObjects
        {
            get
            {
                if (rootGameObjects == null)
                    rootGameObjects = lifetimeScope.gameObject.scene.GetRootGameObjects();
                return rootGameObjects;
            }
        }

        GameObject[] rootGameObjects;

        public ContainerBuilderUnity(
            IContainerBuilder builder,
            LifetimeScope lifetimeScope)
        {
            this.builder = builder;
            this.lifetimeScope = lifetimeScope;
        }

        public RegistrationBuilder Register<T>(Lifetime lifetime) => builder.Register<T>(lifetime);
        public RegistrationBuilder RegisterInstance<T>(T instance) => builder.RegisterInstance(instance);
        public IObjectResolver Build() => builder.Build();

        public RegistrationBuilder RegisterComponentInHierarchy<T>()
        {
            var component = default(T);
            foreach (var x in RootGameObjects)
            {
                component = x.GetComponent<T>();
                if (component != null) break;
            }

            if (component == null)
            {
                throw new VContainerException($"Component {typeof(T)} is not in this scene {lifetimeScope.gameObject.scene}");
            }

            return RegisterInstance(component);
        }

        public RegistrationBuilder RegisterPrefab(Component component, Transform parent = null)
        {
            throw new NotImplementedException();
        }
    }
}