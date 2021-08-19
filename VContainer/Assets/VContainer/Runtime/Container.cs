using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using VContainer.Internal;

namespace VContainer
{
    /// <summary>
    /// A container from which dependencies can be retrieved.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This documentation uses "container" and "resolver" interchangeably.
    /// </para>
    /// <para>
    /// When disposed of with <see cref="IDisposable.Dispose"/>, all registered
    /// dependencies that implement <see cref="IDisposable"/> will be disposed
    /// of except for registered instances and <see cref="Lifetime.Transient"/>
    /// dependencies.
    /// </para>
    /// </remarks>
    public interface IObjectResolver : IDisposable
    {
        /// <summary>
        /// Retrieves the dependency mapped to the given <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// Most dependencies will be constructed the first time they're resolved.
        /// The main exceptions are <see cref="Lifetime.Transient"/> dependencies
        /// and registered instances.
        /// </remarks>
        /// <param name="type">
        /// The type of the dependency to resolve. Note that multiple types can
        /// map to the same dependency.
        /// </param>
        /// <exception cref="VContainerException">
        /// No dependency is registered to <paramref name="type"/>.
        /// </exception>
        /// <returns>The dependency that is mapped to <paramref name="type"/>.</returns>
        /// <seealso cref="IObjectResolverExtensions.Resolve{T}"/>
        object Resolve(Type type);

        /// <summary>
        /// Resolves a dependency from the given <paramref name="registration"/>.
        /// </summary>
        /// <remarks>
        /// This method is mostly used internally. Application code should prefer using
        /// <see cref="IObjectResolverExtensions.Resolve{T}"/> instead.
        /// </remarks>
        /// <param name="registration">
        /// The registration used to resolve the requested dependency. Doesn't technically
        /// need to be a part of this <see cref="IObjectResolver"/>.
        /// </param>
        /// <returns>The dependency that <paramref name="registration"/> resolved.</returns>
        /// <seealso cref="IObjectResolverExtensions.Resolve{T}"/>
        /// <seealso cref="Resolve(System.Type)"/>
        object Resolve(IRegistration registration);

        /// <summary>
        /// Creates a child <see cref="IScopedObjectResolver"/> that can resolve this
        /// container's dependencies or register its own.
        /// </summary>
        /// <param name="installation">
        /// A delegate used to configure the newly-created container. Defaults to
        /// <see langword="null"/>, which does not register any new dependencies.
        /// </param>
        /// <returns>
        /// A new container that will use this one to resolve any dependencies that
        /// it hasn't registered itself.
        /// </returns>
        /// <seealso cref="Lifetime"/>
        IScopedObjectResolver CreateScope(Action<IContainerBuilder> installation = null);

        /// <summary>
        /// Provides an existing object with the dependencies it needs to function.
        /// </summary>
        /// <remarks>
        /// This method will call all methods annotated with <see cref="InjectAttribute"/>.
        /// It will also populate all properties and fields annotated with <see cref="InjectAttribute"/>.
        /// </remarks>
        /// <param name="instance">
        /// The object that will be injected with its requested dependencies.
        /// </param>
        /// <exception cref="VContainerException">
        /// A dependency that <paramref name="instance"/> requires couldn't be resolved.
        /// </exception>
        /// <seealso cref="InjectAttribute"/>
        /// <seealso cref="Unity.ObjectResolverUnityExtensions"/>
        void Inject(object instance);
    }

    /// <summary>
    /// A container from which dependencies can be retrieved.
    /// </summary>
    /// <remarks>
    /// "container" and "resolver" are often used interchangeably.
    /// </remarks>
    public interface IScopedObjectResolver : IObjectResolver
    {
        IObjectResolver Root { get; }
        IScopedObjectResolver Parent { get; }
        bool TryGetRegistration(Type type, out IRegistration registration);
    }

    /// <summary>
    /// Describes the rules that determine when a dependency is constructed.
    /// </summary>
    public enum Lifetime
    {
        /// <summary>
        /// <c>Transient</c> dependencies are instantiated each time they're resolved.
        /// </summary>
        /// <remarks>
        /// <para>
        /// We don't recommend registering an <see cref="IDisposable"/>-implementing
        /// dependency as <c>Transient</c>. VContainer will <b>not</b> dispose of
        /// disposable <c>Transient</c> dependencies. Although you can do so manually
        /// in your own code, refactoring the dependency's <see cref="Lifetime"/>
        /// may lead to non-obvious bugs.
        /// </para>
        /// </remarks>
        Transient,

        /// <summary>
        /// <c>Singleton</c> dependencies are instantiated exactly once within a
        /// tree of <see cref="IObjectResolver"/>s.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is an error to register more than one <c>Singleton</c> dependency
        /// with the same type. Doing so will throw a <see cref="VContainerException"/>.
        /// </para>
        /// <para>
        /// When an <see cref="IObjectResolver"/> is disposed of, <c>Singleton</c>
        /// dependencies that are <see cref="IDisposable"/> will be disposed of
        /// as well, <i>except</i> for registered instances.
        /// </para>
        /// </remarks>
        /// <seealso cref="ContainerBuilderExtensions.RegisterInstance{T}"/>
        Singleton,

