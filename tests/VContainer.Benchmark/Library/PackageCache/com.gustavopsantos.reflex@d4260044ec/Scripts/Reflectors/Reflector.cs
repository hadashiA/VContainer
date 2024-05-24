using System;
using System.Reflection;

namespace Reflex
{
	internal abstract class Reflector
	{
		internal abstract DynamicObjectActivator GenerateDefaultActivator(Type type);
		internal abstract DynamicObjectActivator GenerateGenericActivator(Type type, ConstructorInfo constructor, Type[] parameters);
	}
}