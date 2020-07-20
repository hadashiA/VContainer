#if VCONTAINER_ECS_INTEGRATION
using System;
using Unity.Entities;

namespace VContainer.Unity
{
    public sealed class WorldRegistrationBuilder : RegistrationBuilder
    {
        readonly string name;
        readonly Action<World> initialization;

        public WorldRegistrationBuilder(string name, Lifetime lifetime, Action<World> initialization)
            : base(typeof(World), lifetime)
        {
            this.name = name;
            this.initialization = initialization;
        }

        public override IRegistration Build()
        {
            return new WorldRegistration(name, Lifetime, initialization);
        }
    }
}
#endif