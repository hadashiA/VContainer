using System.Collections.Generic;
using UnityEngine;
using VContainer.Internal;

namespace VContainer.Unity
{
    sealed class PrefabComponentProvider : IInstanceProvider
    {
        readonly IReadOnlyList<IInjectParameter> customParameters;
        readonly Component prefab;
        ComponentDestination destination;

        public PrefabComponentProvider(
            Component prefab,
            IReadOnlyList<IInjectParameter> customParameters,
            in ComponentDestination destination)
        {
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
                ? Object.Instantiate(prefab, parent)
                : Object.Instantiate(prefab);

            if (VContainerSettings.Instance != null && VContainerSettings.Instance.RemoveClonePostfix)
                component.name = prefab.name;

            try
            {
                resolver.InjectGameObject(component.gameObject, customParameters);
                destination.ApplyDontDestroyOnLoadIfNeeded(component);
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