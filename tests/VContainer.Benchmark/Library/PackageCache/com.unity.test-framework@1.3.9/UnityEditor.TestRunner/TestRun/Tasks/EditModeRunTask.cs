using System;
using System.Collections;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class EditModeRunTask : TestTaskBase
    {
        public EditModeRunTask()
        {
            SupportsResumingEnumerator = true;
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.taskInfoStack.Peek().taskMode == TaskMode.Canceled)
            {
                var runner = testJobData.editModeRunner;
                if (runner != null)
                {
                    runner.OnRunCancel();
                }
                yield break;
            }
            else if (testJobData.taskInfoStack.Peek().taskMode == TaskMode.Resume)
            {
                var runner = testJobData.editModeRunner;
                if (runner == null)
                {
                    yield break;
                }

                runner.Resume(testJobData.executionSettings.BuildNUnitFilter(), testJobData.testTree, testJobData.TestStartedEvent, testJobData.TestFinishedEvent, testJobData.Context);
                yield break;
            }

            var editModeRunner = ScriptableObject.CreateInstance<EditModeRunner>();
            testJobData.editModeRunner = editModeRunner;

            editModeRunner.UnityTestAssemblyRunnerFactory = new UnityTestAssemblyRunnerFactory();
            editModeRunner.Init(testJobData.executionSettings.BuildNUnitFilter(), testJobData.executionSettings.runSynchronously, testJobData.testTree, testJobData.TestStartedEvent,
                testJobData.TestFinishedEvent, testJobData.Context, testJobData.executionSettings.orderedTestNames, testJobData.executionSettings.randomOrderSeed);

            while (testJobData.editModeRunner != null && !testJobData.editModeRunner.RunFinished)
            {
                testJobData.editModeRunner.TestConsumer(testJobData.testRunnerStateSerializer);
                yield return null;
            }

            testJobData.TestResults = testJobData.editModeRunner.m_Runner.Result;
            testJobData.editModeRunner.Dispose();
        }
    }
}
