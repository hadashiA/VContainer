#if VCONTAINER_ECS_INTEGRATION
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace VContainer.Unity
{
    public sealed class SystemRegistration : IRegistration
    {
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes { get; }
        public Lifetime Lifetime => Lifetime.Transient;

        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> parameters;

        readonly string worldName;
        readonly Type systemGroupType;

        World world;
        ComponentSystemBase instance;

        public SystemRegistration(
            Type implementationType,
            IReadOnlyList<Type> interfaceTypes,
            IReadOnlyList<IInjectParameter> parameters,
            IInjector injector,
            string worldName,
            Type systemGroupType)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;

            this.injector = injector;
            this.parameters = parameters;
            this.worldName = worldName;
            this.systemGroupType = systemGroupType;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            if (world is null)
                world = GetWorld(resolver);

            if (instance is null)
            {
                instance = (ComponentSystemBase)injector.CreateInstance(resolver, parameters);
                world.AddSystem(instance);

                if (systemGroupType != null)
                {
                    var systemGroup = (ComponentSystemGroup)world.GetOrCreateSystem(systemGroupType);
                    systemGroup.AddSystemToUpdateList(instance);
                }

                return instance;
            }
            return world.GetExistingSystem(ImplementationType);
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
            throw new VContainerException(ImplementationType, $"World `{worldName}` is not registered");
        }
    }
}
#endif