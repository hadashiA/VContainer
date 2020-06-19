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

        internal RegistrationBuilder(Type implementationType, Lifetime lifetime, List<Type> interfaceTypes = null)
        {
            this.ImplementationType = implementationType;
            this.InterfaceTypes = interfaceTypes;
            this.Lifetime = lifetime;
        }

        internal RegistrationBuilder(object instance)
        {
            ImplementationType = instance.GetType();
            Lifetime = Lifetime.Scoped;
            SpecificInstance = instance;
        }

        public virtual IRegistration Build()
        {
            var injector = ReflectionInjectorBuilder.Default.Build(ImplementationType);

            return new Registration(
                ImplementationType,
                InterfaceTypes,
                Lifetime,
                injector,
                SpecificInstance);
        }

        public RegistrationBuilder As<TInterface>()
        {
            // TODO: Check at compilation time
            AddInterfaceType(typeof(TInterface));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2>()
        {
            // TODO: Check at compilation time
            AddInterfaceType(typeof(TInterface1));
            AddInterfaceType(typeof(TInterface2));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3>()
        {
            // TODO: Check at compilation time
            AddInterfaceType(typeof(TInterface1));
            AddInterfaceType(typeof(TInterface2));
            AddInterfaceType(typeof(TInterface3));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3, TInterface4>()
        {
            // TODO: Check at compilation time
            AddInterfaceType(typeof(TInterface1));
            AddInterfaceType(typeof(TInterface2));
            AddInterfaceType(typeof(TInterface3));
            AddInterfaceType(typeof(TInterface4));
            return this;
        }

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
