using System;
using System.Collections.Generic;
using UnityEngine;

namespace VContainer.Unity
{
    sealed class NewGameObjectComponentRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> parameters;
        readonly string newGameObjectName;
        ComponentDestination destination;

        public NewGameObjectComponentRegistration(
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            IInjector injector,
            in ComponentDestination destination,
            string newGameObjectName = null)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
            InterfaceTypes = interfaceTypes;

            this.parameters = parameters;
            this.injector = injector;
            this.destination = destination;
            this.newGameObjectName = newGameObjectName;
        }
        
        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            return $"NewGameObjectComponentRegistration {ImplementationType} ContractTypes=[{contractTypes}] {Lifetime} {injector.GetType().Name})";
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var name = string.IsNullOrEmpty(newGameObjectName)
                ? ImplementationType.Name
                : newGameObjectName;
            var gameObject = new GameObject(name);
            gameObject.SetActive(false);

            var parent = destination.GetParent();
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            var component = gameObject.AddComponent(ImplementationType);

            injector.Inject(component, resolver, parameters);
            component.gameObject.SetActive(true);
            return component;
        }
    }
}