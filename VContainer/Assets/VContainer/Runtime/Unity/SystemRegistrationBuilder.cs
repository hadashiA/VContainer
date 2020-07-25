#if VCONTAINER_ECS_INTEGRATION
using System;
using System.Collections.Generic;
using Unity.Entities;
using VContainer.Internal;

namespace VContainer.Unity
{
    public sealed class SystemRegistrationBuilder : RegistrationBuilder
    {
        readonly string worldName;
        Type systemGroupType;

        internal SystemRegistrationBuilder(
            Type implementationType,
            string worldName,
            List<Type> interfaceTypes = null)
        : base(implementationType, default, interfaceTypes)
        {
            this.worldName = worldName;
            InterfaceTypes = InterfaceTypes ?? new List<Type>();
            InterfaceTypes.Add(typeof(ComponentSystemBase));
            InterfaceTypes.Add(ImplementationType);
        }

        public override IRegistration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);

            return new SystemRegistration(
                ImplementationType,
                InterfaceTypes,
                Parameters,
                injector,
                worldName,
                systemGroupType);
        }

        public SystemRegistrationBuilder IntoGroup<T>() where T : ComponentSystemGroup
        {
            systemGroupType = typeof(T);
            return this;
        }
    }
}
#endif