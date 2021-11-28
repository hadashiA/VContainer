#if VCONTAINER_ECS_INTEGRATION
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace VContainer.Unity
{
    public sealed class SystemInstanceProvider : IInstanceProvider
    {
        readonly Type systemType;
        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> customParameters;
        readonly string worldName;
        readonly Type systemGroupType;

        World world;
        ComponentSystemBase instance;

        public SystemInstanceProvider(
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
                instance = (ComponentSystemBase)injector.CreateInstance(resolver, customParameters);
                world.AddSystem(instance);

                if (systemGroupType != null)
                {
                    var systemGroup = (ComponentSystemGroup)world.GetOrCreateSystem(systemGroupType);
                    systemGroup.AddSystemToUpdateList(instance);
                }

                return instance;
            }
            return world.GetExistingSystem(systemType);
        }

        World GetWorld(IObjectResolver resolver)
        {
            if (worldName is null && World.DefaultGameObjectInjectionWorld != null)
                return World.DefaultGameObjectInjectionWorld;

            var worlds = resolver.Resolve<IEnumerable<World>>();
            foreach (var world in worlds)
            {
                if (world.Name == worldName)
                    return world;
            }
            throw new VContainerException(systemType, $"World `{worldName}` is not registered");
        }
    }
}
#endif
