using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public interface IContainerBuilder
    {
        RegistrationBuilder Register<T>(Lifetime lifetime);
        RegistrationBuilder RegisterInstance(object instance);
        RegistrationBuilder Register(RegistrationBuilder registrationBuilder);
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
            return container;
        }

        public override IObjectResolver Build() => BuildScope();
    }

    public class ContainerBuilder : IContainerBuilder
    {
        readonly IList<RegistrationBuilder> registrationBuilders = new List<RegistrationBuilder>();

        protected bool conterinerRegistered;

        public RegistrationBuilder Register<T>(Lifetime lifetime)
            => Register(new RegistrationBuilder(typeof(T), lifetime));

        public RegistrationBuilder RegisterInstance(object instance)
            => Register(new RegistrationBuilder(instance));

        public RegistrationBuilder Register(RegistrationBuilder registrationBuilder)
        {
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
#if VCONTAINER_PARALLEL_CONTAINER_BUILD
            var registrations = registrationBuilders
                .AsParallel()
                .Select(x => x.Build())
                .ToList();
#else
            var registrations = new List<IRegistration>(registrationBuilders.Count);
            foreach (var registrationBuilder in registrationBuilders)
            {
                registrations.Add(registrationBuilder.Build());
            }
#endif

#if VCONTAINER_PARALLEL_CONTAINER_BUILD
            registrations
                .AsParallel()
                .ForAll(x => TypeAnalyzer.CheckCircularDependency(x.ImplementationType));
#else
            foreach (var x in registrations)
            {
                TypeAnalyzer.CheckCircularDependency(x.ImplementationType);
            }
#endif
            if (conterinerRegistered)
            {
                registrations.Add(ContainerRegistration.Default);
            }
            return registrations;
        }
    }
}
