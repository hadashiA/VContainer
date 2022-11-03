#if VCONTAINER_ECS_INTEGRATION
using System;
using Unity.Entities;
using UnityEngine.LowLevel;

namespace VContainer.Unity
{
    public sealed class WorldInstanceProvider : IInstanceProvider
    {
        readonly string name;
        readonly Action<World> initialization;

        public WorldInstanceProvider(string name, Action<World> initialization = null)
        {
            this.name = name;
            this.initialization = initialization;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var world = new World(name);
            if (initialization != null)
            {
                initialization(world);
            }
            else
            {
#if VCONTAINER_ECS_INTEGRATION_1_0
                world.CreateSystemManaged<InitializationSystemGroup>();
                world.CreateSystemManaged<SimulationSystemGroup>();
                world.CreateSystemManaged<PresentationSystemGroup>();

                ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(world);
                ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
#else
                world.CreateSystem<InitializationSystemGroup>();
                world.CreateSystem<SimulationSystemGroup>();
                world.CreateSystem<PresentationSystemGroup>();

                ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(world);
                ScriptBehaviourUpdateOrder.AddWorldToCurrentPlayerLoop(world);
#endif
            }
            return world;
        }
    }
}
#endif
