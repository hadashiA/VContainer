using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public sealed class RegistrationBuilder
    {
        readonly Type implementationType;
        readonly Lifetime lifetime;
        readonly object specificInstance;
        List<Type> interfaceTypes;

        internal RegistrationBuilder(Type implementationType, Lifetime lifetime, List<Type> interfaceTypes = null)
        {
            this.implementationType = implementationType;
            this.interfaceTypes = interfaceTypes;
            this.lifetime = lifetime;
        }

        internal RegistrationBuilder(object instance)
        {
            implementationType = instance.GetType();
            lifetime = Lifetime.Scoped;
            specificInstance = instance;
        }

        public IRegistration Build()
        {
            var injector = ReflectionInjectorBuilder.Default.Build(implementationType, specificInstance != null);

            return new Registration(
                implementationType,
                interfaceTypes,
                lifetime,
                injector,
                specificInstance);
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
            interfaceTypes = interfaceTypes ?? new List<Type>();
            interfaceTypes.Add(implementationType);
            return this;
        }

        public RegistrationBuilder AsImplementedInterfaces()
        {
            interfaceTypes = interfaceTypes ?? new List<Type>();
            interfaceTypes.AddRange(implementationType.GetInterfaces());
            return this;
        }

        void AddInterfaceType(Type interfaceType)
        {
            if (!interfaceType.IsAssignableFrom(implementationType))
            {
                throw new VContainerException(interfaceType, $"{implementationType.FullName} is not assignable from {interfaceType.FullName}");
            }
            interfaceTypes = interfaceTypes ?? new List<Type>();
            interfaceTypes.Add(interfaceType);
        }
   }
}
