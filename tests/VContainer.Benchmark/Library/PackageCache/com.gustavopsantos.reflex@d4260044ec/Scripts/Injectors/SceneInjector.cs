using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Reflex.Injectors
{
	internal static class SceneInjector
	{
		internal static void Inject(Scene scene, Container container)
		{
			foreach (var monoBehaviour in GetEveryMonoBehaviourAtScene(scene))
			{
				MonoInjector.Inject(monoBehaviour, container);
			}
		}

		private static IEnumerable<MonoBehaviour> GetEveryMonoBehaviourAtScene(Scene scene)
		{
			return scene.GetRootGameObjects().SelectMany(gameObject => gameObject.GetComponentsInChildren<MonoBehaviour>(true));
		}
	}
}