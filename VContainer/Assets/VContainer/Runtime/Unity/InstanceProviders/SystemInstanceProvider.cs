#if VCONTAINER_ECS_INTEGRATION
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace VContainer.Unity
{
    public sealed class SystemInstanceProvider<T> : IInstanceProvider where T : ComponentSystemBase
    {
        readonly Type systemType;
        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> customParameters;
        readonly string worldName;
        readonly Type systemGroupType;

        World world;
        T instance;

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
                instance = (T) injector.CreateInstance(resolver, customParameters);
#if UNITY_2022_2_OR_NEWER
                world.AddSystemManaged(instance);
#else
                world.AddSystem(instance);
#endif

                if (systemGroupType != null)
                {
#if UNITY_2022_2_OR_NEWER
                    var systemGroup = (ComponentSystemGroup)world.GetOrCreateSystemManaged(systemGroupType);
#else
                    var systemGroup = (ComponentSystemGroup)world.GetOrCreateSystem(systemGroupType);
#endif
                    systemGroup.AddSystemToUpdateList(instance);
                }

                return instance;
            }
#if UNITY_2022_2_OR_NEWER
            return world.GetExistingSystemManaged(systemType);
#else
            return world.GetExistingSystem(systemType);
#endif
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
