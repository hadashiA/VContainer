using System;
using System.IO;
using System.Xml;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal class ResultsWriter
    {
        private const string k_nUnitVersion = "3.5.0.0";

        private const string k_TestRunNode = "test-run";
        private const string k_Id = "id";
        private const string k_Testcasecount = "testcasecount";
        private const string k_Result = "result";
        private const string k_Total = "total";
        private const string k_Passed = "passed";
        private const string k_Failed = "failed";
        private const string k_Inconclusive = "inconclusive";
        private const string k_Skipped = "skipped";
        private const string k_Asserts = "asserts";
        private const string k_EngineVersion = "engine-version";
        private const string k_ClrVersion = "clr-version";
        private const string k_StartTime = "start-time";
        private const string k_EndTime = "end-time";
        private const string k_Duration = "duration";

        private const string k_TimeFormat = "u";

        public void WriteResultToFile(ITestResultAdaptor result, string filePath)
        {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Saving results to: {0}", filePath);

            try
            {
                if (!Directory.Exists(filePath))
                {
                    CreateDirectory(filePath);
                }

                using (var fileStream = File.CreateText(filePath))
                {
                    WriteResultToStream(result, fileStream);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Saving result file failed.");
                Debug.LogException(ex);
            }
        }

        private void CreateDirectory(string filePath)
        {
            var driectoryPath = Path.GetDirectoryName(filePath);
            if (!String.IsNullOrEmpty(driectoryPath))
            {
                Directory.CreateDirectory(driectoryPath);
            }
        }

        public void WriteResultToStream(ITestResultAdaptor result, StreamWriter streamWriter, XmlWriterSettings settings = null)
        {
            settings = settings ?? new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = false;

            using (var xmlWriter = XmlWriter.Create(streamWriter, settings))
            {
                WriteResultsToXml(result, xmlWriter);
            }
        }

        private void WriteResultsToXml(ITestResultAdaptor result, XmlWriter xmlWriter)
        {
            // XML format as specified at https://github.com/nunit/docs/wiki/Test-Result-XML-Format

            var testRunNode = new TNode(k_TestRunNode);

            testRunNode.AddAttribute(k_Id, "2");
            testRunNode.AddAttribute(k_Testcasecount, (result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount).ToString());
            testRunNode.AddAttribute(k_Result, result.ResultState);
            testRunNode.AddAttribute(k_Total, (result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount).ToString());
            testRunNode.AddAttribute(k_Passed, result.PassCount.ToString());
            testRunNode.AddAttribute(k_Failed, result.FailCount.ToString());
            testRunNode.AddAttribute(k_Inconclusive, result.InconclusiveCount.ToString());
            testRunNode.AddAttribute(k_Skipped, result.SkipCount.ToString());
            testRunNode.AddAttribute(k_Asserts, result.AssertCount.ToString());
            testRunNode.AddAttribute(k_EngineVersion, k_nUnitVersion);
            testRunNode.AddAttribute(k_ClrVersion, Environment.Version.ToString());
            testRunNode.AddAttribute(k_StartTime, result.StartTime.ToString(k_TimeFormat));
            testRunNode.AddAttribute(k_EndTime, result.EndTime.ToString(k_TimeFormat));
            testRunNode.AddAttribute(k_Duration, result.Duration.ToString());

            var resultNode = result.ToXml();
            testRunNode.ChildNodes.Add(resultNode);

            testRunNode.WriteTo(xmlWriter);
        }
    }
}
