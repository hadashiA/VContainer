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

        readonly IInjector injector;
        readonly object specificInstance;

        internal Registration(
            Type implementationType,
            IReadOnlyList<Type> interfaceTypes,
            Lifetime lifetime,
            IInjector injector,
            object specificInstance = null)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;

            this.injector = injector;
            this.specificInstance = specificInstance;
        }

        public override string ToString() => $"ConcreteType={ImplementationType.Name} ContractTypes={string.Join(", ", InterfaceTypes)} {Lifetime} {injector.GetType().Name}";

        public object SpawnInstance(IObjectResolver resolver)
        {
            if (specificInstance != null)
            {
                injector.Inject(specificInstance, resolver);
                return specificInstance;
            }
            return injector.CreateInstance(resolver);
        }
    }

    public class RegistrationBuilder
    {
        readonly Type implementationType;
        List<Type> interfaceTypes;
        Lifetime lifetime;
        readonly object specificInstance;

        public RegistrationBuilder(Type implementationType, Lifetime lifetime, List<Type> interfaceTypes = null)
        {
            this.implementationType = implementationType;
            this.interfaceTypes = interfaceTypes;
            this.lifetime = lifetime;
        }

        public RegistrationBuilder(object instance, List<Type> interfaceTypes = null)
        {
            implementationType = instance.GetType();
            this.interfaceTypes = interfaceTypes;
            lifetime = Lifetime.Singleton;
            specificInstance = instance;
        }

        public virtual Registration Build()
        {
            var injector = ReflectionInjectorBuilder.Default.Build(implementationType); // TODO:

            return new Registration(
                implementationType,
                interfaceTypes,
                lifetime,
                injector,
                specificInstance);
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
