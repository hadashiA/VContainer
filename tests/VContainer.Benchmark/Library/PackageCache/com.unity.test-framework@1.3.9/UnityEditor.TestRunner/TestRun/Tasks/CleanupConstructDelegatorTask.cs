using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class CleanupConstructDelegatorTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.ConstructDelegator.DestroyCurrentTestObjectIfExists();
            yield break;
        }
    }
}
