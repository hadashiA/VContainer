using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.CommandLineTest;

namespace Unity.PerformanceTesting.Editor
{
    [InitializeOnLoad]
    public class TestRunnerInitializer
    {
        static TestRunnerInitializer()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var obj = ScriptableObject.CreateInstance<PerformanceTestRunSaver>();
            api.RegisterCallbacks(obj);
        }
    }

    [Serializable]
    public class PerformanceTestRunSaver : ScriptableObject, ICallbacks
    {
        void ICallbacks.RunStarted(ITestAdaptor testsToRun)
        {
            PerformanceTest.Active = null;
        }

        void ICallbacks.RunFinished(ITestResultAdaptor result)
        {
            PlayerCallbacks.saved = false;

            try
            {
                var resultWriter = new ResultsWriter();
                var xmlPath = Path.Combine(Application.persistentDataPath, "TestResults.xml");
                var jsonPath = Path.Combine(Application.persistentDataPath, "PerformanceTestResults.json");
                resultWriter.WriteResultToFile(result, xmlPath);
                var xmlParser = new TestResultXmlParser();
                var run = xmlParser.GetPerformanceTestRunFromXml(xmlPath);
                if (run == null) return;
                File.WriteAllText(jsonPath, JsonUtility.ToJson(run, true));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.InnerException);
            }
        }

        void ICallbacks.TestStarted(ITestAdaptor test) { }

        void ICallbacks.TestFinished(ITestResultAdaptor result) { }
    }
}
