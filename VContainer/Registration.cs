using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public sealed class Registration
    {
        public readonly Type ImplementationType;
        public readonly IReadOnlyList<Type> InterfaceTypes;
        public readonly Lifetime Lifetime;
        public readonly IInjector Injector;

        public Registration(
            Type implementationType,
            IReadOnlyList<Type> interfaceTypes,
            Lifetime lifetime,
            IInjector injector)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;
            Injector = injector;
        }

        public override string ToString() => $"ConcreteType={ImplementationType.Name} ContractTypes={string.Join(", ", InterfaceTypes)} {Lifetime} {Injector.GetType().Name}";
    }

    public class RegistrationBuilder
    {
        readonly Type implementationType;
        List<Type> interfaceTypes;
        Lifetime lifetime;

        public RegistrationBuilder(Type implementationType, Lifetime lifetime = default, List<Type> interfaceTypes = null)
        {
            this.implementationType = implementationType;
            this.lifetime = lifetime;
            this.interfaceTypes = interfaceTypes;
        }

        public Registration Build()
        {
            var injector = ReflectionInjectorBuilder.Default.Build(implementationType); // TODO:

            return new Registration(
                implementationType,
                interfaceTypes,
                lifetime,
                injector);
        }

        public RegistrationBuilder SingleInstance()
        {
            lifetime = Lifetime.Singleton;
            return this;
        }

        public RegistrationBuilder InstancePerDependency()
        {
            lifetime = Lifetime.Transient;
            return this;
        }

        public RegistrationBuilder InstancePerLifetimeScope()
        {
            lifetime = Lifetime.Scoped;
            return this;
        }

        public RegistrationBuilder As<TInterface>()
        {
            (interfaceTypes ?? (interfaceTypes = new List<Type>())).Add(typeof(TInterface));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2>()
        {
            var list = (interfaceTypes ?? (interfaceTypes = new List<Type>()));
            list.Add(typeof(TInterface1));
            list.Add(typeof(TInterface2));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3>()
        {
            var list = (interfaceTypes ?? (interfaceTypes = new List<Type>()));
            list.Add(typeof(TInterface1));
            list.Add(typeof(TInterface2));
            list.Add(typeof(TInterface3));
            return this;
        }

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3, TInterface4>()
        {
            var list = (interfaceTypes ?? (interfaceTypes = new List<Type>()));
            list.Add(typeof(TInterface1));
            list.Add(typeof(TInterface2));
            list.Add(typeof(TInterface3));
            list.Add(typeof(TInterface4));
            return this;
        }

        public RegistrationBuilder AsSelf()
        {
            (interfaceTypes ?? (interfaceTypes = new List<Type>())).Add(implementationType);
            return this;
        }
    }
}
