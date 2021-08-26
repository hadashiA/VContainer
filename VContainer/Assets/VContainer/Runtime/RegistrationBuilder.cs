using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    /// <summary>
    /// A fluent interface for configuring the dependencies that will be managed
    /// by an <see cref="IObjectResolver"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You likely won't create these objects explicitly; instead you'll use instances
    /// returned by <see cref="IContainerBuilder"/> and its extension methods.
    /// </para>
    /// <para>
    /// Does not construct dependencies. That occurs later on when they're resolved
    /// after the container is built.
    /// </para>
    /// </remarks>
    /// <seealso cref="IContainerBuilder"/>
    /// <seealso cref="ContainerBuilderExtensions"/>
    public class RegistrationBuilder
    {
        internal readonly Type ImplementationType;
        internal readonly Lifetime Lifetime;

        internal List<Type> InterfaceTypes;
        internal List<IInjectParameter> Parameters;

        /// <summary>
        /// Constructs the registration builder that will in turn be used to
        /// construct a dependency registration.
        /// </summary>
        /// <remarks>
        /// You will not likely need to use this constructor; instead you'll use instances
        /// returned by <see cref="IContainerBuilder"/> and its extension methods.
        /// </remarks>
        /// <param name="implementationType">
        /// The type of the dependency that will be constructed or registered. The
        /// final registration might not necessarily be resolvable with this type,
        /// depending on how this builder is configured.
        /// </param>
        /// <param name="lifetime">
        /// The rules governing how and when this dependency will be instantiated
        /// and cleaned up.
        /// </param>
        /// <seealso cref="IContainerBuilder.Register{T}"/>
        /// <seealso cref="VContainer.Lifetime"/>
        public RegistrationBuilder(Type implementationType, Lifetime lifetime)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

        /// <summary>
        /// Builds the registration that will be used to resolve a dependency
        /// when it's needed later.
        /// </summary>
        /// <returns>
        /// A new <see cref="IRegistration"/> that will be used to populate an <see cref="IContainerBuilder"/>.
        /// </returns>
        public virtual IRegistration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);

            return new Registration(
                ImplementationType,
                Lifetime,
                InterfaceTypes,
                Parameters,
                injector);
        }

        /// <summary>
        /// Enables this dependency to be resolved as a <typeparamref name="TInterface"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Despite the names of the type parameters, <typeparamref name="TInterface"/>
        /// can be an <see langword="interface"/> or a base class (<see langword="abstract"/> or not).
        /// </para>
        /// <para>
        /// If <typeparamref name="TInterface"/> is already registered, this method
        /// has no effect.
        /// </para>
        /// </remarks>
        /// <typeparam name="TInterface">
        /// The type to add to this dependency's registration. Must be an implemented
        /// interface or a base class.
        /// </typeparam>
        /// <returns>Itself. Can be chained as a fluent interface.</returns>
        /// <exception cref="VContainerException">
        /// The implementation type can't be assigned to <typeparamref name="TInterface"/>.
        /// </exception>
        /// <seealso cref="As(System.Type)"/>
        public RegistrationBuilder As<TInterface>()
            => As(typeof(TInterface));

        /// <summary>
        /// Enables this dependency to be resolved as a <typeparamref name="TInterface1"/>
        /// or a <typeparamref name="TInterface2"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Equivalent to <c>.As&lt;TInterface1&gt;().As&lt;TInterface2&gt;()</c>.
        /// </para>
        /// <para>
        /// Despite the names of the type parameters, they can be <see langword="interface"/>s
        /// or base classes (<see langword="abstract"/> or not).
        /// </para>
        /// <para>
        /// Type parameters that are already registered will not be duplicated.
        /// </para>
        /// </remarks>
        /// <typeparam name="TInterface1">
        /// The first type to add to this dependency's registration. Must be an implemented
        /// interface or a base class, but can be unrelated to <typeparamref name="TInterface2"/>.
        /// </typeparam>
        /// <typeparam name="TInterface2">
        /// The second type to add to this dependency's registration. Must be an implemented
        /// interface or a base class, but can be unrelated to <typeparamref name="TInterface1"/>.
        /// </typeparam>
        /// <returns>Itself. Can be chained as a fluent interface.</returns>
        /// <exception cref="VContainerException">
        /// The implementation type can't be assigned to <typeparamref name="TInterface1"/>
        /// or <typeparamref name="TInterface2"/>.
        /// </exception>
        /// <seealso cref="As(System.Type,System.Type)"/>
        public RegistrationBuilder As<TInterface1, TInterface2>()
            => As(typeof(TInterface1), typeof(TInterface2));

        /// <summary>
        /// Enables this dependency to be resolved as a <typeparamref name="TInterface1"/>,
        /// a <typeparamref name="TInterface2"/>, or a <typeparamref name="TInterface3"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Equivalent to <c>.As&lt;TInterface1&gt;().As&lt;TInterface2&gt;().As&lt;TInterface3&gt;()</c>.
        /// </para>
        /// <para>
        /// Despite the names of the type parameters, they can be <see langword="interface"/>s
        /// or base classes (<see langword="abstract"/> or not).
        /// </para>
        /// <para>
        /// Type parameters that are already registered will not be duplicated.
        /// </para>
        /// </remarks>
        /// <typeparam name="TInterface1">
        /// The first type to add to this dependency's registration. Must be an implemented
        /// interface or a base class, but can be unrelated to the other type parameters.
        /// </typeparam>
        /// <typeparam name="TInterface2">
        /// The second type to add to this dependency's registration. Must be an implemented
        /// interface or a base class, but can be unrelated to the other type parameters.
        /// </typeparam>
        /// <typeparam name="TInterface3">
        /// The third type to add to this dependency's registration. Must be an implemented
        /// interface or a base class, but can be unrelated to the other type parameters.
        /// </typeparam>
        /// <returns>Itself. Can be chained as a fluent interface.</returns>
        /// <exception cref="VContainerException">
        /// The implementation type can't be assigned to <typeparamref name="TInterface1"/>,
        /// <typeparamref name="TInterface2"/>, or <typeparamref name="TInterface3"/>.
        /// </exception>
        /// <seealso cref="As(System.Type,System.Type,System.Type)"/>
        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3>()
            => As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));

        /// <summary>
        /// Enables this dependency to be resolved as a <typeparamref name="TInterface1"/>,
        /// a <typeparamref name="TInterface2"/>, a <typeparamref name="TInterface3"/>, or
        /// a <typeparamref name="TInterface4"/>.
        /// </summary>
        /// <remarks>
        /// Despite the names of the type parameters, they can be <see langword="interface"/>s
        /// or base classes (<see langword="abstract"/> or not).
        /// </remarks>
        /// <typeparam name="TInterface1">
        /// The first type to add to this dependency's registration. Must be an implemented
        /// interface or a base class, but can be unrelated to the other type parameters.
        /// </typeparam>
        /// <typeparam name="TInterface2">
        /// The second type to add to this dependency's registration. Must be an implemented
        /// interface or a base class, but can be unrelated to the other type parameters.
        /// </typeparam>
        /// <typeparam name="TInterface3">
        /// The third type to add to this dependency's registration. Must be an implemented
        /// interface or a base class, but can be unrelated to the other type parameters.
        /// </typeparam>
        /// <typeparam name="TInterface4">
        /// The fourth type to add to this dependency's registration. Must be an implemented
        /// interface or a base class, but can be unrelated to the other type parameters.
        /// </typeparam>
        /// <returns>Itself. Can be chained as a fluent interface.</returns>
        /// <exception cref="VContainerException">
        /// The implementation type can't be assigned to any of the given type parameters.
        /// </exception>
        /// <returns>Itself. Can be chained as a fluent interface.</returns>
        /// <seealso cref="As(System.Type[])"/>
        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3, TInterface4>()
            => As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3), typeof(TInterface4));

        /// <summary>
        /// Adds the dependency's implementation type to the registration, allowing
        /// it to be resolved with its own type.
        /// </summary>
        /// <remarks>
        /// If this registration is already resolvable as the implementation type,
        /// then this method has no effect.
        /// </remarks>
        /// <returns>Itself. Can be chained as a fluent interface.</returns>
        public RegistrationBuilder AsSelf()
        {
            AddInterfaceType(ImplementationType);
            return this;
        }

        /// <summary>
        /// Adds all implemented interfaces (but <b>not</b> base classes) to the
        /// registration, allowing this dependency to be resolved with any of its
        /// interfaces.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Be careful if refactoring a class that's registered with this method;
        /// if one of the implementation type's interfaces or base classes is
        /// separately registered as a  <see cref="VContainer.Lifetime.Singleton"/>
        /// dependency, VContainer will throw a <see cref="VContainerException"/>
        /// when building the container.
        /// </para>
        /// <para>
        /// This method does not add duplicate types; types that are already
        /// included in this registration will not be added again.
        /// </para>
        /// <para>
        /// This method does not register the implementation type's base class;
        /// you can do so manually with a call to <see cref="As{TInterface}"/>.
        /// </para>
        /// <para>
        /// This method is fine if you don't want to think too hard about how
        /// this dependency will be resolved, such as when prototyping. If you
        /// need finer control, use one of the other <c>As</c> methods.
        /// </para>
        /// </remarks>
        /// <returns>Itself. Can be chained as a fluent interface.</returns>
        /// <seealso cref="AsSelf"/>
        /// <seealso cref="As{TInterface}"/>
        public RegistrationBuilder AsImplementedInterfaces()
        {
            InterfaceTypes = InterfaceTypes ?? new List<Type>();
            InterfaceTypes.AddRange(ImplementationType.GetInterfaces());
            return this;
        }

        public RegistrationBuilder As(Type interfaceType)
        {
            AddInterfaceType(interfaceType);
            return this;
        }

        public RegistrationBuilder As(Type interfaceType1, Type interfaceType2)
        {
            AddInterfaceType(interfaceType1);
            AddInterfaceType(interfaceType2);
            return this;
        }

        public RegistrationBuilder As(Type interfaceType1, Type interfaceType2, Type interfaceType3)
        {
            AddInterfaceType(interfaceType1);
            AddInterfaceType(interfaceType2);
            AddInterfaceType(interfaceType3);
            return this;
        }

        public RegistrationBuilder As(params Type[] interfaceTypes)
        {
            foreach (var interfaceType in interfaceTypes)
            {
                AddInterfaceType(interfaceType);
            }
            return this;
        }

        public RegistrationBuilder WithParameter(string name, object value)
        {
            Parameters = Parameters ?? new List<IInjectParameter>();
            Parameters.Add(new NamedParameter(name, value));
            return this;
        }

        public RegistrationBuilder WithParameter(Type type, object value)
        {
            Parameters = Parameters ?? new List<IInjectParameter>();
            Parameters.Add(new TypedParameter(type, value));
            return this;
        }

        public RegistrationBuilder WithParameter<TParam>(TParam value)
        {
            return WithParameter(typeof(TParam), value);
        }

        void AddInterfaceType(Type interfaceType)
        {
            if (!interfaceType.IsAssignableFrom(ImplementationType))
            {
                throw new VContainerException(interfaceType, $"{ImplementationType} is not assignable from {interfaceType}");
            }
            InterfaceTypes = InterfaceTypes ?? new List<Type>();
            if (!InterfaceTypes.Contains(interfaceType))
                InterfaceTypes.Add(interfaceType);
        }
   }
}
