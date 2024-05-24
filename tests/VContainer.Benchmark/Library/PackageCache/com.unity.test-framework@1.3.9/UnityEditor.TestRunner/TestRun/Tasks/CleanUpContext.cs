using System;
using System.Collections;
using NUnit.Framework;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class CleanUpContext : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.Context = null;
            TestContext.CurrentTestExecutionContext = null;
            yield break;
        }
    }
}
