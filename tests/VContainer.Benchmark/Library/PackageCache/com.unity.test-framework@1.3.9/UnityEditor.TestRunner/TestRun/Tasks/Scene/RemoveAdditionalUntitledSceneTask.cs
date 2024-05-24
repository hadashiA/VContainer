using System;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene
{
    internal class RemoveAdditionalUntitledSceneTask : TestTaskBase
    {
        internal Func<int> GetSceneCount = () => SceneManager.sceneCount;
        internal Func<int, ISceneWrapper> GetSceneAt = i => new SceneWrapper(SceneManager.GetSceneAt(i));
        internal Func<ISceneWrapper, bool, bool> CloseScene = (scene, remove) => EditorSceneManager.CloseScene(scene.WrappedScene, remove);
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var sceneCount = GetSceneCount();
            if (sceneCount <= 1)
            {
                yield break;
            }

            for (int i = 0; i < sceneCount; i++)
            {
                var scene = GetSceneAt(i);
                if (string.IsNullOrEmpty(scene.path))
                {
                    CloseScene(scene, true);
                    yield break;
                }
            }
        }
    }
}
