#if VCONTAINER_ECS_INTEGRATION && UNITY_2022_2_OR_NEWER
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace VContainer.Unity
{
    public sealed class UnmanagedSystemInstanceProvider : IInstanceProvider
    {
        readonly Type systemType;
        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> customParameters;
        readonly string worldName;
        readonly Type systemGroupType;

        private World world;
        private UnmanagedSystemReference instance;

        public UnmanagedSystemInstanceProvider(
            Type systemType,
            string worldName,
            Type systemGroupType,
            IInjector injector,
            IReadOnlyList<IInjectParameter> customParameters)
        {
            this.systemType = systemType;
            this.worldName = worldName;
            this.systemGroupType = systemGroupType;
            this.injector = injector;
            this.customParameters = customParameters;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            if (world is null)
                world = GetWorld(resolver);

            if (instance is null)
            {
                SystemHandle handle = world.GetOrCreateSystem(systemType);
                injector.Inject(handle, resolver, customParameters);

                if (systemGroupType is not null)
                {
                    var systemGroup = (ComponentSystemGroup) world.GetOrCreateSystemManaged(systemGroupType);
                    systemGroup.AddSystemToUpdateList(handle);
                }
                Type refType = typeof(UnmanagedSystemReference<>);
                Type target = refType.MakeGenericType(systemType);
                instance = (UnmanagedSystemReference)Activator.CreateInstance(target, handle, world);
                return instance;
            }
            return instance;
        }

        private World GetWorld(IObjectResolver resolver)
        {
            if (worldName is null && World.DefaultGameObjectInjectionWorld != null)
                return World.DefaultGameObjectInjectionWorld;

            var worlds = resolver.Resolve<IEnumerable<World>>();
            foreach (World w in worlds)
            {
                if (w.Name == worldName)
                    return w;
            }
            throw new VContainerException(systemType, $"World `{worldName}` is not Created");
        }
    }
}
#endif
