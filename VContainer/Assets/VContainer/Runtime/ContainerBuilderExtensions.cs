using System;
using VContainer.Internal;

namespace VContainer
{
    /// <summary>
    /// Extension methods to simplify the use of <see cref="IContainerBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Most uses of <see cref="IContainerBuilder"/> will be through extension methods
    /// such as these.
    /// </remarks>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Register a dependency that can be resolved as the given <paramref name="type"/>.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IContainerBuilder"/> that will register this dependency.
        /// </param>
        /// <param name="type">
        /// The <see cref="Type"/> to register and use for resolution. It must be
        /// a concrete type, i.e. <see langword="interface"/>s and <see langword="abstract"/>
        /// classes cannot be used.
        /// </param>
        /// <param name="lifetime">
        /// The lifetime of the resolved dependency (i.e. the rules of when it will be constructed).
        /// </param>
        /// <returns>
        /// A <see cref="RegistrationBuilder"/> that can be used to further configure
        /// this dependency.
        /// </returns>
        public static RegistrationBuilder Register(
            this IContainerBuilder builder,
            Type type,
            Lifetime lifetime)
            => builder.Register(new RegistrationBuilder(type, lifetime));

        /// <summary>
        /// Register a dependency that can be resolved as the given type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="Type"/> to register and use for resolution. It must be
        /// a concrete type, i.e. <see langword="interface"/>s and <see langword="abstract"/>
        /// classes cannot be used.
        /// </typeparam>
        /// <param name="builder">
        /// The <see cref="IContainerBuilder"/> that will register this dependency.
        /// </param>
        /// <param name="lifetime">
        /// The lifetime of the resolved dependency (i.e. the rules of when it will be constructed).
        /// </param>
        /// <returns>
        /// A <see cref="RegistrationBuilder"/> that can be used to further configure
        /// this dependency.
        /// </returns>
        public static RegistrationBuilder Register<T>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            => builder.Register(typeof(T), lifetime);

        /// <summary>
        /// Register a dependency that can be resolved as its own type or as an
        /// interface it implements.
        /// </summary>
        /// <typeparam name="TInterface">
        /// An <see langword="interface"/> that <typeparamref name="TImplement"/>
        /// implements or a base class that it extends.
        /// </typeparam>
        /// <typeparam name="TImplement">
        /// The <see cref="Type"/> to register and use for resolution. It must be
        /// a concrete type, i.e. <see langword="interface"/>s and <see langword="abstract"/>
        /// classes cannot be used.
        /// </typeparam>
        /// <param name="builder">
        /// The <see cref="IContainerBuilder"/> that will register this dependency.
        /// </param>
        /// <param name="lifetime">
        /// The lifetime of the resolved dependency (i.e. the rules of when it will be constructed).
        /// </param>
        /// <returns>
        /// A <see cref="RegistrationBuilder"/> that can be used to further configure
        /// this dependency.
        /// </returns>
        public static RegistrationBuilder Register<TInterface, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            where TImplement : TInterface
            => builder.Register<TImplement>(lifetime).As<TInterface>();

        /// <summary>
        /// Register a dependency that can be resolved as its own type or as two
        /// of the interfaces it implements.
        /// </summary>
        /// <typeparam name="TInterface1">
        /// An <see langword="interface"/> that <typeparamref name="TImplement"/>
        /// implements or a base class that it extends.
        /// </typeparam>
        /// <typeparam name="TInterface2">
        /// An <see langword="interface"/> that <typeparamref name="TImplement"/>
        /// implements or a base class that it extends.
        /// </typeparam>
        /// <typeparam name="TImplement">
        /// The <see cref="Type"/> to register and use for resolution. It must be
        /// a concrete type, i.e. <see langword="interface"/>s and <see langword="abstract"/>
        /// classes cannot be used.
        /// </typeparam>
        /// <param name="builder">
        /// The <see cref="IContainerBuilder"/> that will register this dependency.
        /// </param>
        /// <param name="lifetime">
        /// The lifetime of the resolved dependency (i.e. the rules of when it will be constructed).
        /// </param>
        /// <returns>
        /// A <see cref="RegistrationBuilder"/> that can be used to further configure
        /// this dependency.
        /// </returns>
        public static RegistrationBuilder Register<TInterface1, TInterface2, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            where TImplement : TInterface1, TInterface2
            => builder.Register<TImplement>(lifetime).As(typeof(TInterface1), typeof(TInterface2));

