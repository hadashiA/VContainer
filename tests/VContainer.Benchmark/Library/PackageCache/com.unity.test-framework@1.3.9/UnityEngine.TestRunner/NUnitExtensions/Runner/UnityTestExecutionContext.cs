using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;
using UnityEngine.TestTools;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal class UnityTestExecutionContext : ITestExecutionContext
    {
        private readonly UnityTestExecutionContext _priorContext;
        private TestResult _currentResult;
        private int _assertCount;

        public static UnityTestExecutionContext CurrentContext { get; set; }

        public UnityTestExecutionContext Context { get; private set; }

        public bool Automated { get; set; }
        
        public Test CurrentTest { get; set; }
        public DateTime StartTime { get; set; }
        public long StartTicks { get; set; }
        public TestResult CurrentResult
        {
            get { return _currentResult; }
            set
            {
                _currentResult = value;
                if (value != null)
                    OutWriter = value.OutWriter;
            }
        }

        public object TestObject { get; set; }
        public string WorkDirectory { get; set; }


        private TestExecutionStatus _executionStatus;
        public TestExecutionStatus ExecutionStatus
        {
            get
            {
                // ExecutionStatus may have been set to StopRequested or AbortRequested
                // in a prior context. If so, reflect the same setting in this context.
                if (_executionStatus == TestExecutionStatus.Running && _priorContext != null)
                    _executionStatus = _priorContext.ExecutionStatus;

                return _executionStatus;
            }
            set
            {
                _executionStatus = value;

                // Push the same setting up to all prior contexts
                if (_priorContext != null)
                    _priorContext.ExecutionStatus = value;
            }
        }

        public List<ITestAction> UpstreamActions { get; private set; }
        public int TestCaseTimeout { get; set; }
        public CultureInfo CurrentCulture { get; set; }
        public CultureInfo CurrentUICulture { get; set; }
        public ITestListener Listener { get; set; }

        public UnityTestExecutionContext()
        {
            UpstreamActions = new List<ITestAction>();
            SetUpTearDownState = new BeforeAfterTestCommandState();
            OuterUnityTestActionState = new BeforeAfterTestCommandState();
            EnumerableTestState = new EnumerableTestState();
        }

        public UnityTestExecutionContext(BeforeAfterTestCommandState setUpTearDownState,
            BeforeAfterTestCommandState outerUnityTestActionState, EnumerableTestState enumerableTestState) : this()
        {
            SetUpTearDownState = setUpTearDownState;
            OuterUnityTestActionState = outerUnityTestActionState;
            EnumerableTestState = enumerableTestState;
        }

        public UnityTestExecutionContext(UnityTestExecutionContext other)
        {
            _priorContext = other;

            CurrentTest = other.CurrentTest;
            CurrentResult = other.CurrentResult;
            TestObject = other.TestObject;
            WorkDirectory = other.WorkDirectory;
            Listener = other.Listener;
            TestCaseTimeout = other.TestCaseTimeout;
            UpstreamActions = new List<ITestAction>(other.UpstreamActions);
            SetUpTearDownState = other.SetUpTearDownState;
            OuterUnityTestActionState = other.OuterUnityTestActionState;
            EnumerableTestState = other.EnumerableTestState;

            TestContext.CurrentTestExecutionContext = this;

            CurrentCulture = other.CurrentCulture;
            CurrentUICulture = other.CurrentUICulture;
            TestMode = other.TestMode;
            IgnoreTests = other.IgnoreTests;
            FeatureFlags = other.FeatureFlags;
            CurrentContext = this;
            Automated = other.Automated;
            RepeatCount = other.RepeatCount;
            RetryCount = other.RetryCount;
        }

        public TextWriter OutWriter { get; private set; }
        public bool StopOnError { get; set; }

        public IWorkItemDispatcher Dispatcher { get; set; }

        public ParallelScope ParallelScope { get; set; }
        public string WorkerId { get; private set; }
        public Randomizer RandomGenerator { get; private set; }
        public ValueFormatter CurrentValueFormatter { get; private set; }
        public bool IsSingleThreaded { get; set; }
        public BeforeAfterTestCommandState SetUpTearDownState { get; set; }
        public BeforeAfterTestCommandState OuterUnityTestActionState { get; set; }
        public EnumerableTestState EnumerableTestState { get; set; }
        public IgnoreTest[] IgnoreTests { get; set; }
        public FeatureFlags FeatureFlags { get; set; }
        public int RetryCount { get; set; }
        public int RepeatCount { get; set; }
        public EnumerableTestState RetryRepeatState { get; set; }

        internal int AssertCount
        {
            get
            {
                return _assertCount;
            }
        }

        public TestPlatform TestMode { get; set; }

        public void IncrementAssertCount()
        {
            _assertCount += 1;
        }

        public void AddFormatter(ValueFormatterFactory formatterFactory)
        {
            throw new NotImplementedException();
        }
    }
}
