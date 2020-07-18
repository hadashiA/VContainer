#if VCONTAINER_ECS_INTEGRATION
using Unity.Entities;

namespace VContainer.Unity
{
    public sealed class WorldRegistrationBuilder : RegistrationBuilder
    {
        readonly string name;

        public WorldRegistrationBuilder(string name, Lifetime lifetime)
            : base(typeof(World), lifetime)
        {
            this.name = name;
        }

        public override IRegistration Build()
        {
            return new WorldRegistration(name, Lifetime);
        }
    }
}
#endif