using System;
using System.Collections;
using NUnit.Framework.Internal;
using UnityEngine.TestTools.NUnitExtensions;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class SetupConstructDelegatorTask : TestTaskBase
    {
        internal Func<TestRunnerStateSerializer, ConstructDelegator> CreateConstructDelegator =
            stateSerializer => new ConstructDelegator(stateSerializer);

        internal Action<Func<Type, object[], object>> SetConstructorCallWrapper =
            func => Reflect.ConstructorCallWrapper = func;

        public SetupConstructDelegatorTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            var taskInfo = testJobData.taskInfoStack.Peek();
            if (taskInfo.taskMode == TaskMode.Normal)
            {
                testJobData.testRunnerStateSerializer = new TestRunnerStateSerializer();
            }

            testJobData.ConstructDelegator = CreateConstructDelegator(testJobData.testRunnerStateSerializer);
            SetConstructorCallWrapper(testJobData.ConstructDelegator.Delegate);
            yield break;
        }
    }
}
