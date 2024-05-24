using UnityEngine;

namespace Reflex.Injectors
{
	internal static class MonoInjector
	{
		internal static void Inject(MonoBehaviour monoBehaviour, Container container)
		{
			var info = MonoInjectableRepository.Repository.Fetch(monoBehaviour);
			FieldInjector.InjectMany(info.InjectableFields, monoBehaviour, container);
			PropertyInjector.InjectMany(info.InjectableProperties, monoBehaviour, container);
			MethodInjector.InjectMany(info.InjectableMethods, monoBehaviour, container);
		}
	}
}