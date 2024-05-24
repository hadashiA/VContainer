using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Unity.PerformanceTesting.Data;
using UnityEngine;

namespace Unity.PerformanceTesting.Editor
{
    public class TestResultXmlParser
    {
        public Run GetPerformanceTestRunFromXml(string resultXmlFileName)
        {
            ValidateInput(resultXmlFileName);
            var xmlDocument = TryLoadResultXmlFile(resultXmlFileName);
            var performanceTestRun = TryParseXmlToPerformanceTestRun(xmlDocument);
            return performanceTestRun;
        }

        private void ValidateInput(string resultXmlFileName)
        {
            if (string.IsNullOrEmpty(resultXmlFileName))
            {
                Debug.LogWarning($"Test results path is null or empty.");
            }

            if (!File.Exists(resultXmlFileName))
            {
                Debug.LogWarning($"Test results file does not exists at path: {resultXmlFileName}");
            }
        }

        private XDocument TryLoadResultXmlFile(string resultXmlFileName)
        {
            try
            {
                return XDocument.Load(resultXmlFileName);
            }
            catch (Exception e)
            {
                var errMsg = $"Failed to load xml result file: {resultXmlFileName}";
                Debug.LogWarning($"{errMsg}\r\nException: {e.Message}\r\n{e.StackTrace}");
            }

            return null;
        }

        private Run TryParseXmlToPerformanceTestRun(XContainer xmlDocument)
        {
            var output = xmlDocument.Descendants("output").ToArray();
            if (!output.Any())
            {
                return null;
            }

            var run = DeserializeMetadata(output);
            DeserializeTestResults(output, run);
            return run;
        }

        private void DeserializeTestResults(IEnumerable<XElement> output, Run run)
        {
            foreach (var element in output)
            {
                foreach (var line in element.Value.Split('\n'))
                {
                    var json = GetJsonFromHashtag("performancetestresult2", line);
                    if (json == null)
                    {
                        continue;
                    }

                    var result = TryDeserializePerformanceTestResultJsonObject(json);
                    if (result != null)
                    {
                        run.Results.Add(result);
                    }
                }
            }
        }

        private Run DeserializeMetadata(IEnumerable<XElement> output)
        {
            foreach (var element in output)
            {
                var pattern = @"##performancetestruninfo2:(.+)\n";
                var regex = new Regex(pattern);
                var matches = regex.Match(element.Value);
                if (matches.Groups.Count == 0) continue;
                if (matches.Captures.Count == 0) continue;

                if (matches.Groups[1].Captures.Count > 1)
                {
                    Debug.LogError("Performance test run had multiple hardware and player settings, there should only be one.");
                    return null;
                }

                var json = matches.Groups[1].Value;
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("Performance test run has incomplete hardware and player settings.");
                    return null;
                }

                var result = TryDeserializePerformanceTestRunJsonObject(json);
                return result;
            }

            return null;
        }

        private PerformanceTestResult TryDeserializePerformanceTestResultJsonObject(string json)
        {
            try
            {
                return JsonUtility.FromJson<PerformanceTestResult>(json);
            }
            catch (Exception e)
            {
                var errMsg = $"Exception thrown while deserializing json string to PerformanceTestResult: {json}";
                Debug.LogWarning($"{errMsg}\r\nException: {e.Message}\r\n{e.StackTrace}");
            }

            return null;
        }

        private Run TryDeserializePerformanceTestRunJsonObject(string json)
        {
            try
            {
                return JsonUtility.FromJson<Run>(json);
            }
            catch (Exception e)
            {
                var errMsg = $"Exception thrown while deserializing json string to PerformanceTestRun: {json}";
                Debug.LogWarning($"{errMsg}\r\nException: {e.Message}\r\n{e.StackTrace}");
            }

            return null;
        }

        private string GetJsonFromHashtag(string tag, string line)
        {
            if (!line.Contains($"##{tag}:")) return null;
            var jsonStart = line.IndexOf('{');
            var openBrackets = 0;
            var stringIndex = jsonStart;
            while (openBrackets > 0 || stringIndex == jsonStart)
            {
                var character = line[stringIndex];
                switch (character)
                {
                    case '{':
                        openBrackets++;
                        break;
                    case '}':
                        openBrackets--;
                        break;
                }

                stringIndex++;
            }

            var jsonEnd = stringIndex;
            return line.Substring(jsonStart, jsonEnd - jsonStart);
        }
    }
}
