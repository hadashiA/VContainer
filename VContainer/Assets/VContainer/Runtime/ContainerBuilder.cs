using System.Collections.Generic;
using System.Linq;
using VContainer.Internal;

namespace VContainer
{
    public interface IContainerBuilder
    {
        RegistrationBuilder Register<T>(Lifetime lifetime);
        RegistrationBuilder RegisterInstance(object instance);
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
            var registrations = BuildRegistrations();
            var registry = FixedTypeKeyHashTableRegistry.Build(registrations);
            var container = new ScopedContainer(registry, root, parent);
            // registry.AddSystemRegistration(container, ContainerRegistered);
            return container;
        }

        public override IObjectResolver Build() => BuildScope();
    }

    public class ContainerBuilder : IContainerBuilder
    {
        readonly IList<RegistrationBuilder> registrationBuilders = new List<RegistrationBuilder>();

        protected bool conterinerRegistered;

        public RegistrationBuilder Register<T>(Lifetime lifetime)
        {
            var registrationBuilder = new RegistrationBuilder(typeof(T), lifetime);
            registrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public RegistrationBuilder RegisterInstance(object instance)
        {
            var registrationBuilder = new RegistrationBuilder(instance);
            registrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public void RegisterContainer() => conterinerRegistered = true;

        public virtual IObjectResolver Build()
        {
            var registrations = BuildRegistrations();
            var registry = FixedTypeKeyHashTableRegistry.Build(registrations);
            var container = new Container(registry);
            return container;
        }

        protected IReadOnlyList<IRegistration> BuildRegistrations()
        {
            var registrations = registrationBuilders
                // .AsParallel()
                .Select(x => x.Build())
                .ToList();

            // registrations
            //     .AsParallel()
            //     .ForAll(x => TypeAnalyzer.CheckCircularDependency(x.ImplementationType));
            foreach (var x in registrations)
            {
                TypeAnalyzer.CheckCircularDependency(x.ImplementationType);
            }

            if (conterinerRegistered)
            {
                registrations.Add(ContainerRegistration.Default);
            }
            return registrations;
        }
    }
}
