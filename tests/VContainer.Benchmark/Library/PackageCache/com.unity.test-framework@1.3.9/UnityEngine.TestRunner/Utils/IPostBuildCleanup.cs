using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// Implement this interface if you want to define a set of actions to execute as a post-build step. Cleanup runs right away for a standalone test run, but only after all the tests run within the Editor.
    /// </summary>
    public interface IPostBuildCleanup
    {
        /// <summary>
        /// Implement this method to specify actions that should run as a post-build cleanup step.
        /// </summary>
        void Cleanup();
    }
}
