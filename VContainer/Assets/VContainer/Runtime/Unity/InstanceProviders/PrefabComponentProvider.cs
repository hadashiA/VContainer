using System.Collections.Generic;
using UnityEngine;

namespace VContainer.Unity
{
    sealed class PrefabComponentProvider : IInstanceProvider
    {
        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> customParameters;
        readonly Component prefab;
        ComponentDestination destination;

        public PrefabComponentProvider(
            Component prefab,
            IInjector injector,
            IReadOnlyList<IInjectParameter> customParameters,
            in ComponentDestination destination)
        {
            this.injector = injector;
            this.customParameters = customParameters;
            this.prefab = prefab;
            this.destination = destination;
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

            try
            {
                injector.Inject(component, resolver, customParameters);
            }
            finally
            {
                if (wasActive)
                {
                    prefab.gameObject.SetActive(true);
                    component.gameObject.SetActive(true);
                }
            }

            return component;
        }
    }
}