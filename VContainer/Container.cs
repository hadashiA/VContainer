using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public interface IObjectResolver
    {
        object Resolve<T>();
        object Resolve(Type type);
    }

    public interface ILifetimeScope : IDisposable
    {
        ILifetimeScope Parent { get; }

        IScopedObjectResolver BeginScope();
        IScopedObjectResolver BeginScope(Action<ContainerBuilder> configuration);
    }

    public interface IScopedObjectResolver : IObjectResolver, IDisposable
    {
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
    }

    public enum Lifetime
    {
        Transient,
        Singleton,
        Scoped,
    }

    public sealed class Container : IScopedObjectResolver
    {
        readonly IRegistry registry;

        internal Container(IRegistry registry)
        {
            this.registry = registry;
        }
    }

    public sealed class ContainerBuilder
    {
        readonly IList<RegistrationBuilder> registrationBuilders = new List<RegistrationBuilder>();

        public RegistrationBuilder Register<T>(Lifetime lifetime = default)
            => Register(typeof(T), lifetime);

        public RegistrationBuilder Register<TInterface, TImplement>(Lifetime lifetime = default)
            => Register<TImplement>(lifetime).As<TInterface>();

        public RegistrationBuilder Register(Type type, Lifetime lifetime = default)
        {
            var registrationBuilder = new RegistrationBuilder(type, lifetime);
            registrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public RegistrationBuilder RegisterInstance(object instance)
        {
            throw new NotImplementedException();
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
