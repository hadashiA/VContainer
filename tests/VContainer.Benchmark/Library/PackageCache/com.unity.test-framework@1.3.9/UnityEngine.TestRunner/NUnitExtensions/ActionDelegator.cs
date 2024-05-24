using System;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.Logging;

namespace UnityEngine.TestTools.NUnitExtensions
{
    /// <summary>
    /// This class delegates actions from the NUnit thread that should be executed on the main thread.
    /// NUnit thread calls Delegate which blocks the execution on the thread until the action is executed.
    /// The main thread will poll for awaiting actions (HasAction) and invoke them (Execute).
    /// Once the action is executed, the main thread releases the lock and executino on the NUnit thread is continued.
    /// </summary>
    internal class ActionDelegator : BaseDelegator
    {
        private Func<object> m_Action;
        public object Delegate(Action action)
        {
            return Delegate(() => { action(); return null; });
        }

        public object Delegate(Func<object> action)
        {
            if (m_Aborted)
            {
                return null;
            }

            AssertState();
            m_Context = UnityTestExecutionContext.CurrentContext;

            m_Signal.Reset();
            m_Action = action;

            WaitForSignal();

            return HandleResult();
        }

        private void AssertState()
        {
            if (m_Action != null)
            {
                throw new Exception("Action not executed yet");
            }
        }

        public bool HasAction()
        {
            return m_Action != null;
        }

        public void Execute(LogScope logScope)
        {
            try
            {
                SetCurrentTestContext();
                m_Result = m_Action();
                logScope.EvaluateLogScope(true);
            }
            catch (Exception e)
            {
                m_Exception = e;
            }
            finally
            {
                m_Action = null;
                m_Signal.Set();
            }
        }
    }
}
