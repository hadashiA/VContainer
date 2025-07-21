using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class UnlockReloadAssembliesTask : TestTaskBase
    {
        internal Action UnlockReloadAssemblies = EditorApplication.UnlockReloadAssemblies;
        public UnlockReloadAssembliesTask()
        {
            RunOnError = ErrorRunMode.RunAlways;
            RunOnCancel = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            UnlockReloadAssemblies();
            yield break;
        }
    }
}
