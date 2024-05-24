using System;
using System.Collections.Generic;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal class TestListJob
    {
        private CachingTestListProvider m_TestListProvider;
        private TestPlatform m_Platform;
        private Action<ITestAdaptor> m_Callback;
        private IEnumerator<ITestAdaptor> m_ResultEnumerator;
        public TestListJob(CachingTestListProvider testListProvider, TestPlatform platform, Action<ITestAdaptor> callback)
        {
            m_TestListProvider = testListProvider;
            m_Platform = platform;
            m_Callback = callback;
        }

        public void Start()
        {
            m_ResultEnumerator = m_TestListProvider.GetTestListAsync(m_Platform);
            EditorApplication.update += EditorUpdate;
        }

        private void EditorUpdate()
        {
            if (!m_ResultEnumerator.MoveNext())
            {
                m_Callback(m_ResultEnumerator.Current);
                EditorApplication.update -= EditorUpdate;
            }
        }
    }
}
