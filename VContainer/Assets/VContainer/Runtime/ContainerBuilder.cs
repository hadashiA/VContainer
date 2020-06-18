using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public interface IContainerBuilder
    {
        RegistrationBuilder Register<T>(Lifetime lifetime);
        RegistrationBuilder RegisterInstance<T>(T instance);
        void RegisterContainer();
        IObjectResolver Build();
    }

    public sealed class ScopedContainerBuilder : ContainerBuilder
    {
        readonly IObjectResolver root;
        readonly IScopedObjectResolver parent;

        internal ScopedContainerBuilder(
            IObjectResolver root,
            IScopedObjectResolver parent)
        {
            this.root = root;
            this.parent = parent;
        }

        public IScopedObjectResolver BuildScope()
        {
            var registry = FixedTypeKeyHashTableRegistry.Build(RegistrationBuilders);
            var container = new ScopedContainer(registry, root, parent);
            // registry.AddSystemRegistration(container, ContainerRegistered);
            return container;
        }

        public override IObjectResolver Build() => BuildScope();
    }

    public class ContainerBuilder : IContainerBuilder
    {
        protected readonly IList<RegistrationBuilder> RegistrationBuilders = new List<RegistrationBuilder>();

        protected bool ContainerRegistered;

        public RegistrationBuilder Register<T>(Lifetime lifetime)
        {
            var registrationBuilder = new RegistrationBuilder(typeof(T), lifetime);
            RegistrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public RegistrationBuilder RegisterInstance<T>(T instance)
        {
            var registrationBuilder = new RegistrationBuilder(typeof(T), instance);
            RegistrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public void RegisterContainer() => ContainerRegistered = true;

        public virtual IObjectResolver Build()
        {
            var registry = FixedTypeKeyHashTableRegistry.Build(RegistrationBuilders);
            var container = new Container(registry);
            // registry.AddSystemRegistration(container, ContainerRegistered);
            return container;
        }
    }
}