        /// <summary>
        /// <c>Scoped</c> dependencies are instantiated once, but can be overridden
        /// in child scopes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When an <see cref="IObjectResolver"/> is disposed of, <c>Scoped</c>
        /// dependencies that are <see cref="IDisposable"/> will be disposed of as well.
        /// </para>
        /// <para>
        /// If your game only has one <see cref="Unity.LifetimeScope"/>, then this
        /// is equivalent to <see cref="Singleton"/>.
        /// </para>
        /// </remarks>
        Scoped
    }

    public sealed class ScopedContainer : IScopedObjectResolver
    {
        public IObjectResolver Root { get; }
        public IScopedObjectResolver Parent { get; }

        readonly IRegistry registry;
        readonly ConcurrentDictionary<IRegistration, Lazy<object>> sharedInstances = new ConcurrentDictionary<IRegistration, Lazy<object>>();
        readonly CompositeDisposable disposables = new CompositeDisposable();
        readonly Func<IRegistration, Lazy<object>> createInstance;

        internal ScopedContainer(
            IRegistry registry,
            IObjectResolver root,
            IScopedObjectResolver parent = null)
        {
            this.registry = registry;
            Root = root;
            Parent = parent;
            createInstance = registration =>
            {
                return new Lazy<object>(() => registration.SpawnInstance(this));
            };
        }

        public object Resolve(Type type)
        {
            var registration = FindRegistration(type);
            return Resolve(registration);
        }

        public object Resolve(IRegistration registration)
        {
            switch (registration.Lifetime)
            {
                case Lifetime.Transient:
                    return registration.SpawnInstance(this);

                case Lifetime.Singleton:
                    if (Parent is null)
                        return Root.Resolve(registration);

                    if (!registry.Exists(registration.ImplementationType))
                        return Parent.Resolve(registration);

                    return CreateTrackedInstance(registration);

                case Lifetime.Scoped:
                    return CreateTrackedInstance(registration);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IScopedObjectResolver CreateScope(Action<IContainerBuilder> installation = null)
        {
            var containerBuilder = new ScopedContainerBuilder(Root, this);
            installation?.Invoke(containerBuilder);
            return containerBuilder.BuildScope();
        }

        public void Inject(object instance)
        {
            var injector = InjectorCache.GetOrBuild(instance.GetType());
            injector.Inject(instance, this, null);
        }

        public bool TryGetRegistration(Type type, out IRegistration registration)
            => registry.TryGet(type, out registration);

        public void Dispose()
        {
            disposables.Dispose();
            sharedInstances.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object CreateTrackedInstance(IRegistration registration)
        {
            var lazy = sharedInstances.GetOrAdd(registration, createInstance);
            if (!lazy.IsValueCreated && lazy.Value is IDisposable disposable && !(registration is InstanceRegistration))
            {
                disposables.Add(disposable);
            }
            return lazy.Value;
        }

        IRegistration FindRegistration(Type type)
        {
            IScopedObjectResolver scope = this;
            while (scope != null)
            {
                if (scope.TryGetRegistration(type, out var registration))
                {
                    return registration;
                }
                scope = scope.Parent;
            }
            throw new VContainerException(type, $"No such registration of type: {type}");
        }
    }

    public sealed class Container : IObjectResolver
    {
        readonly IRegistry registry;
        readonly IScopedObjectResolver rootScope;
        readonly ConcurrentDictionary<IRegistration, Lazy<object>> sharedInstances = new ConcurrentDictionary<IRegistration, Lazy<object>>();
        readonly CompositeDisposable disposables = new CompositeDisposable();
        readonly Func<IRegistration, Lazy<object>> createInstance;

        internal Container(IRegistry registry)
        {
            this.registry = registry;
            rootScope = new ScopedContainer(registry, this);
            createInstance = registration =>
            {
                return new Lazy<object>(() => registration.SpawnInstance(this));
            };
        }

        public object Resolve(Type type)
        {
            if (registry.TryGet(type, out var registration))
            {
                return Resolve(registration);
            }
            throw new VContainerException(type, $"No such registration of type: {type}");
        }

        public object Resolve(IRegistration registration)
        {
            switch (registration.Lifetime)
            {
                case Lifetime.Transient:
                    return registration.SpawnInstance(this);

                case Lifetime.Singleton:
                    var singleton = sharedInstances.GetOrAdd(registration, createInstance);
                    if (!singleton.IsValueCreated && singleton.Value is IDisposable disposable && !(registration is InstanceRegistration))
                    {
                        disposables.Add(disposable);
                    }
                    return singleton.Value;

                case Lifetime.Scoped:
                    return rootScope.Resolve(registration);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IScopedObjectResolver CreateScope(Action<IContainerBuilder> installation = null)
            => rootScope.CreateScope(installation);

        public void Inject(object instance)
        {
            var injector = InjectorCache.GetOrBuild(instance.GetType());
            injector.Inject(instance, this, null);
        }

        public void Dispose()
        {
            rootScope.Dispose();
            disposables.Dispose();
            sharedInstances.Clear();
        }
    }
}

