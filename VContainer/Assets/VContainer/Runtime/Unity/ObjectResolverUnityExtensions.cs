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

                var monoBehaviours = current.GetComponents<MonoBehaviour>();
                foreach (var x in monoBehaviours)
                {
                    resolver.Inject(x);
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
    }
}
