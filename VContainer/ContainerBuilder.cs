using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public interface IContainerBuilder
    {
        RegistrationBuilder Register(Type interfaceType, Lifetime lifetime);
        IObjectResolver Build();
    }

    public static class ContainerBuilderExtensions
    {
        public static RegistrationBuilder Register<T>(this IContainerBuilder builder, Lifetime lifetime)
            => builder.Register(typeof(T), lifetime);

        public static RegistrationBuilder Register<TInterface, TImplement>(this IContainerBuilder builder, Lifetime lifetime)
            where TImplement : TInterface
            => builder.Register<TImplement>(lifetime).As<TInterface>();
    }

    public sealed class ContainerBuilder : IContainerBuilder
    {
        readonly IList<RegistrationBuilder> registrationBuilders = new List<RegistrationBuilder>();

        public RegistrationBuilder Register(Type type, Lifetime lifetime)
        {
            var registrationBuilder = new RegistrationBuilder(type, lifetime);
            registrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public IObjectResolver Build()
        {
            var registry = new HashTableRegistry();

            foreach (var x in registrationBuilders)
            {
                var registration = x.Build();
                if (registration.InterfaceTypes?.Count > 0)
                {
                    foreach (var contractType in registration.InterfaceTypes)
                    {
                        registry.Add(contractType, registration);
                    }
                }
                else
                {
                    registry.Add(registration.ImplementationType, registration);
                }
            }

            return new Container(registry);
        }
    }
}