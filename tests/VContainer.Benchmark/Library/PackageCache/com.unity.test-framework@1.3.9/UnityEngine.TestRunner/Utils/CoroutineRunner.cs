using System;
using System.Collections;
using NUnit.Framework.Internal;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestTools.Utils
{
    internal class CoroutineRunner
    {
        private bool m_Running;
        private readonly MonoBehaviour m_Controller;
        private readonly UnityTestExecutionContext m_Context;
        private IEnumerator m_TestCoroutine;

        public CoroutineRunner(MonoBehaviour playmodeTestsController, UnityTestExecutionContext context)
        {
            m_Controller = playmodeTestsController;
            m_Context = context;
        }

        public IEnumerator HandleEnumerableTest(IEnumerator testEnumerator)
        {
            do
            {
                if (!m_Running)
                {
                    m_Running = true;
                    m_TestCoroutine = ExMethod(testEnumerator);
                    m_Controller.StartCoroutine(m_TestCoroutine);
                }
                if (m_Context.ExecutionStatus == TestExecutionStatus.StopRequested || m_Context.ExecutionStatus == TestExecutionStatus.AbortRequested)
                {
                    StopAllRunningCoroutines();
                    yield break;
                }
                yield return null;
            }
            while (m_Running);
        }

        private void StopAllRunningCoroutines()
        {
            if (m_TestCoroutine != null)
            {
                m_Controller.StopCoroutine(m_TestCoroutine);
            }
        }

        private IEnumerator ExMethod(IEnumerator e)
        {
            yield return m_Controller.StartCoroutine(e);
            m_Running = false;
        }
    }
}
