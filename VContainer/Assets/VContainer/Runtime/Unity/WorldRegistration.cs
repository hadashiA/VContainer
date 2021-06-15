#if VCONTAINER_ECS_INTEGRATION
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine.LowLevel;

namespace VContainer.Unity
{
    public sealed class WorldRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly string name;
        readonly Action<World> initialization;

        public WorldRegistration(string name, Lifetime lifetime, Action<World> initialization = null)
        {
            ImplementationType = typeof(World);
            Lifetime = lifetime;

            this.name = name;
            this.initialization = initialization;
        }

        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            return $"WorldRegistration {ImplementationType.Name} ContractTypes=[{contractTypes}] {Lifetime}";
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
                world.CreateSystem<InitializationSystemGroup>();
                world.CreateSystem<SimulationSystemGroup>();
                world.CreateSystem<PresentationSystemGroup>();
                ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world, PlayerLoop.GetCurrentPlayerLoop());
            }
            return world;
        }
    }
}
#endif