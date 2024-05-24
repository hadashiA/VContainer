using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using Unity.Profiling;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    internal class EnumerableSetUpTearDownCommand : BeforeAfterTestCommandBase<MethodInfo>
    {
        private static readonly Dictionary<Type, List<MethodInfo>> m_BeforeActionsCache = new Dictionary<Type, List<MethodInfo>>();
        private static readonly Dictionary<Type, List<MethodInfo>> m_AfterActionsCache = new Dictionary<Type, List<MethodInfo>>();

        public EnumerableSetUpTearDownCommand(TestCommand innerCommand)
            : base(innerCommand, "SetUp", "TearDown")
        {
            using (new ProfilerMarker(nameof(EnumerableSetUpTearDownCommand)).Auto())
            {
                if (Test.TypeInfo.Type != null)
                {
                    BeforeActions = GetActions(m_BeforeActionsCache, Test.TypeInfo.Type, typeof(UnitySetUpAttribute), new[] {typeof(IEnumerator)});
                    AfterActions = GetActions(m_AfterActionsCache, Test.TypeInfo.Type, typeof(UnityTearDownAttribute), new[] {typeof(IEnumerator)}).Reverse().ToArray();
                }
            }
        }

        protected override bool MoveAfterEnumerator(IEnumerator enumerator, Test test)
        {
            using (new ProfilerMarker(test.Name + ".TearDown").Auto())
                return base.MoveAfterEnumerator(enumerator, test);
        }

        protected override bool MoveBeforeEnumerator(IEnumerator enumerator, Test test)
        {
            using (new ProfilerMarker(test.Name + ".Setup").Auto())
                return base.MoveBeforeEnumerator(enumerator, test);
        }

        protected override IEnumerator InvokeBefore(MethodInfo action, Test test, UnityTestExecutionContext context)
        {
            return (IEnumerator)Reflect.InvokeMethod(action, context.TestObject);
        }

        protected override IEnumerator InvokeAfter(MethodInfo action, Test test, UnityTestExecutionContext context)
        {
            return (IEnumerator)Reflect.InvokeMethod(action, context.TestObject);
        }

        protected override BeforeAfterTestCommandState GetState(UnityTestExecutionContext context)
        {
            return context.SetUpTearDownState;
        }
    }
}
