using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// PostBuildCleanup attributes run if the respective test or test class is in the current test run. The test is included either by running all tests or setting a [filter](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/workflow-create-test.html#filters) that includes the test. If multiple tests reference the same pre-built setup or post-build cleanup, then it only runs once.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public class PostBuildCleanupAttribute : Attribute
    {
        /// <summary>
        /// Initializes and returns an instance of PostBuildCleanupAttribute by type.
        /// </summary>
        /// <param name="targetClass">The type of the target class.</param>
        public PostBuildCleanupAttribute(Type targetClass)
        {
            TargetClass = targetClass;
        }

        /// <summary>
        /// Initializes and returns an instance of PostBuildCleanupAttribute by class name.
        /// </summary>
        /// <param name="targetClassName">The name of the target class.</param>
        public PostBuildCleanupAttribute(string targetClassName)
        {
            TargetClass = AttributeHelper.GetTargetClassFromName(targetClassName, typeof(IPostBuildCleanup));
        }

        internal Type TargetClass { get; private set; }
    }
}
