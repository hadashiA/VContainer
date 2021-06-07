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
        static readonly List<LifetimeScope> WaitingList = new List<LifetimeScope>();

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

        static void EnqueueAwake(LifetimeScope lifetimeScope)
        {
            WaitingList.Add(lifetimeScope);
        }

        static void CancelAwake(LifetimeScope lifetimeScope)
        {
            for (var i = WaitingList.Count - 1; i >= 0; i--)
            {
                if (WaitingList[i] == lifetimeScope)
                {
                    WaitingList.RemoveAt(i);
                }
            }
        }

        static void AwakeWaitingChildren(LifetimeScope awakenParent)
        {
            var type = awakenParent.GetType();

            for (var i = WaitingList.Count - 1; i >= 0; i--)
            {
                var waitingScope = WaitingList[i];
                if (waitingScope.parentReference.Type == type)
                {
                    waitingScope.parentReference.Object = awakenParent;
                    try
                    {
                        waitingScope.Awake();
                    }
                    finally
                    {
                        WaitingList.RemoveAt(i);
                    }
                }
            }
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var list = new List<LifetimeScope>();

            for (var i = WaitingList.Count - 1; i >= 0; i--)
            {
                var waitingScope = WaitingList[i];
                if (waitingScope.gameObject.scene == scene)
                {
                    WaitingList.RemoveAt(i);
                    list.Add(waitingScope);
                }
            }

            foreach (var waitingScope in list)
            {
                waitingScope.Awake(); // Re-throw if parent not found
            }
        }
    }
}