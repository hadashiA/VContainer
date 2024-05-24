using System;
using System.Collections;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestTools.TestRunner
{
    internal class TestTaskWrapper
    {
        private readonly TestMethod m_TestMethod;

        public TestTaskWrapper(TestMethod testMethod)
        {
            m_TestMethod = testMethod;
        }

        public IEnumerator Execute(ITestExecutionContext context)
        {
            var task = HandleEnumerableTest(context);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(task.Exception.InnerExceptions.Count == 1 ? task.Exception.InnerException : task.Exception).Throw();
            }
        }

        private Task HandleEnumerableTest(ITestExecutionContext context)
        {
            try
            {
                return m_TestMethod.Method.MethodInfo.Invoke(context.TestObject, m_TestMethod.parms != null ? m_TestMethod.parms.OriginalArguments : null) as Task;
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is IgnoreException)
                {
                    context.CurrentResult.SetResult(ResultState.Ignored, e.InnerException.Message);
                    return null;
                }
                throw;
            }
        }
    }
}
