using System.Collections.Generic;
using VContainer.Internal;
#if VCONTAINER_PARALLEL_CONTAINER_BUILD
using System.Threading.Tasks;
#endif

namespace VContainer
{
    public interface IContainerBuilder
    {
        object ApplicationOrigin { get; set; }
        bool ContainerExposed { get; set; }

        RegistrationBuilder Register<T>(Lifetime lifetime);
        RegistrationBuilder RegisterInstance(object instance);

        RegistrationBuilder Register(RegistrationBuilder registrationBuilder);

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
        public object ApplicationOrigin { get; set; }
        public bool ContainerExposed { get; set; }

        readonly IList<RegistrationBuilder> registrationBuilders = new List<RegistrationBuilder>();

        public RegistrationBuilder Register<T>(Lifetime lifetime)
            => Register(new RegistrationBuilder(typeof(T), lifetime));

        public RegistrationBuilder RegisterInstance(object instance)
            => Register(new RegistrationBuilder(instance));

        public RegistrationBuilder Register(RegistrationBuilder registrationBuilder)
        {
            registrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

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
            var a = new IRegistration[registrationBuilders.Count];
            Parallel.For(0, registrationBuilders.Count, i =>
            {
                a[i] = registrationBuilders[i].Build();
            });

            var registrations = new List<IRegistration>(a);
#else
            var registrations = new List<IRegistration>(registrationBuilders.Count);
            foreach (var registrationBuilder in registrationBuilders)
            {
                registrations.Add(registrationBuilder.Build());
            }
#endif

#if VCONTAINER_PARALLEL_CONTAINER_BUILD
            Parallel.For(0, registrations.Count, i =>
            {
                TypeAnalyzer.CheckCircularDependency(registrations[i].ImplementationType);
            });
#else
            foreach (var x in registrations)
            {
                TypeAnalyzer.CheckCircularDependency(x.ImplementationType);
            }
#endif
            if (ContainerExposed)
            {
                registrations.Add(ContainerRegistration.Default);
            }
            registrations.Add(ScopeFactoryRegistration.Default);
            return registrations;
        }
    }
}
