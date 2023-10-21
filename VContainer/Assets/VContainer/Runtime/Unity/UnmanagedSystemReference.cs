#if VCONTAINER_ECS_INTEGRATION && UNITY_2022_2_OR_NEWER
using System;
using Unity.Entities;

namespace VContainer.Unity
{
    public class UnmanagedSystemReference : IDisposable
    {
        private World world;
        
        public SystemHandle Id { get; private set; }

        public Type SystemType { get; private set; }
        
        public UnmanagedSystemReference(Type systemType, SystemHandle id, World world)
        {
            this.world = world;
            SystemType = systemType;
            Id = id;
        }
        
        public ref T GetSystem<T>() where T : unmanaged, ISystem
        {
            if (typeof(T) != SystemType)
            {
                throw new ArgumentException($"System type mismatch. Expected: {SystemType}, Actual: {typeof(T)}");
            }

            if (!world.Unmanaged.IsSystemValid(Id))
            {
                throw new InvalidOperationException($"System is not valid. SystemType: {SystemType}");
            }

            return ref world.Unmanaged.GetUnsafeSystemRef<T>(Id);
        }
        
        public ref T GetSystemFromDefaultWorld<T>() where T : unmanaged, ISystem
        {
            return ref World.DefaultGameObjectInjectionWorld.Unmanaged.GetUnsafeSystemRef<T>(Id);
        }
        
        public void Dispose()
        {
            if (world.Unmanaged.IsSystemValid(Id))
            {
                world.DestroySystem(Id);
            }
            else if(world is null && World.DefaultGameObjectInjectionWorld.Unmanaged.IsSystemValid(Id))
            {
                World.DefaultGameObjectInjectionWorld.DestroySystem(Id);
            }
        }
    }
    
    public class UnmanagedSystemReference<T> : UnmanagedSystemReference where T : unmanaged, ISystem
    {
        public UnmanagedSystemReference(SystemHandle id, World world) : base(typeof(T), id, world) { }

        public ref T GetSystem(WorldUnmanaged world)
        {
            return ref world.GetUnsafeSystemRef<T>(Id);
        }

        public ref T GetSystemFromDefaultWorld()
        {
            return ref World.DefaultGameObjectInjectionWorld.Unmanaged.GetUnsafeSystemRef<T>(Id);
        }
    }
}
#endif