        /// <summary>
        /// Register a dependency that can be resolved as its own type or as three
        /// of the interfaces it implements.
        /// </summary>
        /// <remarks>
        /// If you need to resolve a type using more than three interfaces, use
        /// one of <see cref="RegistrationBuilder"/>'s methods.
        /// </remarks>
        /// <typeparam name="TInterface1">
        /// An <see langword="interface"/> that <typeparamref name="TImplement"/>
        /// implements or a base class that it extends.
        /// </typeparam>
        /// <typeparam name="TInterface2">
        /// An <see langword="interface"/> that <typeparamref name="TImplement"/>
        /// implements or a base class that it extends.
        /// </typeparam>
        /// <typeparam name="TInterface3">
        /// An <see langword="interface"/> that <typeparamref name="TImplement"/>
        /// implements or a base class that it extends.
        /// </typeparam>
        /// <typeparam name="TImplement">
        /// The <see cref="Type"/> to register and use for resolution. It must be
        /// a concrete type, i.e. <see langword="interface"/>s and <see langword="abstract"/>
        /// classes cannot be used.
        /// </typeparam>
        /// <param name="builder">
        /// The <see cref="IContainerBuilder"/> that will register this dependency.
        /// </param>
        /// <param name="lifetime">
        /// The lifetime of the resolved dependency (i.e. the rules of when it will be constructed).
        /// </param>
        /// <returns>
        /// A <see cref="RegistrationBuilder"/> that can be used to further configure
        /// this dependency.
        /// </returns>
        /// <seealso cref="RegistrationBuilder.As{TInterface}"/>
        public static RegistrationBuilder Register<TInterface1, TInterface2, TInterface3, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            where TImplement : TInterface1, TInterface2, TInterface3
            => builder.Register<TImplement>(lifetime).As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));

        public static RegistrationBuilder Register<TInterface>(
            this IContainerBuilder builder,
            Func<IObjectResolver, TInterface> implementationConfiguration,
            Lifetime lifetime)
            where TInterface : class
            => builder.Register(new FuncRegistrationBuilder(implementationConfiguration, typeof(TInterface), lifetime));

        public static RegistrationBuilder RegisterInstance<TInterface>(
            this IContainerBuilder builder,
            TInterface instance)
            => builder.Register(new InstanceRegistrationBuilder(instance)).As(typeof(TInterface));

        public static RegistrationBuilder RegisterInstance<TInterface1, TInterface2>(
            this IContainerBuilder builder,
            TInterface1 instance)
            => builder.RegisterInstance(instance).As(typeof(TInterface1), typeof(TInterface2));

        public static RegistrationBuilder RegisterInstance<TInterface1, TInterface2, TInterface3>(
            this IContainerBuilder builder,
            TInterface1 instance)
            => builder.RegisterInstance(instance).As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));

        public static RegistrationBuilder RegisterFactory<T>(
            this IContainerBuilder builder,
            Func<T> factory)
            => builder.RegisterInstance(factory);

        public static RegistrationBuilder RegisterFactory<TParam1, T>(
            this IContainerBuilder builder,
            Func<TParam1, T> factory)
            => builder.RegisterInstance(factory);

        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, T>(
            this IContainerBuilder builder,
            Func<TParam1, TParam2, T> factory)
            => builder.RegisterInstance(factory);

        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, TParam3, T>(
            this IContainerBuilder builder,
            Func<TParam1, TParam2, TParam3, T> factory)
            => builder.RegisterInstance(factory);

        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, TParam3, TParam4, T>(
            this IContainerBuilder builder,
            Func<TParam1, TParam2, TParam3, TParam4, T> factory)
            => builder.RegisterInstance(factory);

        public static RegistrationBuilder RegisterFactory<T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, Func<T>> factoryFactory,
            Lifetime lifetime)
            => builder.Register(new FuncRegistrationBuilder(factoryFactory, typeof(Func<T>), lifetime));

        public static RegistrationBuilder RegisterFactory<TParam1, T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, Func<TParam1, T>> factoryFactory,
            Lifetime lifetime)
            => builder.Register(new FuncRegistrationBuilder(factoryFactory, typeof(Func<TParam1, T>), lifetime));

        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, Func<TParam1, TParam2, T>> factoryFactory,
            Lifetime lifetime)
            => builder.Register(new FuncRegistrationBuilder(factoryFactory, typeof(Func<TParam1, TParam2, T>), lifetime));

        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, TParam3, T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, Func<TParam1, TParam2, TParam3, T>> factoryFactory,
            Lifetime lifetime)
            => builder.Register(new FuncRegistrationBuilder(factoryFactory, typeof(Func<TParam1, TParam2, TParam3, T>), lifetime));

        public static RegistrationBuilder RegisterFactory<TParam1, TParam2, TParam3, TParam4, T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, Func<TParam1, TParam2, TParam3, TParam4, T>> factoryFactory,
            Lifetime lifetime)
            => builder.Register(new FuncRegistrationBuilder(factoryFactory, typeof(Func<TParam1, TParam2, TParam3, TParam4, T>), lifetime));

        [Obsolete("IObjectResolver is registered by default. This method does nothing.")]
        public static void RegisterContainer(this IContainerBuilder builder)
        {
        }
    }
}
