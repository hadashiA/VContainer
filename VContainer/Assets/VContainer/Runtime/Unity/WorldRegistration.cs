using System;
using System.Collections.Generic;
using Unity.Entities;

#if VCONTAINER_ECS_INTEGRATION
namespace VContainer.Unity
{
    public sealed class WorldRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime { get; }

        readonly string name;
        readonly Action<World> configuration;

        public WorldRegistration(string name, Lifetime lifetime, Action<World> configuration = null)
        {
            ImplementationType = typeof(World);
            Lifetime = lifetime;

            this.name = name;
            this.configuration = configuration;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var world = new World(name);
            if (configuration != null)
            {
                configuration(world);
            }
            else
            {
                ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
            }
            return world;
        }
    }
}
#endif