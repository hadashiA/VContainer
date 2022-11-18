using UnityEngine;

namespace VContainer.Unity
{
    public static class ObjectResolverUnityExtensions
    {
        public static void InjectGameObject(this IObjectResolver resolver, GameObject gameObject)
        {
            void InjectGameObjectRecursive(GameObject current)
            {
                if (current == null) return;

                using (UnityEngineObjectListBuffer<MonoBehaviour>.Get(out var buffer))
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

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab) where T : UnityEngine.Object
        {
            T instance;
            if (resolver.ApplicationOrigin is LifetimeScope scope)
            {
                if (scope.IsRoot)
                {
                    instance = UnityEngine.Object.Instantiate(prefab);
                    UnityEngine.Object.DontDestroyOnLoad(instance);
                }
                else
                {
                    // Into the same scene as LifetimeScope
                    instance = UnityEngine.Object.Instantiate(prefab, scope.transform);
                    ResetParent(instance);
                }
            }
            else
            {
                instance = UnityEngine.Object.Instantiate(prefab);
            }
            SetName(instance, prefab);

            InjectUnityEngineObject(resolver, instance);
            return instance;
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab, Transform parent, bool worldPositionStays = false)
            where T : UnityEngine.Object
        {
            var instance = UnityEngine.Object.Instantiate(prefab, parent, worldPositionStays);
            SetName(instance, prefab);

            InjectUnityEngineObject(resolver, instance);
            return instance;
        }

        public static T Instantiate<T>(
            this IObjectResolver resolver,
            T prefab,
            Vector3 position,
            Quaternion rotation)
            where T : UnityEngine.Object
        {
            T instance;
            if (resolver.ApplicationOrigin is LifetimeScope scope)
            {
                if (scope.IsRoot)
                {
                    instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
                    UnityEngine.Object.DontDestroyOnLoad(instance);
                }
                else
                {
                    // Into the same scene as LifetimeScope
                    instance = UnityEngine.Object.Instantiate(prefab, position, rotation, scope.transform);
                    ResetParent(instance);
                }
            }
            else
            {
                instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
            }

            SetName(instance, prefab);
            InjectUnityEngineObject(resolver, instance);
            return instance;
        }

        public static T Instantiate<T>(
            this IObjectResolver resolver,
            T prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
            where T : UnityEngine.Object
        {
            var instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            SetName(instance, prefab);

            InjectUnityEngineObject(resolver, instance);
            return instance;
        }

        static void InjectUnityEngineObject<T>(IObjectResolver resolver, T instance) where T : UnityEngine.Object
        {
            if (instance is GameObject gameObject)
                resolver.InjectGameObject(gameObject);
            else
                resolver.Inject(instance);
        }

        static void ResetParent<T>(T instance) where T : UnityEngine.Object
        {
            switch (instance)
            {
                case Component component:
                    component.transform.parent = null;
                    break;
                case GameObject gameObject:
                    gameObject.transform.parent = null;
                    break;
            }
        }

        static void SetName<T>(T instance, T prefab) where T : UnityEngine.Object
        {
            if (VContainerSettings.Instance != null && VContainerSettings.Instance.RemoveClonePostfix)
                instance.name = prefab.name;
        }
    }
}
