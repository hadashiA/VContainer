using System;
using System.Reflection;
using System.Text;
using NUnit.Framework.Interfaces;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    [Serializable]
    internal class BeforeAfterTestCommandState
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        public int NextBeforeStepIndex;
        public int NextBeforeStepPc;
        public int NextAfterStepIndex;
        public int NextAfterStepPc;
        public bool TestHasRun;
        public TestStatus CurrentTestResultStatus;
        public string CurrentTestResultLabel;
        public FailureSite CurrentTestResultSite;
        public string CurrentTestMessage;
        public string CurrentTestStrackTrace;
        public bool TestAfterStarted;
        public string Output;
        public long StartTicks;
        public double StartTimeOA;
        public bool ShouldRestore;

        public void Reset()
        {
            NextBeforeStepIndex = 0;
            NextBeforeStepPc = 0;
            NextAfterStepIndex = 0;
            NextAfterStepPc = 0;
            TestHasRun = false;
            CurrentTestResultStatus = TestStatus.Inconclusive;
            CurrentTestResultLabel = null;
            CurrentTestResultSite = default(FailureSite);
            CurrentTestMessage = null;
            CurrentTestStrackTrace = null;
            TestAfterStarted = false;
            Output = null;
            StartTicks = 0;
            StartTimeOA = 0;
            ShouldRestore = false;
        }

        public void StoreContext(UnityTestExecutionContext context)
        {
            var result = context.CurrentResult;
            CurrentTestResultStatus = result.ResultState.Status;
            CurrentTestResultLabel = result.ResultState.Label;
            CurrentTestResultSite = result.ResultState.Site;
            CurrentTestMessage = result.Message;
            CurrentTestStrackTrace = result.StackTrace;
            Output = result.Output;
            StartTicks = context.StartTicks;
            StartTimeOA = context.StartTime.ToOADate();
            ShouldRestore = true;
        }

        public void ApplyContext(UnityTestExecutionContext context)
        {
            var outputProp = context.CurrentResult.GetType().BaseType.GetField("_output", Flags);
            var stringBuilder = (outputProp.GetValue(context.CurrentResult) as StringBuilder);
            stringBuilder.Clear();
            stringBuilder.Append(Output);
            context.StartTicks = StartTicks;
            context.StartTime = DateTime.FromOADate(StartTimeOA);
            context.CurrentResult.SetResult(new ResultState(CurrentTestResultStatus, CurrentTestResultLabel, CurrentTestResultSite), CurrentTestMessage, CurrentTestStrackTrace);
            ShouldRestore = false;
        }
    }
}
