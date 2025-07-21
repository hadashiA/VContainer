using System;
using Reflex.Injectors;

namespace Reflex
{
    internal class TransientResolver : Resolver
    {
        internal override object Resolve(Type contract, Container container)
        {
            return ConstructorInjector.ConstructAndInject(container.GetConcreteTypeFor(contract), container);
        }
    }
}