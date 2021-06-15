using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VContainer.Unity
{
    sealed class FindComponentRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime => Lifetime.Scoped;

        readonly IReadOnlyList<IInjectParameter> parameters;
        readonly IInjector injector;
        ComponentDestination destination;
        Scene scene;

        public FindComponentRegistration(
            Type implementationType,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            IInjector injector,
            in Scene scene,
            in ComponentDestination destination)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;

            this.parameters = parameters;
            this.injector = injector;
            this.scene = scene;
            this.destination = destination;
        }

        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            return $"FindComponentRegistration {ImplementationType} ContractTypes=[{contractTypes}] {Lifetime} {injector.GetType().Name})";
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var component = default(Component);

            var parent = destination.GetParent();
            if (parent != null)
            {
                component = parent.GetComponentInChildren(ImplementationType, true);
                if (component == null)
                {
                    throw new VContainerException(ImplementationType, $"{ImplementationType} is not in the parent {parent.name} : {this}");
                }
            }
            else if (scene.IsValid())
            {
                var gameObjectBuffer = UnityEngineObjectListBuffer<GameObject>.Get();
                scene.GetRootGameObjects(gameObjectBuffer);
                foreach (var gameObject in gameObjectBuffer)
                {
                    component = gameObject.GetComponentInChildren(ImplementationType, true);
                    if (component != null) break;
                }
                if (component == null)
                {
                    throw new VContainerException(ImplementationType, $"{ImplementationType} is not in this scene {scene.path} : {this}");
                }
            }
            else
            {
                throw new VContainerException(ImplementationType, $"Invalid Component find target {this}");
            }

            if (component is MonoBehaviour monoBehaviour)
            {
                injector.Inject(monoBehaviour, resolver, parameters);
            }
            return component;
        }
    }
}