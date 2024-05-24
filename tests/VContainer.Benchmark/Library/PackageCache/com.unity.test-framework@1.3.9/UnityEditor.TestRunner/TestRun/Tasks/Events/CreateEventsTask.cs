using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Events
{
    internal class CreateEventsTask : TestTaskBase
    {
        public CreateEventsTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.RunStartedEvent = new RunStartedEvent();
            testJobData.TestStartedEvent = new TestStartedEvent();
            testJobData.TestFinishedEvent = new TestFinishedEvent();
            testJobData.RunFinishedEvent = new RunFinishedEvent();

            if (PlaymodeTestsController.ActiveController != null)
            {
                var controller = PlaymodeTestsController.ActiveController;
                controller.runStartedEvent.AddListener(testJobData.RunStartedEvent.Invoke);
                controller.testStartedEvent.AddListener(testJobData.TestStartedEvent.Invoke);
                controller.testFinishedEvent.AddListener(testJobData.TestFinishedEvent.Invoke);
                controller.runFinishedEvent.AddListener(testJobData.RunFinishedEvent.Invoke);
            }

            yield break;
        }
    }
}
