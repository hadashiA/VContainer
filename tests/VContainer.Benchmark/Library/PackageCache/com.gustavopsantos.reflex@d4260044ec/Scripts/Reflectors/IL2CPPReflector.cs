using System;
using System.Reflection;

namespace Reflex
{
	internal class IL2CPPReflector : Reflector
	{
		internal override DynamicObjectActivator GenerateGenericActivator(Type type, ConstructorInfo constructor, Type[] parameters)
		{
			return args =>
			{
				var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
				constructor.Invoke(instance, args);
				return instance;
			};
		}

		internal override DynamicObjectActivator GenerateDefaultActivator(Type type)
		{
			return args => System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
		}
	}
}