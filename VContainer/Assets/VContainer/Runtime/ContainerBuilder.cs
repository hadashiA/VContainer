using System;
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

        RegistrationBuilder Register(Type type, Lifetime lifetime);
        RegistrationBuilder RegisterInstance(object instance);
        RegistrationBuilder Register(RegistrationBuilder registrationBuilder);

        void RegisterBuildCallback(Action<IObjectResolver> container);

        IObjectResolver Build();
    }

    public sealed class ScopedContainerBuilder : ContainerBuilder
    {
        readonly IObjectResolver root;
        readonly IScopedObjectResolver parent;

        internal ScopedContainerBuilder(IObjectResolver root, IScopedObjectResolver parent)
        {
            this.root = root;
            this.parent = parent;
        }

        public IScopedObjectResolver BuildScope()
        {
            var registrations = BuildRegistrations();
            var registry = FixedTypeKeyHashTableRegistry.Build(registrations);
            var container = new ScopedContainer(registry, root, parent);
            EmitCallbacks(container);
            return container;
        }

        public override IObjectResolver Build() => BuildScope();
    }

    public class ContainerBuilder : IContainerBuilder
    {
        public object ApplicationOrigin { get; set; }
        public bool ContainerExposed { get; set; }

        readonly IList<RegistrationBuilder> registrationBuilders = new List<RegistrationBuilder>();
        List<Action<IObjectResolver>> buildCallbacks;

        public RegistrationBuilder Register(Type type, Lifetime lifetime)
            => Register(new RegistrationBuilder(type, lifetime));

        public RegistrationBuilder RegisterInstance(object instance)
            => Register(new RegistrationBuilder(instance));

        public RegistrationBuilder Register(RegistrationBuilder registrationBuilder)
        {
            registrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public void RegisterBuildCallback(Action<IObjectResolver> callback)
        {
            if (buildCallbacks == null)
                buildCallbacks = new List<Action<IObjectResolver>>();
            buildCallbacks.Add(callback);
        }

        public virtual IObjectResolver Build()
        {
            var registrations = BuildRegistrations();
            var registry = FixedTypeKeyHashTableRegistry.Build(registrations);
            var container = new Container(registry);
            EmitCallbacks(container);
            return container;
        }

        protected IReadOnlyList<IRegistration> BuildRegistrations()
        {
            var registrations = new IRegistration[registrationBuilders.Count + (ContainerExposed ? 1 : 0)];

#if VCONTAINER_PARALLEL_CONTAINER_BUILD
            Parallel.For(0, registrationBuilders.Count, i =>
            {
                registrations[i] = registrationBuilders[i].Build();
            });

#else
            for (var i = 0; i < registrationBuilders.Count; i++)
            {
                registrations[i] = registrationBuilders[i].Build();
            }
#endif
            if (ContainerExposed)
            {
                registrations[registrations.Length - 1] = ContainerRegistration.Default;
            }

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
            return registrations;
        }

        protected void EmitCallbacks(IObjectResolver container)
        {
            if (buildCallbacks == null) return;

            foreach (var callback in buildCallbacks)
            {
                callback.Invoke(container);
            }
        }
    }
}
