using System;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene
{
    internal interface ISceneWrapper
    {
        UnityEngine.SceneManagement.Scene WrappedScene { get; }
        bool isDirty { get; }
        string path { get; }
    }
}
