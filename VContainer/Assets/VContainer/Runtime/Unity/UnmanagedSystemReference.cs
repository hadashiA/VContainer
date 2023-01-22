#if VCONTAINER_ECS_INTEGRATION_1_0
using System;
using Unity.Entities;
using UnityEngine;

namespace VContainer.Unity
{
    public class UnmanagedSystemReference
    {
        public SystemHandle Id { get; private set; }

        public Type SystemType { get; private set; }
        
        public UnmanagedSystemReference(Type systemType, SystemHandle id)
        {
            SystemType = systemType;
            Id = id;
        }
        
        public ref T GetSystem<T>(ref WorldUnmanaged world) where T : unmanaged, ISystem
        {
            if (typeof(T) != SystemType)
            {
                throw new ArgumentException($"System type mismatch. Expected: {SystemType}, Actual: {typeof(T)}");
            }

            return ref world.GetUnsafeSystemRef<T>(Id);
        }
    }
    
    public class UnmanagedSystemReference<T> : UnmanagedSystemReference where T : unmanaged, ISystem
    {
        public UnmanagedSystemReference(SystemHandle id) : base(typeof(T), id) { }

        public ref T GetSystem(WorldUnmanaged world)
        {
            return ref world.GetUnsafeSystemRef<T>(Id);
        }

        public ref T GetSystemFromDefaultWorld()
        {
            var world = World.DefaultGameObjectInjectionWorld.Unmanaged;
            return ref world.GetUnsafeSystemRef<T>(Id);
        }
    }
}
#endif
