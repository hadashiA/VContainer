using UnityEngine;
using VContainer.Internal;

namespace VContainer.Unity
{
    public static class ObjectResolverUnityExtensions
    {
        public static void InjectGameObject(this IObjectResolver resolver, GameObject gameObject)
        {
            var buffer = UnityEngineObjectListBuffer<MonoBehaviour>.Get();

            void InjectGameObjectRecursive(GameObject current)
            {
                if (current == null) return;

                buffer.Clear();
                current.GetComponents(buffer);
                foreach (var monoBehaviour in buffer)
                {
                    resolver.Inject(monoBehaviour);
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
            var instance = UnityEngine.Object.Instantiate(prefab);
            InjectUnityEngineObject(resolver, instance);
            return instance;
        }

        public static T Instantiate<T>(this IObjectResolver resolver, T prefab, Transform parent, bool worldPositionStays = false)
            where T : UnityEngine.Object
        {
            var instance = UnityEngine.Object.Instantiate(prefab, parent, worldPositionStays);
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
            var instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
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
    }
}
