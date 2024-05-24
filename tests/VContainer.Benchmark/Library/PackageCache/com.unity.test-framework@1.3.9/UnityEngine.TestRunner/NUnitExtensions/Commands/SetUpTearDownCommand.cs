using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using Unity.Profiling;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    internal class SetUpTearDownCommand : BeforeAfterTestCommandBase<MethodInfo>
    {
        private static readonly Dictionary<Type, List<MethodInfo>> m_BeforeActionsCache = new Dictionary<Type, List<MethodInfo>>();
        private static readonly Dictionary<Type, List<MethodInfo>> m_AfterActionsCache = new Dictionary<Type, List<MethodInfo>>();

        public SetUpTearDownCommand(TestCommand innerCommand)
            : base(innerCommand, "SetUp", "TearDown")
        {
            using (new ProfilerMarker(nameof(SetUpTearDownCommand)).Auto())
            {
                if (Test.TypeInfo.Type != null)
                {
                    BeforeActions = GetActions(m_BeforeActionsCache, Test.TypeInfo.Type, typeof(SetUpAttribute), new[] {typeof(void), typeof(Task)});
                    AfterActions =  GetActions(m_AfterActionsCache, Test.TypeInfo.Type, typeof(TearDownAttribute), new[] {typeof(void), typeof(Task)}).Reverse().ToArray();
                }
            }
        }

        protected override bool AllowFrameSkipAfterAction(MethodInfo action)
        {
            return action.ReturnType == typeof(Task);
        }

        protected override IEnumerator InvokeBefore(MethodInfo action, Test test, UnityTestExecutionContext context)
        {
            if (action.ReturnType == typeof(Task))
            {
                Task task;
                using (new ProfilerMarker(test.Name + ".Setup").Auto())
                    task = HandleTaskCommand(action, context);
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                if (task.IsFaulted)
                {
                    ExceptionDispatchInfo.Capture(task.Exception.InnerExceptions.Count == 1 ? task.Exception.InnerException : task.Exception).Throw();
                }
            }
            else
            {
                using (new ProfilerMarker(test.Name + ".Setup").Auto())
                    Reflect.InvokeMethod(action, context.TestObject);
                yield return null;
            }
        }

        protected override IEnumerator InvokeAfter(MethodInfo action, Test test, UnityTestExecutionContext context)
        {
            if (action.ReturnType == typeof(Task))
            {
                Task task;
                using (new ProfilerMarker(test.Name + ".TearDown").Auto())
                    task = HandleTaskCommand(action, context);
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                if (task.IsFaulted)
                {
                    ExceptionDispatchInfo.Capture(task.Exception.InnerExceptions.Count == 1 ? task.Exception.InnerException : task.Exception).Throw();
                }
            }
            else
            {
                using (new ProfilerMarker(test.Name + ".TearDown").Auto())
                    Reflect.InvokeMethod(action, context.TestObject);
                yield return null;
            }
        }

        private Task HandleTaskCommand(MethodInfo action, UnityTestExecutionContext context)
        {
            try
            {
                return Reflect.InvokeMethod(action, context.TestObject) as Task;
            }
            catch (TargetInvocationException e)
            {
                throw e;
            }
        }

        protected override BeforeAfterTestCommandState GetState(UnityTestExecutionContext context)
        {
            // Normal Setup/Teardown does not support domain reloads and will not need a persisted state.
            return new BeforeAfterTestCommandState();
        }
    }
}
