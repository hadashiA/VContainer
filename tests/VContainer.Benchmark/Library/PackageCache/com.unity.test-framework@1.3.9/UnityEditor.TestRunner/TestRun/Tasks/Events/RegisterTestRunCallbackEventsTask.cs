using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestRunner.Utils;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Events
{
    internal class RegisterTestRunCallbackEventsTask : TestTaskBase
    {
        public RegisterTestRunCallbackEventsTask()
        {
            RerunAfterResume = true;
        }

        internal Func<TestRunCallbackListener> GetListener = () => ScriptableObject.CreateInstance<TestRunCallbackListener>();
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var listener = GetListener();
            testJobData.RunStartedEvent.AddListener(v => listener.RunStarted(v));
            testJobData.TestStartedEvent.AddListener(v => listener.TestStarted(v));
            testJobData.TestFinishedEvent.AddListener(v => listener.TestFinished(v));
            testJobData.RunFinishedEvent.AddListener(v => listener.RunFinished(v));
            yield break;
        }
    }
}
