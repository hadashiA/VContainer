using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner
{
    internal interface IUnityTestAssemblyRunnerFactory
    {
        IUnityTestAssemblyRunner Create(TestPlatform testPlatform, string[] orderedTestNames, int randomOrderSeed, WorkItemFactory factory, UnityTestExecutionContext context);
    }

    internal class UnityTestAssemblyRunnerFactory : IUnityTestAssemblyRunnerFactory
    {
        public IUnityTestAssemblyRunner Create(TestPlatform testPlatform, string[] orderedTestNames, int randomOrderSeed,
            WorkItemFactory factory, UnityTestExecutionContext context)
        {
            return new UnityTestAssemblyRunner(new UnityTestAssemblyBuilder(orderedTestNames, randomOrderSeed), factory, context);
        }
    }

    [Serializable]
    internal class EditModeRunner : ScriptableObject, IDisposable
    {
        //The counter from the IEnumerator object
        [SerializeField]
        private int m_CurrentPC;

        [SerializeField]
        private bool m_ExecuteOnEnable;

        [SerializeField]
        private List<string> m_AlreadyStartedTests;

        [SerializeField]
        private List<TestResultSerializer> m_ExecutedTests;

        private TestStartedEvent m_TestStartedEvent;
        private TestFinishedEvent m_TestFinishedEvent;

        [SerializeField]
        private object m_CurrentYieldObject;

        [SerializeField]
        private string[] m_OrderedTestNames;

        [SerializeField]
        public bool RunFinished;

        public bool RunningSynchronously { get; private set; }

        internal IUnityTestAssemblyRunner m_Runner;

        private IEnumerator m_RunStep;

        public IUnityTestAssemblyRunnerFactory UnityTestAssemblyRunnerFactory { get; set; }

        public void Init(ITestFilter filter, bool runningSynchronously, ITest testTree, TestStartedEvent testStartedEvent, TestFinishedEvent testFinishedEvent, UnityTestExecutionContext context,
            string[] orderedTestNames, int randomOrderSeed)
        {
            TestEnumerator.Reset();
            m_AlreadyStartedTests = new List<string>();
            m_ExecutedTests = new List<TestResultSerializer>();
            m_OrderedTestNames = orderedTestNames;
            m_randomOrderSeed = randomOrderSeed;
            RunningSynchronously = runningSynchronously;
            Run(testTree, filter, context, testStartedEvent, testFinishedEvent);
        }

        public void Resume(ITestFilter filter, ITest testTree, TestStartedEvent testStartedEvent, TestFinishedEvent testFinishedEvent, UnityTestExecutionContext context)
        {
            if (m_ExecuteOnEnable)
            {
                m_ExecuteOnEnable = false;

                if (m_CurrentPC >= 0)
                {
                    EnumeratorStepHelper.SetEnumeratorPC(m_CurrentPC);
                }

                UnityWorkItemDataHolder.alreadyExecutedTests = m_ExecutedTests.Select(x => x.uniqueName).ToList();
                UnityWorkItemDataHolder.alreadyStartedTests = m_AlreadyStartedTests;
                Run(testTree, filter, context, testStartedEvent, testFinishedEvent);
            }
        }

        public void TestStartedEvent(ITest test)
        {
            m_AlreadyStartedTests.Add(test.GetUniqueName());
        }

        public void TestFinishedEvent(ITestResult testResult)
        {
            m_AlreadyStartedTests.Remove(testResult.Test.GetUniqueName());
            m_ExecutedTests.Add(TestResultSerializer.MakeFromTestResult(testResult));
        }

        private void Run(ITest testTree, ITestFilter filter, UnityTestExecutionContext context, TestStartedEvent testStartedEvent, TestFinishedEvent testFinishedEvent)
        {
            m_TestStartedEvent = testStartedEvent;
            m_TestFinishedEvent = testFinishedEvent;

            m_Runner = (UnityTestAssemblyRunnerFactory ?? new UnityTestAssemblyRunnerFactory()).Create(TestPlatform.EditMode, m_OrderedTestNames, m_randomOrderSeed, new EditmodeWorkItemFactory(), context);
            m_Runner.LoadTestTree(testTree);
            hideFlags |= HideFlags.DontSave;
            EnumerableSetUpTearDownCommand.ActivePcHelper = new EditModePcHelper();
            OuterUnityTestActionCommand.ActivePcHelper = new EditModePcHelper();

            EditModeTestCallbacks.RestoringTestContext += OnRestoringTest;

            m_TestStartedEvent.AddListener(TestStartedEvent);
            m_TestFinishedEvent.AddListener(TestFinishedEvent);

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            RunningTests = true;

            EditorApplication.LockReloadAssemblies();

            var testListenerWrapper = new TestListenerWrapper(m_TestStartedEvent, m_TestFinishedEvent);
            m_RunStep = m_Runner.Run(testListenerWrapper, filter).GetEnumerator();
        }

        private void OnBeforeAssemblyReload()
        {
            if (m_ExecuteOnEnable)
            {
                AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
                return;
            }

            if (m_Runner != null && m_Runner.TopLevelWorkItem != null)
                m_Runner.TopLevelWorkItem.ResultedInDomainReload = true;

            if (RunningTests)
            {
                Debug.LogError("TestRunner: Unexpected assembly reload happened while running tests");

                EditorUtility.ClearProgressBar();

                if (m_Runner.GetCurrentContext() != null && m_Runner.GetCurrentContext().CurrentResult != null)
                {
                    m_Runner.GetCurrentContext().CurrentResult.SetResult(ResultState.Cancelled, "Unexpected assembly reload happened");
                }
                OnRunCancel();
            }
        }

        private bool RunningTests;

        private Stack<IEnumerator> StepStack = new Stack<IEnumerator>();
        private int m_randomOrderSeed;

        private bool MoveNextAndUpdateYieldObject()
        {
            var result = m_RunStep.MoveNext();

            if (result)
            {
                m_CurrentYieldObject = m_RunStep.Current;
                while (m_CurrentYieldObject is IEnumerator)    // going deeper
                {
                    var currentEnumerator = (IEnumerator)m_CurrentYieldObject;

                    // go deeper and add parent to stack
                    StepStack.Push(m_RunStep);

                    m_RunStep = currentEnumerator;
                    m_CurrentYieldObject = m_RunStep.Current;
                }

                if (StepStack.Count > 0 && m_CurrentYieldObject != null)    // not null and not IEnumerator, nested
                {
                    Debug.LogError("EditMode test can only yield null, but not <" + m_CurrentYieldObject.GetType().Name + ">");
                }

                return true;
            }

            if (StepStack.Count == 0)       // done
                return false;

            m_RunStep = StepStack.Pop();    // going up
            return MoveNextAndUpdateYieldObject();
        }

        public void TestConsumer(TestRunnerStateSerializer testRunnerStateSerializer)
        {
            var moveNext = MoveNextAndUpdateYieldObject();

            if (m_CurrentYieldObject != null)
            {
                InvokeDelegator(testRunnerStateSerializer);
            }

            if (!moveNext && !m_Runner.IsTestComplete)
            {
                CompleteTestRun();
                throw new IndexOutOfRangeException("There are no more elements to process and IsTestComplete is false");
            }

            if (m_Runner.IsTestComplete)
            {
                CompleteTestRun();
            }
        }

        private void CompleteTestRun()
        {
            RunFinished = true;
            UnityWorkItemDataHolder.alreadyExecutedTests = null;
        }

        private void OnRestoringTest()
        {
            var item = m_ExecutedTests.Find(t => t.fullName == UnityTestExecutionContext.CurrentContext.CurrentTest.FullName);
            if (item != null)
            {
                item.RestoreTestResult(UnityTestExecutionContext.CurrentContext.CurrentResult);
            }
        }

        private static bool IsCancelled()
        {
            return UnityTestExecutionContext.CurrentContext.ExecutionStatus == TestExecutionStatus.AbortRequested || UnityTestExecutionContext.CurrentContext.ExecutionStatus == TestExecutionStatus.StopRequested;
        }

        private void InvokeDelegator(TestRunnerStateSerializer testRunnerStateSerializer)
        {
            if (m_CurrentYieldObject == null)
            {
                return;
            }

            if (IsCancelled())
            {
                return;
            }

            if (m_CurrentYieldObject is RestoreTestContextAfterDomainReload)
            {
                if (testRunnerStateSerializer.ShouldRestore())
                {
                    testRunnerStateSerializer.RestoreContext();
                }

                return;
            }

            try
            {
                if (m_CurrentYieldObject is IEditModeTestYieldInstruction)
                {
                    var editModeTestYieldInstruction = (IEditModeTestYieldInstruction)m_CurrentYieldObject;
                    if (editModeTestYieldInstruction.ExpectDomainReload)
                    {
                        PrepareForDomainReload(testRunnerStateSerializer);
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                UnityTestExecutionContext.CurrentContext.CurrentResult.RecordException(e);
                return;
            }

            UnityTestExecutionContext.CurrentContext.CurrentResult.RecordException(new InvalidOperationException("EditMode test can only yield null"));
        }

        private void CompilationFailureWatch()
        {
            if (EditorApplication.isCompiling)
                return;

            EditorApplication.update -= CompilationFailureWatch;

            if (EditorUtility.scriptCompilationFailed)
            {
                EditorUtility.ClearProgressBar();
                OnRunCancel();
            }
        }

        private void PrepareForDomainReload(TestRunnerStateSerializer testRunnerStateSerializer)
        {
            testRunnerStateSerializer.SaveContext();
            m_CurrentPC = EnumeratorStepHelper.GetEnumeratorPC(TestEnumerator.Enumerator);
            m_ExecuteOnEnable = true;

            RunningTests = false;
        }

        public void Dispose()
        {
            Reflect.MethodCallWrapper = null;

            DestroyImmediate(this);

            RunningTests = false;
            EditorApplication.UnlockReloadAssemblies();
        }

        public void OnRunCancel()
        {
            UnityWorkItemDataHolder.alreadyExecutedTests = null;
            m_ExecuteOnEnable = false;
            m_Runner.StopRun();
            RunFinished = true;
        }
    }
}
