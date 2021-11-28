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

        public override Registration Build()
        {
            var provider = new WorldInstanceProvider(name, initialization);
            return new Registration(typeof(World), Lifetime, null, provider);
        }
    }
}
#endif