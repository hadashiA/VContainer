using System;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene
{
    internal class ReloadModifiedScenesTask : TestTaskBase
    {
        internal Func<int> GetSceneCount = () => SceneManager.sceneCount;
        internal Func<int, ISceneWrapper> GetSceneAt = i => new SceneWrapper(SceneManager.GetSceneAt(i));
        internal Func<ISceneWrapper, bool> ReloadScene = scene => EditorSceneManager.ReloadScene(scene.WrappedScene);

        public override IEnumerator Execute(TestJobData testJobData)
        {
            for (var i = 0; i < GetSceneCount(); i++)
            {
                var scene = GetSceneAt(i);
                var isSceneSaved = !string.IsNullOrEmpty(scene.path);
                var isSceneDirty = scene.isDirty;
                if (isSceneSaved && isSceneDirty)
                {
                    ReloadScene(scene);
                }
            }

            yield break;
        }
    }
}
