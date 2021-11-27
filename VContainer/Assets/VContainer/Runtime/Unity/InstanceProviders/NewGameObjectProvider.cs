using System;
using System.Collections.Generic;
using UnityEngine;

namespace VContainer.Unity
{
    sealed class NewGameObjectProvider : IInstanceProvider
    {
        readonly Type componentType;
        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> customParameters;
        readonly string newGameObjectName;
        ComponentDestination destination;

        public NewGameObjectProvider(
            Type componentType,
            IInjector injector,
            IReadOnlyList<IInjectParameter> customParameters,
            in ComponentDestination destination,
            string newGameObjectName = null)
        {
            this.componentType = componentType;
            this.customParameters = customParameters;
            this.injector = injector;
            this.destination = destination;
            this.newGameObjectName = newGameObjectName;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var name = string.IsNullOrEmpty(newGameObjectName)
                ? componentType.Name
                : newGameObjectName;
            var gameObject = new GameObject(name);
            gameObject.SetActive(false);

            var parent = destination.GetParent();
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            var component = gameObject.AddComponent(componentType);

            injector.Inject(component, resolver, customParameters);
            component.gameObject.SetActive(true);
            return component;
        }
    }
}