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

        public RegistrationBuilder Register(Type implementationType, Lifetime lifetime)
        {
            var registrationBuilder = new RegistrationBuilder(implementationType, lifetime);
            RegistrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }

        public RegistrationBuilder RegisterInstance(object instance)
        {
            var registrationBuilder = new RegistrationBuilder(instance);
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
