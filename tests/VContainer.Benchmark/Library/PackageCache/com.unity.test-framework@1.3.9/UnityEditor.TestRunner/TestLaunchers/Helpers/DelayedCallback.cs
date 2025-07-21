using System;

namespace UnityEditor.TestTools.TestRunner
{
    internal class DelayedCallback
    {
        private Action m_Callback;
        private double m_CallbackTime;
        private double m_Delay;

        public DelayedCallback(Action function, double timeFromNow)
        {
            m_Callback = function;
            m_CallbackTime = EditorApplication.timeSinceStartup + timeFromNow;
            m_Delay = timeFromNow;
            EditorApplication.update += Update;
        }

        public void Clear()
        {
            EditorApplication.update -= Update;
            m_CallbackTime = 0.0;
            m_Callback = null;
        }

        private void Update()
        {
            if (EditorApplication.timeSinceStartup > m_CallbackTime)
            {
                // Clear state before firing callback to ensure reset (callback could call ExitGUI)
                var callback = m_Callback;
                Clear();

                callback?.Invoke();
            }
        }

        public void Reset()
        {
            if (m_Callback != null)
            {
                m_CallbackTime = EditorApplication.timeSinceStartup + m_Delay;
            }
        }
    }
}