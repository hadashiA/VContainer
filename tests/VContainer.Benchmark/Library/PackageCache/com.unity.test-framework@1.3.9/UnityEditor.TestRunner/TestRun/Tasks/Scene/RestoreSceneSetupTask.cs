using System;
using System.Collections;
using UnityEditor.SceneManagement;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene
{
    internal class RestoreSceneSetupTask : TestTaskBase
    {
        internal Action<SceneSetup[]> RestoreSceneManagerSetup = EditorSceneManager.RestoreSceneManagerSetup;
        internal Func<NewSceneSetup, NewSceneMode, ISceneWrapper> NewScene = (setup, mode) => new SceneWrapper(EditorSceneManager.NewScene(setup, mode));
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var sceneSetup = testJobData.SceneSetup;
            if (sceneSetup != null && sceneSetup.Length > 0)
            {
                RestoreSceneManagerSetup(sceneSetup);
            }
            else
            {
                NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            }

            yield break;
        }
    }
}
