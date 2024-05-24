using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Reflex.Injectors
{
	internal static class MethodInjector
	{
		internal static void Inject(MethodInfo method, object instance, Container container)
		{
			var parameters = method.GetParameters();
			
			try
			{
				var arguments = parameters.Select(p => container.Resolve(p.ParameterType)).ToArray();
				method.Invoke(instance, arguments);
			}
			catch (Exception e)
			{
				var methodDescription = $"'{instance.GetType().Name}::{method.Name}'";
				var parametersDescription = $"'{string.Join(", ", parameters.Select(p => p.ParameterType.Name))}'";
				Debug.LogError($"Could not inject method {methodDescription} with parameters {parametersDescription}\n\n{e}");
			}
		}

		internal static void InjectMany(IEnumerable<MethodInfo> methods, object instance, Container container)
		{
			foreach (var method in methods)
			{
				Inject(method, instance, container);
			}
		}
	}
}