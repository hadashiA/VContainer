using System;
using System.Linq;
using Reflex.Scripts.Utilities;

namespace Reflex
{
	internal static class BakeTypeInfo
	{
		internal static TypeInfo Bake(Type type)
		{
			// Should we add this complexity to be able to inject strings?
			if (type == typeof(string))
			{
				return new TypeInfo(Type.EmptyTypes, args => null);
			}

			if (type.TryGetConstructors(out var constructors))
			{
				var constructor = constructors.MaxBy(ctor => ctor.GetParameters().Length);
				var parameters = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
				return new TypeInfo(parameters, PlatformReflector.Current.GenerateGenericActivator(type, constructor, parameters));
			}

			// Should we add this complexity yo be able to inject value types?
			return new TypeInfo(Type.EmptyTypes, PlatformReflector.Current.GenerateDefaultActivator(type));
		}
	}
}