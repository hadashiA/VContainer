using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VContainer.Unity
{
    public sealed class VContainerParentTypeReferenceNotFound : Exception
    {
        public readonly Type ParentType;

        public VContainerParentTypeReferenceNotFound(Type parentType, string message)
            : base(message)
        {
            ParentType = parentType;
        }
    }


    partial class LifetimeScope
    {
        static readonly List<(LifetimeScope, VContainerParentTypeReferenceNotFound)> WaitingList =
            new List<(LifetimeScope, VContainerParentTypeReferenceNotFound)>();

#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static void SubscribeSceneEvents()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void EnqueueAwake(LifetimeScope lifetimeScope, VContainerParentTypeReferenceNotFound ex)
        {
            WaitingList.Add((lifetimeScope, ex));
        }

        static void CancelAwake(LifetimeScope lifetimeScope)
        {
            for (var i = WaitingList.Count - 1; i >= 0; i--)
            {
                if (WaitingList[i].Item1 == lifetimeScope)
                {
                    WaitingList.RemoveAt(i);
                }
            }
        }

        static void AwakeWaitingChildren(LifetimeScope awakenParent)
        {
            if (WaitingList.Count < 0) return;

            var type = awakenParent.GetType();

            for (var i = WaitingList.Count - 1; i >= 0; i--)
            {
                var (waiting, ex) = WaitingList[i];
                if (ex.ParentType == type)
                {
                    waiting.parentReference.Object = awakenParent;
                    WaitingList.RemoveAt(i);
                    waiting.Awake();
                }
            }
        }


        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var list = new List<(LifetimeScope, VContainerParentTypeReferenceNotFound)>();

            for (var i = WaitingList.Count - 1; i >= 0; i--)
            {
                var (waitingScope, ex) = WaitingList[i];
                if (waitingScope.gameObject.scene == scene)
                {
                    WaitingList.RemoveAt(i);
                    list.Add((waitingScope, ex));
                }
            }

            foreach (var entry in list)
            {
                var (waitingScope, _) = entry;
                waitingScope.Awake(); // Re-throw if parent not found
            }
        }
    }
}