using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer.Runtime
{
    public static class ContainerBuilderDecoratorExtensions
    {
        public static RegistrationBuilder RegisterDecorator<TInner, TDecorator>(this IContainerBuilder builder)
        {
            var innerRegistrationBuilder = FindInnerRegistration(builder, typeof(TInner));
            var decoratorBuilder = new DecoratorRegistrationBuilder<TInner>(innerRegistrationBuilder, typeof(TDecorator));
            builder.Register(decoratorBuilder);
            return decoratorBuilder;
        }

        public static RegistrationBuilder RegisterDecorator<TInner, TDecorator>(
            this IContainerBuilder builder,
            Func<TInner, IObjectResolver, TDecorator> factory)
        {
            var innerRegistrationBuilder = FindInnerRegistration(builder, typeof(TInner));
            var decoratorBuilder = new FuncDecoratorRegistrationBuilder<TInner,TDecorator>(innerRegistrationBuilder, factory);
            builder.Register(decoratorBuilder);
            return decoratorBuilder;
        }

        private static IInnerRegistrationProvider FindInnerRegistration(IContainerBuilder builder, Type innerType)
        {
            for (var i = builder.Count - 1; i >= 0; i--)
            {
                var interfaceTypes = builder[i].InterfaceTypes;
                if (interfaceTypes != null && interfaceTypes.Contains(innerType))
                    return new BuilderInnerRegistrationProvider(builder[i]);
            }
            
            if (builder is ScopedContainerBuilder scopedBuilder)
            {
                var registration = scopedBuilder.parent.FindRegistration(innerType);
                return new CachedInnerRegistrationProvider(registration);
            }
            
            throw new VContainerException(innerType, $"No such decorator target: {innerType}");
        }
    }

    interface IInnerRegistrationProvider
    {
        Type ImplementationType { get; }
        Lifetime Lifetime { get; }
        IReadOnlyList<Type> InterfaceTypes { get; }

        Registration Get();
    }

    sealed class CachedInnerRegistrationProvider : IInnerRegistrationProvider
    {
        readonly Registration registration;
        
        public Type ImplementationType => registration.ImplementationType;
        public Lifetime Lifetime => registration.Lifetime;
        public IReadOnlyList<Type> InterfaceTypes => registration.InterfaceTypes;

        public CachedInnerRegistrationProvider(Registration registration)
        {
            this.registration = registration;
        }

        public Registration Get() => 
            registration;
    }

    sealed class BuilderInnerRegistrationProvider : IInnerRegistrationProvider
    {
        readonly RegistrationBuilder builder;
        
        public Type ImplementationType => builder.ImplementationType;
        public Lifetime Lifetime => builder.Lifetime;
        public IReadOnlyList<Type> InterfaceTypes => builder.InterfaceTypes;

        public BuilderInnerRegistrationProvider(RegistrationBuilder builder)
        {
            this.builder = builder;
        }
        
        public Registration Get() => 
            builder.Build();
    }

    class DecoratorRegistrationBuilder<TInner> : RegistrationBuilder
    {
        protected readonly IInnerRegistrationProvider inner;
        
        public DecoratorRegistrationBuilder(IInnerRegistrationProvider inner, Type implementationType) 
            : base(implementationType, inner.Lifetime)
        {
            this.inner = inner;
            
            InterfaceTypes = new List<Type>();
            for (int i = 0; i < inner.InterfaceTypes.Count; i++)
            {
                if (inner.InterfaceTypes[i].IsAssignableFrom(typeof(TInner)))
                    InterfaceTypes.Add(inner.InterfaceTypes[i]);
            }
            
            As<TInner>();
        }
        
        public override Registration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);
            var innerRegistration = inner.Get();
            
            var provider = new FuncInstanceProvider(container =>
            {
                var innerInstance = container.Resolve(innerRegistration);
                var parameters = new IInjectParameter[Parameters == null ? 1 : Parameters.Count];
                Parameters?.CopyTo(parameters);
                parameters[parameters.Length - 1] = new TypedParameter(typeof(TInner), innerInstance);
                return injector.CreateInstance(container, parameters);
            });
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, provider);
        }
    }

    class FuncDecoratorRegistrationBuilder<TInner, TDecorator> : DecoratorRegistrationBuilder<TInner>
    {
        readonly Func<TInner, IObjectResolver, TDecorator> factory;
        
        public FuncDecoratorRegistrationBuilder(IInnerRegistrationProvider inner, Func<TInner, IObjectResolver, TDecorator> factory)
            : base(inner, typeof(TDecorator))
        {
            this.factory = factory;
        }

        public override Registration Build()
        {
            var innerRegistration = inner.Get();
            var provider = new FuncInstanceProvider(container =>
            {
                var innerInstance = container.Resolve(innerRegistration);
                return factory((TInner)innerInstance, container);
            });
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, provider);
        }
    }
}
