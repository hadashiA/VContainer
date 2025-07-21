#if !NOT_UNITY3D

using System;
using ModestTree;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zenject
{
    public enum LoadSceneRelationship
    {
        // This will use the ProjectContext container as parent for the new scene
        // This is similar to just running the new scene normally
        None,
        // This will use current scene as parent for the new scene
        // This will allow the new scene to refer to dependencies in the current scene
        Child,
        // This will use the parent of the current scene as the parent for the next scene
        // In most cases this will be the same as None
        Sibling
    }

    public class ZenjectSceneLoader
    {
        readonly ProjectKernel _projectKernel;
        readonly DiContainer _sceneContainer;

        public ZenjectSceneLoader(
            [InjectOptional]
            SceneContext sceneRoot,
            ProjectKernel projectKernel)
        {
            _projectKernel = projectKernel;
            _sceneContainer = sceneRoot == null ? null : sceneRoot.Container;
        }

        public void LoadScene(
            string sceneName,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            Action<DiContainer> extraBindings = null,
            LoadSceneRelationship containerMode = LoadSceneRelationship.None,
            Action<DiContainer> extraBindingsLate = null)
        {
            PrepareForLoadScene(loadMode, extraBindings, extraBindingsLate, containerMode);

            Assert.That(Application.CanStreamedLevelBeLoaded(sceneName),
                "Unable to load scene '{0}'", sceneName);

            SceneManager.LoadScene(sceneName, loadMode);

            // It would be nice here to actually verify that the new scene has a SceneContext
            // if we have extra binding hooks, or LoadSceneRelationship != None, but
            // we can't do that in this case since the scene isn't loaded until the next frame
        }

            public AsyncOperation LoadSceneAsync(
            string sceneName,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            Action<DiContainer> extraBindings = null,
            LoadSceneRelationship containerMode = LoadSceneRelationship.None,
            Action<DiContainer> extraBindingsLate = null)
        {
            PrepareForLoadScene(loadMode, extraBindings, extraBindingsLate, containerMode);

            Assert.That(Application.CanStreamedLevelBeLoaded(sceneName),
                "Unable to load scene '{0}'", sceneName);

            return SceneManager.LoadSceneAsync(sceneName, loadMode);
        }

        void PrepareForLoadScene(
            LoadSceneMode loadMode,
            Action<DiContainer> extraBindings,
            Action<DiContainer> extraBindingsLate,
            LoadSceneRelationship containerMode)
        {
            if (loadMode == LoadSceneMode.Single)
            {
                Assert.IsEqual(containerMode, LoadSceneRelationship.None);

                // Here we explicitly unload all existing scenes rather than relying on Unity to
                // do this for us.  The reason we do this is to ensure a deterministic destruction
                // order for everything in the scene and in the container.
                // See comment at ProjectKernel.OnApplicationQuit for more details
                _projectKernel.ForceUnloadAllScenes();
            }

            if (containerMode == LoadSceneRelationship.None)
            {
                SceneContext.ParentContainers = null;
            }
            else if (containerMode == LoadSceneRelationship.Child)
            {
                if (_sceneContainer == null)
                {
                    SceneContext.ParentContainers = null;
                }
                else
                {
                    SceneContext.ParentContainers = new[] { _sceneContainer };
                }
            }
            else
            {
                Assert.IsNotNull(_sceneContainer,
                    "Cannot use LoadSceneRelationship.Sibling when loading scenes from ProjectContext");
                Assert.IsEqual(containerMode, LoadSceneRelationship.Sibling);
                SceneContext.ParentContainers = _sceneContainer.ParentContainers;
            }

            SceneContext.ExtraBindingsInstallMethod = extraBindings;
            SceneContext.ExtraBindingsLateInstallMethod = extraBindingsLate;
        }

        public void LoadScene(
            int sceneIndex,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            Action<DiContainer> extraBindings = null,
            LoadSceneRelationship containerMode = LoadSceneRelationship.None,
            Action<DiContainer> extraBindingsLate = null)
        {
            PrepareForLoadScene(loadMode, extraBindings, extraBindingsLate, containerMode);

            Assert.That(Application.CanStreamedLevelBeLoaded(sceneIndex),
                "Unable to load scene '{0}'", sceneIndex);

            SceneManager.LoadScene(sceneIndex, loadMode);

            // It would be nice here to actually verify that the new scene has a SceneContext
            // if we have extra binding hooks, or LoadSceneRelationship != None, but
            // we can't do that in this case since the scene isn't loaded until the next frame
        }

        public AsyncOperation LoadSceneAsync(
            int sceneIndex,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            Action<DiContainer> extraBindings = null,
            LoadSceneRelationship containerMode = LoadSceneRelationship.None,
            Action<DiContainer> extraBindingsLate = null)
        {
            PrepareForLoadScene(loadMode, extraBindings, extraBindingsLate, containerMode);

            Assert.That(Application.CanStreamedLevelBeLoaded(sceneIndex),
                "Unable to load scene '{0}'", sceneIndex);

            return SceneManager.LoadSceneAsync(sceneIndex, loadMode);
        }
    }
}

#endif
