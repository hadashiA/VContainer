using System;
using Unity.Entities;

namespace VContainer.Unity
{
    public sealed class WorldDisposable : IDisposable
    {
        readonly World world;

        public WorldDisposable(World world)
        {
            this.world = world;
        }

        public void Dispose()
        {
            foreach (var system in world.Systems)
            {
                if (system is IDisposable disposableSystem)
                {
                    disposableSystem.Dispose();
                }
            }
            world.Dispose();
        }
    }
}