using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public class RegistrationBuilder
    {
        protected readonly Type ImplementationType;
        protected readonly Lifetime Lifetime;
        protected readonly object SpecificInstance;

        protected List<Type> InterfaceTypes;
        protected List<IInjectParameter> Parameters;

        internal RegistrationBuilder(Type implementationType, Lifetime lifetime, List<Type> interfaceTypes = null)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;
        }

        internal RegistrationBuilder(object instance)
        {
            ImplementationType = instance.GetType();
            Lifetime = Lifetime.Scoped;
            SpecificInstance = instance;
        }

        public virtual IRegistration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);

            return new Registration(
                ImplementationType,
                Lifetime,
                InterfaceTypes,
                Parameters,
                injector,
                SpecificInstance);
        }

        public RegistrationBuilder As<TInterface>()
            => As(typeof(TInterface));

        public RegistrationBuilder As<TInterface1, TInterface2>()
            => As(typeof(TInterface1), typeof(TInterface2));

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3>()
            => As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3, TInterface4>()
            => As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3), typeof(TInterface4));

        public RegistrationBuilder AsSelf()
        {
            InterfaceTypes = InterfaceTypes ?? new List<Type>();
            InterfaceTypes.Add(ImplementationType);
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
            InterfaceTypes = InterfaceTypes ?? new List<Type>();
            InterfaceTypes.AddRange(interfaceTypes);
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

        public RegistrationBuilder WithParameter<TParam>(object value)
        {
            return WithParameter(typeof(TParam), value);
        }

        void AddInterfaceType(Type interfaceType)
        {
            if (!interfaceType.IsAssignableFrom(ImplementationType))
            {
                throw new VContainerException(interfaceType, $"{ImplementationType.FullName} is not assignable from {interfaceType.FullName}");
            }
            InterfaceTypes = InterfaceTypes ?? new List<Type>();
            InterfaceTypes.Add(interfaceType);
        }
   }
}
