#if VCONTAINER_ECS_INTEGRATION
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace VContainer.Unity
{
    public sealed class WorldConfigurationHelper : IDisposable
    {
        public readonly World World;

        public WorldConfigurationHelper(IEnumerable<World> worlds, string targetWorldName)
        {
            foreach (var world in worlds)
            {
                if (world.Name == targetWorldName)
                {
                    World = world;
                    break;
                }
            }

            if (World is null)
                throw new VContainerException(typeof(WorldConfigurationHelper), $"World {targetWorldName} is not registered");
        }

        public void SortSystems()
        {
            foreach (var system in World.Systems)
            {
                if (system is ComponentSystemGroup group)
                    group.SortSystems();
            }
        }

        public void Dispose()
        {
            foreach (var system in World.Systems)
            {
                if (system is IDisposable disposableSystem)
                {
                    disposableSystem.Dispose();
                }
            }
        }
    }
}
#endif