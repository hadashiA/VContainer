using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VContainer.Unity
{
    sealed class FindComponentProvider : IInstanceProvider
    {
        readonly Type componentType;
        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> customParameters;
        ComponentDestination destination;
        Scene scene;

        public FindComponentProvider(
            Type componentType,
            IInjector injector,
            IReadOnlyList<IInjectParameter> customParameters,
            in Scene scene,
            in ComponentDestination destination)
        {
            this.componentType = componentType;
            this.injector = injector;
            this.customParameters = customParameters;
            this.scene = scene;
            this.destination = destination;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var component = default(Component);

            var parent = destination.GetParent();
            if (parent != null)
            {
                component = parent.GetComponentInChildren(componentType, true);
                if (component == null)
                {
                    throw new VContainerException(componentType, $"{componentType} is not in the parent {parent.name} : {this}");
                }
            }
            else if (scene.IsValid())
            {
                var gameObjectBuffer = UnityEngineObjectListBuffer<GameObject>.Get();
                scene.GetRootGameObjects(gameObjectBuffer);
                foreach (var gameObject in gameObjectBuffer)
                {
                    component = gameObject.GetComponentInChildren(componentType, true);
                    if (component != null) break;
                }
                if (component == null)
                {
                    throw new VContainerException(componentType, $"{componentType} is not in this scene {scene.path} : {this}");
                }
            }
            else
            {
                throw new VContainerException(componentType, $"Invalid Component find target {this}");
            }

            if (component is MonoBehaviour monoBehaviour)
            {
                injector.Inject(monoBehaviour, resolver, customParameters);
            }
            return component;
        }
    }
}