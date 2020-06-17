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

        public RegistrationBuilder(Type implementationType, Lifetime lifetime, List<Type> interfaceTypes = null)
        {
            this.implementationType = implementationType;
            this.interfaceTypes = interfaceTypes;
            this.lifetime = lifetime;
        }

        public RegistrationBuilder(Type implementationType, object instance)
        {
            this.implementationType = implementationType;
            lifetime = Lifetime.Scoped;
            specificInstance = instance;
        }

        public Registration Build()
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
            if (!typeof(TInterface).IsAssignableFrom(implementationType))
            {
                throw new VContainerException($"{implementationType.FullName} is not assignable from {typeof(TInterface)}");
            }
            interfaceTypes = interfaceTypes ?? new List<Type>();
            interfaceTypes.Add(typeof(TInterface));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2>()
        {
            // TODO: Check at compilation time
            if (!typeof(TInterface1).IsAssignableFrom(implementationType))
            {
                throw new VContainerException($"{implementationType.FullName} is not assignable from {typeof(TInterface1)}");
            }
            if (!typeof(TInterface2).IsAssignableFrom(implementationType))
            {
                throw new VContainerException($"{implementationType.FullName} is not assignable from {typeof(TInterface2)}");
            }

            interfaceTypes = interfaceTypes ?? new List<Type>();
            interfaceTypes.Add(typeof(TInterface1));
            interfaceTypes.Add(typeof(TInterface2));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3>()
        {
            if (!typeof(TInterface1).IsAssignableFrom(implementationType))
            {
                throw new VContainerException($"{implementationType.FullName} is not assignable from {typeof(TInterface1)}");
            }
            if (!typeof(TInterface2).IsAssignableFrom(implementationType))
            {
                throw new VContainerException($"{implementationType.FullName} is not assignable from {typeof(TInterface2)}");
            }
            if (!typeof(TInterface3).IsAssignableFrom(implementationType))
            {
                throw new VContainerException($"{implementationType.FullName} is not assignable from {typeof(TInterface3)}");
            }

            interfaceTypes = interfaceTypes ?? new List<Type>();
            interfaceTypes.Add(typeof(TInterface1));
            interfaceTypes.Add(typeof(TInterface2));
            interfaceTypes.Add(typeof(TInterface3));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3, TInterface4>()
        {
            if (!typeof(TInterface1).IsAssignableFrom(implementationType))
            {
                throw new VContainerException($"{implementationType.FullName} is not assignable from {typeof(TInterface1)}");
            }
            if (!typeof(TInterface2).IsAssignableFrom(implementationType))
            {
                throw new VContainerException($"{implementationType.FullName} is not assignable from {typeof(TInterface2)}");
            }
            if (!typeof(TInterface3).IsAssignableFrom(implementationType))
            {
                throw new VContainerException($"{implementationType.FullName} is not assignable from {typeof(TInterface3)}");
            }
            if (!typeof(TInterface4).IsAssignableFrom(implementationType))
            {
                throw new VContainerException($"{implementationType.FullName} is not assignable from {typeof(TInterface4)}");
            }
            interfaceTypes = interfaceTypes ?? new List<Type>();
            interfaceTypes.Add(typeof(TInterface1));
            interfaceTypes.Add(typeof(TInterface2));
            interfaceTypes.Add(typeof(TInterface3));
            interfaceTypes.Add(typeof(TInterface4));
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
            interfaceTypes.AddRange(implementationType.GetInterfaces());
            return this;
        }
   }
}