#if VCONTAINER_ECS_INTEGRATION && UNITY_2022_2_OR_NEWER
using System;
using System.Collections.Generic;
using Unity.Entities;
using VContainer.Internal;

namespace VContainer.Unity
{
    public sealed class UnmanagedSystemRegistrationBuilder : RegistrationBuilder
    {
        readonly string worldName;
        Type systemGroupType;

        internal UnmanagedSystemRegistrationBuilder(Type implementationType, string worldName)
            : base(implementationType, default)
        {
            this.worldName = worldName;
            Type refType = typeof(UnmanagedSystemReference<>);
            Type[] typeArgs = { implementationType };
            Type refImplType = refType.MakeGenericType(typeArgs);
            InterfaceTypes = new List<Type>
            {
                typeof(UnmanagedSystemReference),
                refImplType
            };
        }

        public override Registration Build()
        {
            IInjector injector = InjectorCache.GetOrBuild(ImplementationType);
            var provider = new UnmanagedSystemInstanceProvider(
                ImplementationType,
                worldName,
                systemGroupType,
                injector,
                Parameters);
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, provider);
        }

        public UnmanagedSystemRegistrationBuilder IntoGroup<T>() where T : ComponentSystemGroup
        {
            systemGroupType = typeof(T);
            return this;
        }
    }
}
#endif
