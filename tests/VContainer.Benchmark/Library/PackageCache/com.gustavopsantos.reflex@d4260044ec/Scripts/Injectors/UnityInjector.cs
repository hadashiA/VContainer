using UnityEngine;
using Reflex.Scripts;
using UnityEngine.SceneManagement;

namespace Reflex.Injectors
{
	internal class UnityInjector : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void AfterAssembliesLoaded()
		{
			if (TryGetProjectContext(out var projectContext))
			{
				projectContext.InstallBindings();
				projectContext.Container.InstantiateNonLazySingletons();
				SceneManager.sceneLoaded += (scene, mode) => SceneInjector.Inject(scene, projectContext.Container);
			}
		}

		private static bool TryGetProjectContext(out ProjectContext projectContext)
		{
			projectContext = Resources.Load<ProjectContext>("ProjectContext");
			ValidateProjectContext(projectContext);
			return projectContext != null;
		}

		private static void ValidateProjectContext(ProjectContext context)
		{
			if (context == null)
			{
				Debug.LogWarning($"Skipping {nameof(UnityInjector)}. A project context prefab named '{nameof(ProjectContext)}' should exist inside a Resources folder.");
			}
		}
	}
}