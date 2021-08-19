using UnityEngine;
using VContainer.Internal;

namespace VContainer.Unity
{
    /// <summary>
    /// Extension methods for <see cref="IObjectResolver"/> to simplify its use
    /// with Unity <see cref="UnityEngine.Object"/>s (especially prefabs).
    /// </summary>
    /// <seealso cref="IObjectResolverExtensions"/>
    /// <seealso href="https://vcontainer.hadashikick.jp/resolving/gameobject-injection"/>
    public static class ObjectResolverUnityExtensions
    {
        /// <summary>
        /// Injects dependencies into the <see cref="MonoBehaviour"/>s of the
        /// given <see cref="GameObject"/> and its descendents.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will inject dependencies into <i>all</i> <see cref="MonoBehaviour"/>s
        /// of <paramref name="gameObject"/> and its descendents, regardless of whether
        /// the <see cref="GameObject"/>s or <see cref="MonoBehaviour"/>s are enabled.
        /// </para>
        /// <para>
        /// As <see cref="MonoBehaviour"/>s do not support constructors, use
        /// <see cref="InjectAttribute"/> on methods, properties, or fields instead.
        /// </para>
        /// </remarks>
        /// <param name="resolver">
        /// The dependency container that will be used to inject <paramref name="gameObject"/>'s
        /// <see cref="MonoBehaviour"/>s.
        /// </param>
        /// <param name="gameObject">
        /// The <see cref="GameObject"/> whose <see cref="MonoBehaviour"/>s will
        /// receive dependencies.
        /// </param>
        /// <exception cref="VContainerException">
        /// A dependency couldn't be resolved.
        /// </exception>
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

        /// <summary>
        /// Instantiates a prefab and injects its <see cref="MonoBehaviour"/>s with
        /// their dependencies.
        /// </summary>
        /// <remarks>
        /// This method can only be used to instantiate and inject prefabs synchronously.
        /// If you need to do so asynchronously (e.g. with <a href="https://docs.unity3d.com/Packages/com.unity.addressables@latest">Addressables</a>),
        /// spawn the prefab first then use <see cref="InjectGameObject"/> when it's ready. 
        /// </remarks>
        /// <param name="resolver">
        /// The dependency container that will be used to inject the spawned object's
        /// <see cref="MonoBehaviour"/>s.
        /// </param>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>The spawned prefab.</returns>
        /// <exception cref="VContainerException">A dependency couldn't be resolved.</exception>
        /// <seealso cref="InjectGameObject"/>
        /// <seealso cref="Object.Instantiate{T}(T)"/>
        public static T Instantiate<T>(this IObjectResolver resolver, T prefab) where T : UnityEngine.Object
        {
            var instance = UnityEngine.Object.Instantiate(prefab);
            InjectUnityEngineObject(resolver, instance);
            return instance;
        }

        /// <inheritdoc cref="Instantiate{T}(VContainer.IObjectResolver,T)"/>
        /// <param name="parent">
        /// The parent of the game object. Overloads without this parameter will spawn
        /// the prefab at the root of the active scene's hierarchy.
        /// </param>
        /// <param name="worldPositionStays">
        /// Defaults to <see langword="false"/>.
        /// </param>
        /// <seealso cref="Object.Instantiate{T}(T,Transform,bool)"/>
        public static T Instantiate<T>(this IObjectResolver resolver, T prefab, Transform parent, bool worldPositionStays = false)
            where T : UnityEngine.Object
        {
            var instance = UnityEngine.Object.Instantiate(prefab, parent, worldPositionStays);
            InjectUnityEngineObject(resolver, instance);
            return instance;
        }

        /// <inheritdoc cref="Instantiate{T}(VContainer.IObjectResolver,T)"/>
        /// <param name="position">
        /// The position of the spawned object, in local space. Overloads without
        /// this parameter will use the prefab's position.
        /// </param>
        /// <param name="rotation">
        /// Overloads without this parameter will use the prefab's rotation.
        /// </param>
        /// <seealso cref="Object.Instantiate{T}(T,Vector3,Quaternion)"/>
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

        /// <inheritdoc cref="Instantiate{T}(VContainer.IObjectResolver,T,Vector3,Quaternion)"/>
        /// <param name="parent"></param>
        /// <seealso cref="Object.Instantiate{T}(T,Vector3,Quaternion,Transform)"/>
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
