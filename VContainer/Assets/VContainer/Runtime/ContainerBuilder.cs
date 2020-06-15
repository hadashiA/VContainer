using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public interface IContainerBuilder
    {
        RegistrationBuilder Register<T>(Lifetime lifetime);
        RegistrationBuilder RegisterInstance<T>(T instance);
        IObjectResolver Build();
    }

    public sealed class ScopedContainerBuilder : ContainerBuilder
    {
        readonly IObjectResolver root;
        readonly IScopedObjectResolver parent;

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
            foreach (var x in RegistrationBuilders)
            {
                registry.Add(x.Build());
            }
            return new ScopedContainer(registry, root, parent);
        }

        public override IObjectResolver Build() => BuildScope();
    }

    public class ContainerBuilder : IContainerBuilder
    {
        protected readonly IList<RegistrationBuilder> RegistrationBuilders = new List<RegistrationBuilder>();

        public RegistrationBuilder Register<T>(Lifetime lifetime)
        {
            var registrationBuilder = new RegistrationBuilder(typeof(T), lifetime);
            RegistrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public RegistrationBuilder RegisterInstance<T>(T instance)
        {
            var registrationBuilder = new RegistrationBuilder(typeof(T), instance);
            RegistrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public virtual IObjectResolver Build()
        {
            var registry = new HashTableRegistry();
            foreach (var x in RegistrationBuilders)
            {
                registry.Add(x.Build());
            }
            return new Container(registry);
        }
    }
}
