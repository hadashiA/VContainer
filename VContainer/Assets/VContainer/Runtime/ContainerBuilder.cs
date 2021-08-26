using System;
using System.Collections.Generic;
using VContainer.Internal;
#if VCONTAINER_PARALLEL_CONTAINER_BUILD
using System.Threading.Tasks;
#endif

namespace VContainer
{
    /// <summary>
    /// Used to configure the dependencies that will be provided by an <see cref="IObjectResolver"/>
    /// at runtime.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface will generally be used at the start of your game and at major
    /// transition points (e.g. when loading new scenes).
    /// </para>
    /// <para>
    /// Instances of this interface will not affect containers after they're built.
    /// Do not pass it into <see cref="RegisterBuildCallback">build callbacks</see>,
    /// you can't modify containers that way.
    /// </para>
    /// </remarks>
    /// <seealso cref="ContainerBuilderExtensions"/>
    /// <seealso cref="IObjectResolver"/>
    public interface IContainerBuilder
    {
        /// <summary>
        /// The object that represents the application.
        /// </summary>
        /// <remarks>
        /// Will generally be an instance or subclass of <see cref="Unity.LifetimeScope"/>,
        /// but it can be anything that represents the application.
        /// </remarks>
        object ApplicationOrigin { get; set; }

        /// <summary>
        /// Adds a registration to this container builder.
        /// </summary>
        /// <remarks>
        /// You likely won't use this method directly; instead you'll want to use
        /// one of the extension methods defined in <see cref="ContainerBuilderExtensions"/>.
        /// </remarks>
        /// <param name="registrationBuilder">
        /// The registration builder that will be used to construct the registrations
        /// that resolve dependencies.
        /// </param>
        /// <typeparam name="T">
        /// The type of the registration builder.
        /// </typeparam>
        /// <returns>
        /// The provided <paramref name="registrationBuilder"/>, for fluently
        /// chaining further method calls.
        /// </returns>
        /// <seealso cref="ContainerBuilderExtensions"/>
        T Register<T>(T registrationBuilder) where T : RegistrationBuilder;

        /// <summary>
        /// Enqueues a <see langword="delegate"/> to be executed once the container
        /// is built.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Possible use cases include (but are not limited to):
        /// <list type="bullet">
        /// <item>
        /// Forcibly resolving dependencies even if they're not explicitly used.
        /// Try not to do that too often.
        /// </item>
        /// <item>
        /// Setting external configuration such as global event handlers.
        /// </item>
        /// <item>
        /// Signalling that your application is ready to proceed (e.g. that a
        /// level has finished loading).
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// Callbacks will be executed in the order they're registered.
        /// </para>
        /// </remarks>
        /// <param name="container">
        /// The callback to execute. The container it provides can be used to resolve dependencies.
        /// </param>
        void RegisterBuildCallback(Action<IObjectResolver> container);

        /// <summary>
        /// Returns <see langword="true"/> if this builder will register a
        /// dependency with the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// The <see cref="Type"/> of the dependency whose registration we're looking for.
        /// </param>
        /// <param name="includeInterfaceTypes">
        /// If <see langword="true"/>, looks for interfaces and base classes included
        /// in <see cref="IRegistration.InterfaceTypes"/> as well. Otherwise
        /// looks only for the registration's implementation type. Defaults to <see langword="false"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a dependency is registered to <paramref name="type"/>.
        /// </returns>
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

        readonly IList<RegistrationBuilder> registrationBuilders = new List<RegistrationBuilder>();
        List<Action<IObjectResolver>> buildCallbacks;

        public T Register<T>(T registrationBuilder) where T : RegistrationBuilder
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
            var registrations = BuildRegistrations();
            var registry = FixedTypeKeyHashTableRegistry.Build(registrations);
            var container = new Container(registry);
            EmitCallbacks(container);
            return container;
        }

        protected IReadOnlyList<IRegistration> BuildRegistrations()
        {
            var registrations = new IRegistration[registrationBuilders.Count + 1];

#if VCONTAINER_PARALLEL_CONTAINER_BUILD
            Parallel.For(0, registrationBuilders.Count, i =>
            {
                var registrationBuilder = registrationBuilders[i];
                var registration = registrationBuilder.Build();
                registrations[i] = registration;
            });
#else
            for (var i = 0; i < registrationBuilders.Count; i++)
            {
                var registrationBuilder = registrationBuilders[i];
                var registration = registrationBuilder.Build();
                registrations[i] = registration;
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
