using System;
using NUnit.Framework.Internal;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.Logging;

namespace UnityEngine.TestTools.NUnitExtensions
{
    /// <summary>
    /// Specialization of BaseDelegator that makes sure objects are created on the MainThread.
    /// It also deals with ScriptableObjects so that tests can survive assembly reload.
    /// </summary>
    internal class ConstructDelegator
    {
        private Type m_RequestedType;
        private object[] m_Arguments;

        protected ScriptableObject m_CurrentRunningTest;
        private readonly IStateSerializer m_StateSerializer;

        protected Exception m_Exception;
        protected object m_Result;
        protected ITestExecutionContext m_Context;

        public ConstructDelegator(IStateSerializer stateSerializer)
        {
            m_StateSerializer = stateSerializer;
        }

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

        protected void SetCurrentTestContext()
        {
            var prop = typeof(UnityTestExecutionContext).GetProperty("CurrentContext");
            if (prop != null)
            {
                prop.SetValue(null, m_Context, null);
            }
        }

        public object Delegate(Type type, object[] arguments)
        {
            AssertState();
            m_Context = UnityTestExecutionContext.CurrentContext;

            m_RequestedType = type;
            m_Arguments = arguments;

            using (var logScope = new LogScope())
            {
                Execute(logScope);
            }

            return HandleResult();
        }

        private void AssertState()
        {
            if (m_RequestedType != null)
            {
                throw new Exception("Constructor not executed yet");
            }
        }

        public bool HasAction()
        {
            return m_RequestedType != null;
        }

        public void Execute(LogScope logScope)
        {
            try
            {
                if (typeof(ScriptableObject).IsAssignableFrom(m_RequestedType))
                {
                    if (m_CurrentRunningTest != null && m_RequestedType != m_CurrentRunningTest.GetType())
                    {
                        DestroyCurrentTestObjectIfExists();
                    }
                    if (m_CurrentRunningTest == null)
                    {
                        if (m_StateSerializer.CanRestoreFromScriptableObject(m_RequestedType))
                        {
                            m_CurrentRunningTest = m_StateSerializer.RestoreScriptableObjectInstance();
                        }
                        else
                        {
                            m_CurrentRunningTest = ScriptableObject.CreateInstance(m_RequestedType);
                        }
                    }
                    m_Result = m_CurrentRunningTest;
                }
                else
                {
                    DestroyCurrentTestObjectIfExists();
                    m_Result = Activator.CreateInstance(m_RequestedType, m_Arguments);
                    if (m_StateSerializer.CanRestoreFromJson(m_RequestedType))
                    {
                        m_StateSerializer.RestoreClassFromJson(ref m_Result);
                    }
                }
                logScope.EvaluateLogScope(true);
            }
            catch (Exception e)
            {
                m_Exception = e;
            }
            finally
            {
                m_RequestedType = null;
                m_Arguments = null;
            }
        }

        public void DestroyCurrentTestObjectIfExists()
        {
            if (m_CurrentRunningTest != null)
            {
                Object.DestroyImmediate(m_CurrentRunningTest);
            }
        }
    }
}
