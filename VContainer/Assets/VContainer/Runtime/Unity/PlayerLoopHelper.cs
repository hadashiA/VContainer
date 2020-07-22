using System;
using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
#else
using UnityEngine.Experimental.LowLevel;
using UnityEngine.Experimental.PlayerLoop;
#endif

namespace VContainer.Unity
{
    public struct VContainerDelayedStartupFrame {}
    public struct VContainerPostEarlyUpdate {}
    public struct VContainerFixedUpdate {}
    public struct VContainerPostFixedUpdate {}
    public struct VContainerUpdate {}
    public struct VContainerPostUpdate {}
    public struct VContainerLateUpdate {}
    public struct VContainerPostLateUpdate {}

    enum PlayerLoopTiming
    {
        Initialization = 0,
        PostInitialization = 1,

        FixedUpdate = 2,
        PostFixedUpdate = 3,

        Update = 4,
        PostUpdate = 5,

        LateUpdate = 6,
        PostLateUpdate = 7,
    }

    static class PlayerLoopHelper
    {
        const int InitializationSystemIndex = 0;
        const int EarlyUpdateSystemIndex = 1;
        const int FixedUpdateSystemIndex = 2;
        const int PreUpdateSystemIndex = 3;
        const int UpdateSystemIndex = 4;
        const int PreLateUpdateSystemIndex = 5;

        static readonly PlayerLoopRunner[] Runners = new PlayerLoopRunner[8];
        static bool initialized;

        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            lock (Runners)
            {
                if (initialized) return;
                initialized = true;
            }

            for (var i = 0; i < Runners.Length; i++)
            {
                Runners[i] = new PlayerLoopRunner();
            }

            var playerLoop =
#if UNITY_2019_3_OR_NEWER
                PlayerLoop.GetCurrentPlayerLoop();
#else
                PlayerLoop.GetDefaultPlayerLoop();
#endif

            var copyList = playerLoop.subSystemList;

            InsertSubsystem(
                ref copyList[EarlyUpdateSystemIndex],
                typeof(EarlyUpdate.ScriptRunDelayedStartupFrame),
                new PlayerLoopSystem
                {
                    type = typeof(VContainerDelayedStartupFrame),
                    updateDelegate = Runners[(int)PlayerLoopTiming.Initialization].Run
                },
                new PlayerLoopSystem
                {
                    type = typeof(VContainerPostEarlyUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.PostInitialization].Run
                });

            InsertSubsystem(
                ref copyList[FixedUpdateSystemIndex],
                typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate),
                new PlayerLoopSystem
                {
                    type = typeof(VContainerFixedUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.FixedUpdate].Run
                },
                new PlayerLoopSystem
                {
                    type = typeof(VContainerPostFixedUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.PostFixedUpdate].Run
                });

            InsertSubsystem(
                ref copyList[UpdateSystemIndex],
                typeof(Update.ScriptRunBehaviourUpdate),
                new PlayerLoopSystem
                {
                    type = typeof(VContainerUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.Update].Run
                },
                new PlayerLoopSystem
                {
                    type = typeof(VContainerPostUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.PostUpdate].Run
                });

            InsertSubsystem(
                ref copyList[PreLateUpdateSystemIndex],
                typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate),
                new PlayerLoopSystem
                {
                    type = typeof(VContainerLateUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.LateUpdate].Run
                },
                new PlayerLoopSystem
                {
                    type = typeof(VContainerPostLateUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.PostLateUpdate].Run
                });

            playerLoop.subSystemList = copyList;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static void Dispatch(PlayerLoopTiming timing, IPlayerLoopItem item)
        {
            Runners[(int)timing].Dispatch(item);
        }

        static void InsertSubsystem(
            ref PlayerLoopSystem parentSystem,
            Type before,
            PlayerLoopSystem newSystem,
            PlayerLoopSystem newPostSystem)
        {
            var source = parentSystem.subSystemList;
            var insertIndex = -1;
            for (var i = 0; i < source.Length; i++)
            {
                if (source[i].type == before)
                {
                    insertIndex = i;
                }
            }

            if (insertIndex < 0)
            {
                throw new ArgumentException($"{before.FullName} not in system {parentSystem} {parentSystem.type.FullName}");
            }

            var dest = new PlayerLoopSystem[source.Length + 2];
            for (var i = 0; i < dest.Length; i++)
            {
                if (i == insertIndex)
                {
                    dest[i] = newSystem;
                }
                else if (i == dest.Length - 1)
                {
                    dest[i] = newPostSystem;
                }
                else if (i < insertIndex)
                {
                    dest[i] = source[i];
                }
                else
                {
                    dest[i] = source[i - 1];
                }
            }

            parentSystem.subSystemList = dest;
        }
    }
}