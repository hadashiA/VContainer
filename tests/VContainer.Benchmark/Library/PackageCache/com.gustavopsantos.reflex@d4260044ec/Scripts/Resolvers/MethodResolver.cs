using System;

namespace Reflex
{
    internal class MethodResolver : Resolver
    {
        internal override object Resolve(Type contract, Container container)
        {
            if (container.TryGetMethod(contract, out var method))
            {
                return method();
            }
            
            throw new UnknownMethodException($"Cannot resolve method of type '{contract}'.");
        }
    }
}