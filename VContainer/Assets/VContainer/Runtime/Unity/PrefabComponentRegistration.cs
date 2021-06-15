using System;
using System.Collections.Generic;
using UnityEngine;

namespace VContainer.Unity
{
    sealed class PrefabComponentRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly IReadOnlyList<IInjectParameter> parameters;
        readonly IInjector injector;
        readonly Component prefab;
        ComponentDestination destination;

        public PrefabComponentRegistration(
            Component prefab,
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            IInjector injector,
            in ComponentDestination destination)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;

            this.parameters = parameters;
            this.injector = injector;
            this.prefab = prefab;
            this.destination = destination;
        }
        
        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            return $"PrefabComponentRegistration {ImplementationType} Prefab={prefab} ContractTypes=[{contractTypes}] {Lifetime} {injector.GetType().Name})";
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var wasActive = prefab.gameObject.activeSelf;
            if (wasActive)
            {
                prefab.gameObject.SetActive(false);
            }

            var parent = destination.GetParent();
            var component = parent != null
                ? UnityEngine.Object.Instantiate(prefab, parent)
                : UnityEngine.Object.Instantiate(prefab);

            injector.Inject(component, resolver, parameters);

            if (wasActive)
            {
                prefab.gameObject.SetActive(true);
                component.gameObject.SetActive(true);
            }
            return component;
        }
    }
}