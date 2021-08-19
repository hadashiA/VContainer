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

        public RegistrationBuilder(Type implementationType, Lifetime lifetime)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

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

        /// <remarks>
        /// Despite the names of the type parameters, <typeparamref name="TInterface"/>
        /// can be an <see langword="interface"/> or a base class (<see langword="abstract"/> or not).
        /// </remarks>
        /// <returns>Itself.</returns>
        /// <exception cref="VContainerException">
        /// The implementation type can't be assigned to <typeparamref name="TInterface"/>.
        /// </exception>
        /// <seealso cref="As(System.Type)"/>
        public RegistrationBuilder As<TInterface>()
            => As(typeof(TInterface));

        /// <remarks>
        /// Despite the names of the type parameters, they can be <see langword="interface"/>s
        /// or base classes (<see langword="abstract"/> or not).
        /// </remarks>
        /// <returns>Itself.</returns>
        /// <exception cref="VContainerException">
        /// The implementation type can't be assigned to <typeparamref name="TInterface1"/>
        /// or <typeparamref name="TInterface2"/>.
        /// </exception>
        /// <seealso cref="As(System.Type,System.Type)"/>
        public RegistrationBuilder As<TInterface1, TInterface2>()
            => As(typeof(TInterface1), typeof(TInterface2));

        /// <remarks>
        /// Despite the names of the type parameters, they can be <see langword="interface"/>s
        /// or base classes (<see langword="abstract"/> or not).
        /// </remarks>
        /// <returns>Itself.</returns>
        /// <exception cref="VContainerException">
        /// The implementation type can't be assigned to <typeparamref name="TInterface1"/>,
        /// <typeparamref name="TInterface2"/>, or <typeparamref name="TInterface3"/>.
        /// </exception>
        /// <seealso cref="As(System.Type,System.Type,System.Type)"/>
        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3>()
            => As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3, TInterface4>()
            => As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3), typeof(TInterface4));

        public RegistrationBuilder AsSelf()
        {
            AddInterfaceType(ImplementationType);
            return this;
        }

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
