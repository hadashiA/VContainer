using System;
using VContainer.Internal;

namespace VContainer.Runtime
{
    public static class ConatinerBuilderDecoratorExtensions
    {
        public static DecoratorRegistrationBuilder RegisterDecorator<TInterface, TDecorator>(this IContainerBuilder builder)
        {
            for (var i = 0; i < builder.Count; i++)
            {
                var interfaceTypes = builder[i].InterfaceTypes;
                if (interfaceTypes != null && interfaceTypes.Contains(typeof(TInterface)))
                {
                    return new DecoratorRegistrationBuilder(builder[i], typeof(TDecorator));
                }
            }
            throw new VContainerException(typeof(TInterface), $"No such decorator target: {typeof(TInterface)}");
        }

        public static DecoratorRegistrationBuilder RegisterDecorator<TInterface, TDecorator>(
            this IContainerBuilder builder,
            Func<TInterface, IObjectResolver, TDecorator> factory)
        {
            for (var i = 0; i < builder.Count; i++)
            {
                var interfaceTypes = builder[i].InterfaceTypes;
                if (interfaceTypes != null && interfaceTypes.Contains(typeof(TInterface)))
                {
                    return new FuncDecoratorRegistrationBuilder<TInterface, TDecorator>(builder[i], factory);
                }
            }
            throw new VContainerException(typeof(TInterface), $"No such decorator target: {typeof(TInterface)}");
        }
    }

    public class DecoratorRegistrationBuilder : RegistrationBuilder
    {
        readonly RegistrationBuilder inner;
        readonly Type interfaceType;

        public DecoratorRegistrationBuilder(RegistrationBuilder inner, Type decoratorType)
            : base(decoratorType, inner.Lifetime)
        {
            this.inner = inner;
            interfaceType = inner.InterfaceTypes != null ? inner.InterfaceTypes[0] : inner.ImplementationType;
            InterfaceTypes = inner.InterfaceTypes;
            As(interfaceType);
        }

        public override Registration Build()
        {
            var injector = InjectorCache.GetOrBuild(ImplementationType);
            var innerRegistration = inner.Build();

            var provider = new FuncInstanceProvider(container =>
            {
                var innerInstance = container.Resolve(innerRegistration);
                var parameters = new IInjectParameter[Parameters == null ? 1 : Parameters.Count];
                Parameters?.CopyTo(parameters);
                parameters[parameters.Length - 1] = new TypedParameter(interfaceType, innerInstance);
                return injector.CreateInstance(container, parameters);
            });
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, provider);
        }
    }

    public class FuncDecoratorRegistrationBuilder<TInner, TDecorator> : DecoratorRegistrationBuilder
    {
        readonly RegistrationBuilder inner;
        readonly Type interfaceType;
        readonly Func<TInner, IObjectResolver, TDecorator> factory;

        public FuncDecoratorRegistrationBuilder(RegistrationBuilder inner, Func<TInner, IObjectResolver, TDecorator> factory)
            : base(inner, typeof(TDecorator))
        {
            this.factory = factory;
        }

        public override Registration Build()
        {
            var innerRegistration = inner.Build();
            var provider = new FuncInstanceProvider(container =>
            {
                var innerInstance = container.Resolve(innerRegistration);
                return factory((TInner)innerInstance, container);
            });
            return new Registration(ImplementationType, Lifetime, InterfaceTypes, provider);
        }
    }
}
