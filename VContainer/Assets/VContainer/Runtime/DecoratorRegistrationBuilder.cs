using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer.Runtime
{
    public static class ConatinerBuilderDecoratorExtensions
    {
        public static DecoratorRegistrationBuilder<TInner, TDecorator> RegisterDecorator<TInner, TDecorator>(this IContainerBuilder builder)
        {
            for (var i = builder.Count - 1; i >= 0; i--)
            {
                var interfaceTypes = builder[i].InterfaceTypes;
                if (interfaceTypes != null && interfaceTypes.Contains(typeof(TInner)))
                {
                    var decoratorBuilder = new DecoratorRegistrationBuilder<TInner, TDecorator>(builder[i]);
                    builder.Register(decoratorBuilder);
                    return decoratorBuilder;
                }
            }
            throw new VContainerException(typeof(TInner), $"No such decorator target: {typeof(TInner)}");
        }

        public static FuncDecoratorRegistrationBuilder<TInner, TDecorator> RegisterDecorator<TInner, TDecorator>(
            this IContainerBuilder builder,
            Func<TInner, IObjectResolver, TDecorator> factory)
        {
            for (var i = builder.Count - 1; i >= 0; i--)
            {
                var interfaceTypes = builder[i].InterfaceTypes;
                if (interfaceTypes != null && interfaceTypes.Contains(typeof(TInner)))
                {
                    var decoratorBuilder = new FuncDecoratorRegistrationBuilder<TInner, TDecorator>(builder[i], factory);
                    builder.Register(decoratorBuilder);
                    return decoratorBuilder;
                }
            }
            throw new VContainerException(typeof(TInner), $"No such decorator target: {typeof(TInner)}");
        }
    }

    public class DecoratorRegistrationBuilder<TInner, TDecorator> : RegistrationBuilder
    {
        readonly RegistrationBuilder inner;

        public DecoratorRegistrationBuilder(RegistrationBuilder inner)
            : base(typeof(TDecorator), inner.Lifetime)
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
            var innerRegistration = inner.Build();

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

    public class FuncDecoratorRegistrationBuilder<TInner, TDecorator> : RegistrationBuilder
    {
        readonly RegistrationBuilder inner;
        readonly Func<TInner, IObjectResolver, TDecorator> factory;

        public FuncDecoratorRegistrationBuilder(RegistrationBuilder inner, Func<TInner, IObjectResolver, TDecorator> factory)
            : base(typeof(TDecorator), inner.Lifetime)
        {
            this.inner = inner;
            this.factory = factory;
            
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
