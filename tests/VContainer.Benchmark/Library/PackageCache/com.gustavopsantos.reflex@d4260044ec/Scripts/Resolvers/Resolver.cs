using System;

namespace Reflex
{
	internal abstract class Resolver
	{
		internal abstract object Resolve(Type contract, Container container);
	}
}