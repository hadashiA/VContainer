using System;
using System.IO;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.Utils;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    [Serializable]
    internal class ResultsSavingCallbacks : ScriptableObject, ICallbacks
    {
        [SerializeField]
        public string m_ResultFilePath;

        public ResultsSavingCallbacks()
        {
            m_ResultFilePath = GetDefaultResultFilePath();
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public virtual void RunFinished(ITestResultAdaptor testResults)
        {
            if (string.IsNullOrEmpty(m_ResultFilePath))
            {
                m_ResultFilePath = GetDefaultResultFilePath();
            }

            var resultWriter = new ResultsWriter();
            resultWriter.WriteResultToFile(testResults, m_ResultFilePath);
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
        }

        private static string GetDefaultResultFilePath()
        {
            var fileName = "TestResults-" + DateTime.Now.Ticks + ".xml";
            var projectPath = Directory.GetCurrentDirectory();
            return Paths.Combine(projectPath, fileName);
        }
    }
}
