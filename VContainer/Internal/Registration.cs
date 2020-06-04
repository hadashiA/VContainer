using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    public interface IInjector
    {
        object CreateInstance(IObjectResolver resolver);
    }

    interface IInjectorBuilder
    {
        IInjector Build(Type type);
    }

    sealed class ReflectionInjector : IInjector
    {
        readonly InjectTypeInfo injectTypeInfo;

        public ReflectionInjector(InjectTypeInfo injectTypeInfo)
        {
            this.injectTypeInfo = injectTypeInfo;
        }

        public object CreateInstance(IObjectResolver resolver)
        {
            var parameters = injectTypeInfo.InjectConstructor.GetParameters();
            var parameterValues = new object[parameters.Length]; // TODO: pooling
            for (var i = 0; i < parameters.Length; i++)
            {
                parameterValues[i] = resolver.Resolve(parameters[i].ParameterType);
            }

            return injectTypeInfo.InjectConstructor.Invoke(parameterValues);
        }
    }

    sealed class ReflectionInjectorBuilder : IInjectorBuilder
    {
        public static ReflectionInjectorBuilder Default = new ReflectionInjectorBuilder();

        public IInjector Build(Type type)
        {
            var injectTypeInfo = TypeAnalyzer.Analyze(type);
            return new ReflectionInjector(injectTypeInfo);
        }
    }

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

    public sealed class RegistrationBuilder
    {
        readonly Type concreteType;
        List<Type> contractTypes;
        Lifetime lifetime = Lifetime.Transient;

        public RegistrationBuilder(Type concreteType, Lifetime lifetime = default)
        {
            this.concreteType = concreteType;
            this.lifetime = lifetime;
        }

        public Registration Build()
        {
            var injector = ReflectionInjectorBuilder.Default.Build(concreteType); // TODO:

            return new Registration(
                concreteType,
                contractTypes,
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

        public RegistrationBuilder As<TInterfaceType>()
        {
            if (contractTypes == null)
                contractTypes = new List<Type>();
            contractTypes.Add(typeof(TInterfaceType));
            return this;
        }

        public RegistrationBuilder AsSelf()
        {
            if (contractTypes == null)
                contractTypes = new List<Type>();
            contractTypes.Add(concreteType);
            return this;
        }
    }
}
