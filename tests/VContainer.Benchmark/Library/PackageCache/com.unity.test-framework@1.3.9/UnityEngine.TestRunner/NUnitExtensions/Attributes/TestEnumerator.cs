using System;
using System.Collections;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestTools
{
    internal class TestEnumerator
    {
        private readonly ITestExecutionContext m_Context;
        private static IEnumerator m_TestEnumerator;

        public static IEnumerator Enumerator { get { return m_TestEnumerator; } }

        public static void Reset()
        {
            m_TestEnumerator = null;
        }

        public TestEnumerator(ITestExecutionContext context, IEnumerator testEnumerator)
        {
            m_Context = context;
            m_TestEnumerator = testEnumerator;
        }

        public IEnumerator Execute()
        {
            m_Context.CurrentResult.SetResult(ResultState.Success);

            return Execute(m_TestEnumerator, new EnumeratorContext(m_Context));
        }

        private IEnumerator Execute(IEnumerator enumerator, EnumeratorContext context)
        {
            while (true)
            {
                if (context.ExceptionWasRecorded)
                {
                    break;
                }

                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    context.RecordExceptionWithHint(ex);
                    break;
                }

                if (enumerator.Current is IEnumerator nestedEnumerator)
                {
                    yield return Execute(nestedEnumerator, context);
                }
                else
                {
                    yield return enumerator.Current;
                }
            }
        }

        private class EnumeratorContext
        {
            private readonly ITestExecutionContext m_Context;

            public EnumeratorContext(ITestExecutionContext context)
            {
                m_Context = context;
            }

            public bool ExceptionWasRecorded
            {
                get;
                private set;
            }

            public void RecordExceptionWithHint(Exception ex)
            {
                if (ExceptionWasRecorded)
                {
                    return;
                }
                m_Context.CurrentResult.RecordException(ex);
                ExceptionWasRecorded = true;
            }
        }
    }
}
