using System;
using UnityEngine;

namespace VContainer.Unity.Extensions
{
    public abstract class NestedSingletonLifetimeScope<T> : SingletonLifetimeScopeBase where T : SingletonLifetimeScope<T>
    {
        protected void OnValidate()
        {
            if (parentReference.Type != typeof(T))
            {
                Reset();
            }
        }

        protected virtual void Reset()
        {
            parentReference = ParentReference.Create<T>();
        }

        protected override LifetimeScope FindParent()
        {
            var objs = FindObjects(parentReference.Type);
            if (objs.Length > 1) 
            { 
                foreach (var obj in objs) 
                {
                    var scope = (LifetimeScope) obj;
                    if (scope.gameObject.scene.name == "DontDestroyOnLoad"
                        && scope.Container != null)
                    {
                        return scope;
                    } 
                }
            }
            {
                if (objs.Length > 0 
                    && objs[0] is LifetimeScope scope 
                    && scope.Container != null)
                {
                    return scope;
                }
            }
            return null;
        }
        
        static UnityEngine.Object[] FindObjects(Type type)
        {
#if UNITY_2022_1_OR_NEWER
            return FindObjectsByType(type, FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            return FindObjectsOfType(type);
#endif
        }
    }
}