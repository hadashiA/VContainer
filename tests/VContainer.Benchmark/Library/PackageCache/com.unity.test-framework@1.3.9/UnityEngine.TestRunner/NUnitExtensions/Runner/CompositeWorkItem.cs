using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using NUnit.Framework.Internal.Execution;
using UnityEngine.TestTools.Logging;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal class CompositeWorkItem : UnityWorkItem
    {
        private readonly TestSuite _suite;
        private readonly TestSuiteResult _suiteResult;
        private readonly ITestFilter _childFilter;
        private TestCommand _setupCommand;
        private TestCommand _teardownCommand;

        public List<UnityWorkItem> Children { get; private set; }

        private int _countOrder;

        private CountdownEvent _childTestCountdown;

        public CompositeWorkItem(TestSuite suite, ITestFilter childFilter, WorkItemFactory factory)
            : base(suite, factory)
        {
            _suite = suite;
            _suiteResult = Result as TestSuiteResult;
            _childFilter = childFilter;
            _countOrder = 0;
        }

        protected override IEnumerable PerformWork()
        {
            InitializeSetUpAndTearDownCommands();

            if (UnityTestExecutionContext.CurrentContext != null && m_DontRunRestoringResult && EditModeTestCallbacks.RestoringTestContext != null)
            {
                EditModeTestCallbacks.RestoringTestContext();
            }

            if (!CheckForCancellation())
                if (Test.RunState == RunState.Explicit && !_childFilter.IsExplicitMatch(Test))
                    SkipFixture(ResultState.Explicit, GetSkipReason(), null);
                else
                    switch (Test.RunState)
                    {
                        default:
                        case RunState.Runnable:
                        case RunState.Explicit:
                            Result.SetResult(ResultState.Success);

                            CreateChildWorkItems();

                            if (Children.Count > 0)
                            {
                                if (!m_DontRunRestoringResult)
                                {
                                    //This is needed to give the editor a chance to go out of playmode if needed before creating objects.
                                    //If we do not, the objects could be automatically destroyed when exiting playmode and could result in errors later on
                                    yield return null;
                                    PerformOneTimeSetUp();
                                }

                                if (!CheckForCancellation())
                                {
                                    switch (Result.ResultState.Status)
                                    {
                                        case TestStatus.Passed:
                                            foreach (var child in RunChildren())
                                            {
                                                if (CheckForCancellation())
                                                {
                                                    yield break;
                                                }

                                                yield return child;
                                            }
                                            break;
                                        case TestStatus.Skipped:
                                        case TestStatus.Inconclusive:
                                        case TestStatus.Failed:
                                            SkipChildren(_suite, Result.ResultState.WithSite(FailureSite.Parent), "OneTimeSetUp: " + Result.Message);
                                            break;
                                    }
                                }

                                if (Context.ExecutionStatus != TestExecutionStatus.AbortRequested && !m_DontRunRestoringResult)
                                {
                                    PerformOneTimeTearDown();
                                }
                            }
                            break;

                        case RunState.Skipped:
                            SkipFixture(ResultState.Skipped, GetSkipReason(), null);
                            break;

                        case RunState.Ignored:
                            SkipFixture(ResultState.Ignored, GetSkipReason(), null);
                            break;

                        case RunState.NotRunnable:
                            SkipFixture(ResultState.NotRunnable, GetSkipReason(), GetProviderStackTrace());
                            break;
                    }
            if (!ResultedInDomainReload)
            {
                WorkItemComplete();
            }
        }

        private bool CheckForCancellation()
        {
            if (Context.ExecutionStatus != TestExecutionStatus.Running)
            {
                Result.SetResult(ResultState.Cancelled, "Test cancelled by user");
                return true;
            }

            return false;
        }

        private void InitializeSetUpAndTearDownCommands()
        {
            List<SetUpTearDownItem> setUpTearDownItems = _suite.TypeInfo != null
                ? CommandBuilder.BuildSetUpTearDownList(_suite.TypeInfo.Type, typeof(OneTimeSetUpAttribute), typeof(OneTimeTearDownAttribute))
                : new List<SetUpTearDownItem>();

            var actionItems = new List<TestActionItem>();
            foreach (ITestAction action in Actions)
            {
                bool applyToSuite = (action.Targets & ActionTargets.Suite) == ActionTargets.Suite
                    || action.Targets == ActionTargets.Default && !(Test is ParameterizedMethodSuite);

                bool applyToTest = (action.Targets & ActionTargets.Test) == ActionTargets.Test
                    && !(Test is ParameterizedMethodSuite);

                if (applyToSuite)
                    actionItems.Add(new TestActionItem(action));

                if (applyToTest)
                    Context.UpstreamActions.Add(action);
            }

            _setupCommand = CommandBuilder.MakeOneTimeSetUpCommand(_suite, setUpTearDownItems, actionItems);
            _teardownCommand = CommandBuilder.MakeOneTimeTearDownCommand(_suite, setUpTearDownItems, actionItems);
        }

        private void PerformOneTimeSetUp()
        {
            var logScope = new LogScope();
            try
            {
                _setupCommand.Execute(Context);
                logScope.EvaluateLogScope(true);
            }
            catch (Exception ex)
            {
                if (ex is NUnitException || ex is TargetInvocationException)
                    ex = ex.InnerException;

                Result.RecordException(ex, FailureSite.SetUp);
            }

            logScope.Dispose();
        }

        private IEnumerable RunChildren()
        {
            int childCount = Children.Count;
            if (childCount == 0)
                throw new InvalidOperationException("RunChildren called but item has no children");

            _childTestCountdown = new CountdownEvent(childCount);

            foreach (UnityWorkItem child in Children)
            {
                if (CheckForCancellation())
                {
                    yield break;
                }

                var unityTestExecutionContext = new UnityTestExecutionContext(Context);
                child.InitializeContext(unityTestExecutionContext);

                var enumerable = child.Execute().GetEnumerator();

                while (true)
                {
                    if (!enumerable.MoveNext())
                    {
                        break;
                    }
                    ResultedInDomainReload |= child.ResultedInDomainReload;
                    yield return enumerable.Current;
                }

                _suiteResult.AddResult(child.Result);
                childCount--;
            }

            if (childCount > 0)
            {
                while (childCount-- > 0)
                    CountDownChildTest();
            }
        }

        private void CreateChildWorkItems()
        {
            Children = new List<UnityWorkItem>();
            var testSuite = _suite;

            foreach (ITest test in testSuite.Tests)
            {
                if (_childFilter.Pass(test))
                {
                    var child = m_Factory.Create(test, _childFilter);

                    if (test.Properties.ContainsKey(PropertyNames.Order))
                    {
                        Children.Insert(0, child);
                        _countOrder++;
                    }
                    else
                    {
                        Children.Add(child);
                    }
                }
            }

            if (_countOrder != 0) SortChildren();
        }

        private class UnityWorkItemOrderComparer : IComparer<UnityWorkItem>
        {
            public int Compare(UnityWorkItem x, UnityWorkItem y)
            {
                var xKey = int.MaxValue;
                var yKey = int.MaxValue;

                if (x.Test.Properties.ContainsKey(PropertyNames.Order))
                    xKey = (int)x.Test.Properties[PropertyNames.Order][0];

                if (y.Test.Properties.ContainsKey(PropertyNames.Order))
                    yKey = (int)y.Test.Properties[PropertyNames.Order][0];

                return xKey.CompareTo(yKey);
            }
        }

        private void SortChildren()
        {
            Children.Sort(0, _countOrder, new UnityWorkItemOrderComparer());
        }

        private void SkipFixture(ResultState resultState, string message, string stackTrace)
        {
            Result.SetResult(resultState.WithSite(FailureSite.SetUp), message, StackFilter.Filter(stackTrace));
            SkipChildren(_suite, resultState.WithSite(FailureSite.Parent), message);
        }

        private void SkipChildren(TestSuite suite, ResultState resultState, string message)
        {
            foreach (Test child in suite.Tests)
            {
                if (_childFilter.Pass(child))
                {
                    Context.Listener.TestStarted(child);
                    TestResult childResult = child.MakeTestResult();
                    childResult.SetResult(resultState, message);
                    _suiteResult.AddResult(childResult);

                    if (child.IsSuite)
                        SkipChildren((TestSuite)child, resultState, message);

                    Context.Listener.TestFinished(childResult);
                }
            }
        }

        private void PerformOneTimeTearDown()
        {
            var logScope = new LogScope();
            try
            {
                _teardownCommand.Execute(Context);
                logScope.EvaluateLogScope(true);
            }
            catch (Exception ex)
            {
                if (ex is NUnitException || ex is TargetInvocationException)
                    ex = ex.InnerException;

                Result.RecordException(ex, FailureSite.SetUp);
            }

            logScope.Dispose();
        }

        private string GetSkipReason()
        {
            return (string)Test.Properties.Get(PropertyNames.SkipReason);
        }

        private string GetProviderStackTrace()
        {
            return (string)Test.Properties.Get(PropertyNames.ProviderStackTrace);
        }

        private void CountDownChildTest()
        {
            _childTestCountdown.Signal();
            if (_childTestCountdown.CurrentCount == 0)
            {
                if (Context.ExecutionStatus != TestExecutionStatus.AbortRequested)
                    PerformOneTimeTearDown();

                foreach (var childResult in _suiteResult.Children)
                    if (childResult.ResultState == ResultState.Cancelled)
                    {
                        Result.SetResult(ResultState.Cancelled, "Cancelled by user");
                        break;
                    }

                WorkItemComplete();
            }
        }

        public override void Cancel(bool force)
        {
            if (Children == null)
                return;

            foreach (var child in Children)
            {
                var ctx = child.Context;
                if (ctx != null)
                    ctx.ExecutionStatus = force ? TestExecutionStatus.AbortRequested : TestExecutionStatus.StopRequested;

                if (child.State == WorkItemState.Running)
                    child.Cancel(force);
            }
        }
    }
}
