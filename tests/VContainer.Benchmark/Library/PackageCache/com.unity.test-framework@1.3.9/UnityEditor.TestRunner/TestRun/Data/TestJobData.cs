using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    [Serializable]
    internal class TestJobData : ISerializationCallbackReceiver
    {
        [SerializeField]
        public string guid;

        [SerializeField]
        public string startTime;

        [NonSerialized]
        public Stack<TaskInfo> taskInfoStack = new Stack<TaskInfo>();

        [SerializeField] 
        public int taskPC;

        [SerializeField]
        public bool isRunning;

        [SerializeField]
        public ExecutionSettings executionSettings;

        [SerializeField]
        public RunProgress runProgress = new RunProgress();

        [SerializeField]
        public string[] existingFiles;

        [SerializeField]
        public int undoGroup = -1;

        [SerializeField]
        public EditModeRunner editModeRunner;

        [SerializeField]
        public BeforeAfterTestCommandState setUpTearDownState;

        [SerializeField]
        public BeforeAfterTestCommandState outerUnityTestActionState;
        
        [SerializeField]
        public TestRunnerStateSerializer testRunnerStateSerializer;
        
        [SerializeField]
        public EnumerableTestState enumerableTestState;
        
        [SerializeField]
        private TaskInfo[] savedTaskInfoStack;

        [NonSerialized]
        public bool isHandledByRunner;

        [SerializeField]
        public SceneSetup[] SceneSetup;
        [NonSerialized]
        public TestTaskBase[] Tasks;

        [SerializeField] 
        public TestProgress testProgress;
        
        public ITest testTree;
        
        [NonSerialized]
        public TestStartedEvent TestStartedEvent;
        [NonSerialized]
        public TestFinishedEvent TestFinishedEvent;
        [NonSerialized]
        public RunStartedEvent RunStartedEvent;
        [NonSerialized]
        public RunFinishedEvent RunFinishedEvent;

        [NonSerialized]
        public UnityTestExecutionContext Context;

        [NonSerialized]
        public ConstructDelegator ConstructDelegator;
        
        [NonSerialized]
        public ITestResult TestResults;
        
        [SerializeField]
        public EnumerableTestState RetryRepeatState;

        public TestJobData(ExecutionSettings settings)
        {
            guid = Guid.NewGuid().ToString();
            executionSettings = settings;
            isRunning = false;
            startTime = DateTime.Now.ToString("o");
        }

        public void OnBeforeSerialize()
        {
            savedTaskInfoStack = taskInfoStack.ToArray();
        }

        public void OnAfterDeserialize()
        {
            taskInfoStack = new Stack<TaskInfo>(savedTaskInfoStack);
        }

    }
}
