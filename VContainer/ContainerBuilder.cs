using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public interface IContainerBuilder
    {
        RegistrationBuilder Register(Type interfaceType, Lifetime lifetime);
        RegistrationBuilder RegisterInstance(object instance);
        IObjectResolver Build();
    }

    public static class ContainerBuilderExtensions
    {
        public static RegistrationBuilder Register<T>(this IContainerBuilder builder, Lifetime lifetime)
            => builder.Register(typeof(T), lifetime);

        public static RegistrationBuilder Register<TInterface, TImplement>(this IContainerBuilder builder, Lifetime lifetime)
            where TImplement : TInterface
            => builder.Register<TImplement>(lifetime).As<TInterface>();

        public static RegistrationBuilder Register<TInterface1, TInterface2, TImplement>(this IContainerBuilder builder, Lifetime lifetime)
            where TImplement : TInterface1, TInterface2
            => builder.Register<TImplement>(lifetime).As<TInterface1, TInterface2>();

        public static RegistrationBuilder Register<TInterface1, TInterface2, TInterface3, TImplement>(this IContainerBuilder builder, Lifetime lifetime)
            where TImplement : TInterface1, TInterface2, TInterface3
            => builder.Register<TImplement>(lifetime).As<TInterface1, TInterface2, TInterface3>();

        public static RegistrationBuilder RegisterInstance<TInterface>(this IContainerBuilder builder, object instance)
            => builder.RegisterInstance(instance).As<TInterface>();

        public static RegistrationBuilder RegisterInstance<TInterface1, TInterface2>(this IContainerBuilder builder, object instance)
            => builder.RegisterInstance(instance).As<TInterface1, TInterface2>();

        public static RegistrationBuilder RegisterInstance<TInterface1, TInterface2, TInterface3>(this IContainerBuilder builder, object instance)
            => builder.RegisterInstance(instance).As<TInterface1, TInterface2, TInterface3>();
    }

    public sealed class ScopedContainerBuilder : ContainerBuilder
    {
        readonly IObjectResolver root;
        readonly IScopedObjectResolver parent;
        readonly IList<RegistrationBuilder> registrationBuilders = new List<RegistrationBuilder>();

        internal ScopedContainerBuilder(
            IObjectResolver root,
            IScopedObjectResolver parent)
        {
            this.root = root;
            this.parent = parent;
        }

        public IScopedObjectResolver BuildScope()
        {
            var registry = new HashTableRegistry();
            foreach (var x in registrationBuilders)
            {
                registry.Add(x.Build());
            }
            return new ScopedContainer(registry, root, parent);
        }

        public override IObjectResolver Build() => BuildScope();
    }

    public class ContainerBuilder : IContainerBuilder
    {
        readonly IList<RegistrationBuilder> registrationBuilders = new List<RegistrationBuilder>();

        public RegistrationBuilder Register(Type implementationType, Lifetime lifetime)
        {
            var registrationBuilder = new RegistrationBuilder(implementationType, lifetime);
            registrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public RegistrationBuilder RegisterInstance(object instance)
        {
            var registrationBuilder = new RegistrationBuilder(instance);
            registrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public virtual IObjectResolver Build()
        {
            var registry = new HashTableRegistry();
            foreach (var x in registrationBuilders)
            {
                registry.Add(x.Build());
            }
            return new Container(registry);
        }
    }
}
