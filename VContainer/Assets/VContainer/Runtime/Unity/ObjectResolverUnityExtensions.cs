using UnityEngine;
using VContainer.Internal;

namespace VContainer.Unity
{
    public static class ObjectResolverUnityExtensions
    {
        public static void InjectGameObject(this IObjectResolver resolver, GameObject gameObject)
        {
            void InjectGameObjectRecursive(GameObject current)
            {
                if (current == null) return;

                using (ListPool<MonoBehaviour>.Get(out var buffer))
                {
                    buffer.Clear();
                    current.GetComponents(buffer);
                    foreach (var monoBehaviour in buffer)
                    {
                        if (monoBehaviour != null)
                        { // Can be null if the MonoBehaviour's type wasn't found (e.g. if it was stripped)
                            resolver.Inject(monoBehaviour);
                        }
                    }
                }

                var transform = current.transform;
                for (var i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    InjectGameObjectRecursive(child.gameObject);
                }
            }

            InjectGameObjectRecursive(gameObject);
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab)
            where T : Component
        {
            return resolver.Instantiate(prefab, prefab.transform.position, prefab.transform.rotation);
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab, Transform parent, bool worldPositionStays = false)
            where T : Component
        {
            var wasActive = prefab.gameObject.activeSelf;
            prefab.gameObject.SetActive(false);

            var instance = UnityEngine.Object.Instantiate(prefab, parent, worldPositionStays);

            SetName(instance, prefab);

            try
            {
                resolver.InjectGameObject(instance.gameObject);
            }
            finally
            {
                prefab.gameObject.SetActive(wasActive);
                instance.gameObject.SetActive(wasActive);
            }

            return instance;
        }

        public static T Instantiate<T>(
            this IObjectResolver resolver,
            T prefab,
            Vector3 position,
            Quaternion rotation)
            where T : Component
        {
            if (resolver.ApplicationOrigin is LifetimeScope scope)
            {
                return scope.Instantiate(prefab, position, rotation);
            }

            return resolver.Instantiate(prefab, position, rotation, null);
        }

        public static T Instantiate<T>(
            this IObjectResolver resolver,
            T prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
            where T : Component
        {
            var wasActive = prefab.gameObject.activeSelf;
            prefab.gameObject.SetActive(false);

            var instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);

            SetName(instance, prefab);

            try
            {
                resolver.InjectGameObject(instance.gameObject);
            }
            finally
            {
                prefab.gameObject.SetActive(wasActive);
                instance.gameObject.SetActive(wasActive);
            }

            return instance;
        }

        static T Instantiate<T>(this LifetimeScope scope, T prefab, Vector3 position, Quaternion rotation)
            where T : Component
        {
            var wasActive = prefab.gameObject.activeSelf;
            prefab.gameObject.SetActive(false);

            T instance;
            if (scope.IsRoot)
            {
                instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
                UnityEngine.Object.DontDestroyOnLoad(instance);
            }
            else
            {
                // Into the same scene as LifetimeScope
                instance = UnityEngine.Object.Instantiate(prefab, position, rotation, scope.transform);
                instance.transform.SetParent(null);
            }

            SetName(instance, prefab);

            try
            {
                scope.Container.InjectGameObject(instance.gameObject);
            }
            finally
            {
                prefab.gameObject.SetActive(wasActive);
                instance.gameObject.SetActive(wasActive);
            }

            return instance;
        }

        static GameObject Instantiate(this LifetimeScope scope, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var wasActive = prefab.activeSelf;
            prefab.SetActive(false);

            GameObject instance;
            if (scope.IsRoot)
            {
                instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
                UnityEngine.Object.DontDestroyOnLoad(instance);
            }
            else
            {
                // Into the same scene as LifetimeScope
                instance = UnityEngine.Object.Instantiate(prefab, position, rotation, scope.transform);
                instance.transform.SetParent(null);
            }

            SetName(instance, prefab);

            try
            {
                scope.Container.InjectGameObject(instance);
            }
            finally
            {
                prefab.SetActive(wasActive);
                instance.SetActive(wasActive);
            }

            return instance;
        }

        public static GameObject Instantiate(this IObjectResolver resolver, GameObject prefab)
        {
            return resolver.Instantiate(prefab, prefab.transform.position, prefab.transform.rotation);
        }

        public static GameObject Instantiate(this IObjectResolver resolver, GameObject prefab, Transform parent, bool worldPositionStays = false)
        {
            var wasActive = prefab.activeSelf;
            prefab.SetActive(false);

            GameObject instance = null;
            try
            {
                instance = UnityEngine.Object.Instantiate(prefab, parent, worldPositionStays);
                SetName(instance, prefab);
                resolver.InjectGameObject(instance);
            }
            finally
            {
                prefab.SetActive(wasActive);
                instance?.SetActive(wasActive);    
            }
            return instance;
        }

        public static GameObject Instantiate(
            this IObjectResolver resolver,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation)
        {
            if (resolver.ApplicationOrigin is LifetimeScope scope)
            {
                return scope.Instantiate(prefab, position, rotation);
            }

            return resolver.Instantiate(prefab, position, rotation, null);
        }

        public static GameObject Instantiate(
            this IObjectResolver resolver,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
        {
            var wasActive = prefab.activeSelf;
            prefab.SetActive(false);

            var instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);

            SetName(instance, prefab);

            try
            {
                resolver.InjectGameObject(instance);
            }
            finally
            {
                prefab.SetActive(wasActive);
                instance.SetActive(wasActive);
            }

            return instance;
        }

        static void SetName(UnityEngine.Object instance, UnityEngine.Object prefab)
        {
            if (VContainerSettings.Instance != null && VContainerSettings.Instance.RemoveClonePostfix)
                instance.name = prefab.name;
        }
    }
}