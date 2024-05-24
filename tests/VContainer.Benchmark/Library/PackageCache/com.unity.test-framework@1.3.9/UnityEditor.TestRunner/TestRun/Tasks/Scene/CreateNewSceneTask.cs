using System;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene
{
    internal class CreateNewSceneTask : TestTaskBase
    {
        internal Func<int> GetSceneCount = () => SceneManager.sceneCount;
        internal Func<int, ISceneWrapper> GetSceneAt = i => new SceneWrapper(SceneManager.GetSceneAt(i));
        internal Func<NewSceneSetup, NewSceneMode, ISceneWrapper> NewScene = (setup, mode) => new SceneWrapper(EditorSceneManager.NewScene(setup, mode));
        internal Action<ISceneWrapper> SetActiveScene = scene => SceneManager.SetActiveScene(scene.WrappedScene);
        
        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (GetSceneCount() == 1 && string.IsNullOrEmpty(GetSceneAt(0).path))
            {
                NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                yield break;
            }

            var scene = NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SetActiveScene(scene);
        }
    }
}
