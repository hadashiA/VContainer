using System;

namespace Reflex
{
	internal class SingletonNonLazyResolver : Resolver
	{
		internal override object Resolve(Type contract, Container container)
		{
			return container.Singletons[contract];
		}
	}
}