using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Internal;

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
            WaitingList.Remove(lifetimeScope);
        }

        static void AwakeWaitingChildren(LifetimeScope awakenParent)
        {
            if (WaitingList.Count <= 0) return;

            using (ListPool<LifetimeScope>.Get(out var buffer))
            {
                for (var i = WaitingList.Count - 1; i >= 0; i--)
                {
                    var waitingScope = WaitingList[i];
                    if (waitingScope.parentReference.Type == awakenParent.GetType())
                    {
                        waitingScope.parentReference.Object = awakenParent;
                        WaitingList.RemoveAt(i);
                        buffer.Add(waitingScope);
                    }
                }

                foreach (var waitingScope in buffer)
                {
                    waitingScope.Awake();
                }
            }
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (WaitingList.Count <= 0)
                return;

            using (ListPool<LifetimeScope>.Get(out var buffer))
            {
                for (var i = WaitingList.Count - 1; i >= 0; i--)
                {
                    var waitingScope = WaitingList[i];
                    if (waitingScope.gameObject.scene == scene)
                    {
                        WaitingList.RemoveAt(i);
                        buffer.Add(waitingScope);
                    }
                }

                foreach (var waitingScope in buffer)
                {
                    waitingScope.Awake(); // Re-throw if parent not found
                }
            }
        }
    }
}