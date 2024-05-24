using System;
using System.Threading;
using NUnit.Framework.Internal;

namespace UnityEngine.TestTools.NUnitExtensions
{
    internal abstract class BaseDelegator
    {
        protected ManualResetEvent m_Signal = new ManualResetEvent(false);

        protected object m_Result;
        protected Exception m_Exception;
        protected ITestExecutionContext m_Context;

        protected bool m_Aborted;

        protected object HandleResult()
        {
            SetCurrentTestContext();
            if (m_Exception != null)
            {
                var temp = m_Exception;
                m_Exception = null;
                throw temp;
            }
            var tempResult = m_Result;
            m_Result = null;
            return tempResult;
        }

        protected void WaitForSignal()
        {
            while (!m_Signal.WaitOne(100))
            {
                if (m_Aborted)
                {
                    m_Aborted = false;
                    Reflect.MethodCallWrapper = null;
                    throw new Exception();
                }
            }
        }

        public void Abort()
        {
            m_Aborted = true;
        }

        protected void SetCurrentTestContext()
        {
            var prop = typeof(TestExecutionContext).GetProperty("CurrentContext");
            if (prop != null)
            {
                prop.SetValue(null, m_Context, null);
            }
        }
    }
}
