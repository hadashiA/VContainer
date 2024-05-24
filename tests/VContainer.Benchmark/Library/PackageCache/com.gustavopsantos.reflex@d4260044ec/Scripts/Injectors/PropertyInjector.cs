using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Reflex.Injectors
{
	internal static class PropertyInjector
	{
		internal static void Inject(PropertyInfo property, object instance, Container container)
		{
			try
			{
				property.SetValue(instance, container.Resolve(property.PropertyType));
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		internal static void InjectMany(IEnumerable<PropertyInfo> properties, object instance, Container container)
		{
			foreach (var property in properties)
			{
				Inject(property, instance, container);
			}
		}
	}
}