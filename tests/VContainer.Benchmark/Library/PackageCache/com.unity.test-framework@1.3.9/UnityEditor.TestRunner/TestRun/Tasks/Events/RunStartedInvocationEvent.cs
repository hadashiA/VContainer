using System;
using System.Collections;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Events
{
    internal class RunStartedInvocationEvent : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.testTree == null)
            {
                throw new Exception("TestTree must be set before run started event.");
            }
            testJobData.RunStartedEvent.Invoke(testJobData.testTree);
            yield break;
        }
    }
}
