using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    internal class TestActionCommand : BeforeAfterTestCommandBase<ITestAction>
    {
        private static readonly Dictionary<MethodInfo, List<ITestAction>> m_TestActionsCache = new Dictionary<MethodInfo, List<ITestAction>>();

        public TestActionCommand(TestCommand innerCommand)
            : base(innerCommand, "BeforeTest", "AfterTest")
        {
            if (Test.TypeInfo.Type != null)
            {
                BeforeActions = GetTestActions(m_TestActionsCache, Test.Method.MethodInfo);
                AfterActions = BeforeActions;
            }
        }

        protected override bool AllowFrameSkipAfterAction(ITestAction action)
        {
            return false;
        }

        protected override IEnumerator InvokeBefore(ITestAction action, Test test, UnityTestExecutionContext context)
        {
            action.BeforeTest(test);
            yield return null;
        }

        protected override IEnumerator InvokeAfter(ITestAction action, Test test, UnityTestExecutionContext context)
        {
            action.AfterTest(test);
            yield return null;
        }

        protected override BeforeAfterTestCommandState GetState(UnityTestExecutionContext context)
        {
            // TestActionCommand does not support domain reloads and will not need a persisted state.
            return new BeforeAfterTestCommandState();
        }
    }
}
