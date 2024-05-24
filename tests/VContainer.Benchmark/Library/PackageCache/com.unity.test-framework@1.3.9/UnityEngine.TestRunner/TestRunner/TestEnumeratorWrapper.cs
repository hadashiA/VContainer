using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestTools.TestRunner
{
    internal class TestEnumeratorWrapper
    {
        private readonly TestMethod m_TestMethod;

        public TestEnumeratorWrapper(TestMethod testMethod)
        {
            m_TestMethod = testMethod;
        }

        public IEnumerator GetEnumerator(ITestExecutionContext context)
        {
            if (m_TestMethod.Method.ReturnType.Type == typeof(IEnumerator))
            {
                return HandleEnumerableTest(context);
            }
            var message = string.Format("Return type {0} of {1} in {2} is not supported.",
                m_TestMethod.Method.ReturnType, m_TestMethod.Method.Name, m_TestMethod.Method.TypeInfo.FullName);
            if (m_TestMethod.Method.ReturnType.Type == typeof(IEnumerable))
            {
                message += "\nDid you mean IEnumerator?";
            }
            throw new InvalidSignatureException(message);
        }

        private IEnumerator HandleEnumerableTest(ITestExecutionContext context)
        {
            try
            {
                return m_TestMethod.Method.MethodInfo.Invoke(context.TestObject, m_TestMethod.parms != null ? m_TestMethod.parms.OriginalArguments : null) as IEnumerator;
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
