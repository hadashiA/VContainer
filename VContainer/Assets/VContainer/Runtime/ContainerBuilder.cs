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

        RegistrationBuilder Register(Type type, Lifetime lifetime);
        RegistrationBuilder RegisterInstance(object instance);
        RegistrationBuilder Register(RegistrationBuilder registrationBuilder);
        void RegisterBuildCallback(Action<IObjectResolver> container);

        bool Exists(Type type, bool includeInterfaceTypes = false);
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
            var (registrations, callbacks) = BuildRegistrations();
            var registry = FixedTypeKeyHashTableRegistry.Build(registrations);
            var container = new ScopedContainer(registry, root, parent);
            EmitCallbacks(container, callbacks);
            return container;
        }

        public override IObjectResolver Build() => BuildScope();
    }

    public class ContainerBuilder : IContainerBuilder
    {
        public object ApplicationOrigin { get; set; }

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

        public bool Exists(Type type, bool includeInterfaceTypes = false)
        {
            foreach (var registrationBuilder in registrationBuilders)
            {
                if (registrationBuilder.ImplementationType == type ||
                    includeInterfaceTypes && registrationBuilder.InterfaceTypes?.Contains(type) == true)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual IObjectResolver Build()
        {
            var (registrations, callbacks) = BuildRegistrations();
            var registry = FixedTypeKeyHashTableRegistry.Build(registrations);
            var container = new Container(registry);
            EmitCallbacks(container, callbacks);
            return container;
        }

        protected (IReadOnlyList<IRegistration>, IReadOnlyList<(IRegistration, Action<IRegistration, IObjectResolver>)>) BuildRegistrations()
        {
            var registrations = new IRegistration[registrationBuilders.Count + 1];
            var callbacks = new List<(IRegistration, Action<IRegistration, IObjectResolver>)>(registrations.Length);

#if VCONTAINER_PARALLEL_CONTAINER_BUILD
            Parallel.For(0, registrationBuilders.Count, i =>
            {
                var registrationBuilder = registrationBuilders[i];
                var registration = registrationBuilder.Build();
                registrations[i] = registration;

                if (registrationBuilder.BuildCallback is Action<IRegistration, IObjectResolver> callback)
                {
                    callbacks.Add((registration, callback));
                }
            });
#else
            for (var i = 0; i < registrationBuilders.Count; i++)
            {
                var registrationBuilder = registrationBuilders[i];
                var registration = registrationBuilder.Build();
                registrations[i] = registration;

                if (registrationBuilder.BuildCallback is Action<IRegistration, IObjectResolver> callback)
                {
                    callbacks.Add((registration, callback));
                }
            }
#endif
            registrations[registrations.Length - 1] = ContainerRegistration.Default;

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
            return (registrations, callbacks);
        }

        protected void EmitCallbacks(
            IObjectResolver container,
            IEnumerable<(IRegistration, Action<IRegistration, IObjectResolver>)> callbacks)
        {
            foreach (var x in callbacks)
            {
                var (registration, callback) = x;
                callback.Invoke(registration, container);
            }

            if (buildCallbacks == null) return;

            foreach (var callback in buildCallbacks)
            {
                callback.Invoke(container);
            }
        }
    }
}
