using System;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene
{
    internal class SceneWrapper : ISceneWrapper
    {
        private UnityEngine.SceneManagement.Scene m_WrappedScene;

        public SceneWrapper(UnityEngine.SceneManagement.Scene wrappedScene)
        {
            m_WrappedScene = wrappedScene;
        }

        public UnityEngine.SceneManagement.Scene WrappedScene
        {
            get { return m_WrappedScene; }
        }

        public bool isDirty
        {
            get { return m_WrappedScene.isDirty; }
        }

        public string path
        {
            get { return m_WrappedScene.path; }
        }
    }
}
