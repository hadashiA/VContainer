using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.PerformanceTesting.Runtime;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Unity.PerformanceTesting.Exceptions;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions;

[assembly: InternalsVisibleTo("Unity.PerformanceTesting.Tests.Editor")]
namespace Unity.PerformanceTesting
{
    [Serializable]
    public class PerformanceTest
    {
        public string Name;
        public string Version;
        public List<string> Categories = new List<string>();
        public List<SampleGroup> SampleGroups = new List<SampleGroup>();
        public static PerformanceTest Active { get; set; }
        internal static List<IDisposable> Disposables = new List<IDisposable>(1024);
        PerformanceTestHelper m_PerformanceTestHelper;

        public delegate void Callback();

        public static Callback OnTestEnded;

        public PerformanceTest()
        {
            Active = this;
        }

        internal static void StartTest(ITest currentTest)
        {
            if (currentTest.IsSuite) return;

            var go = new GameObject("PerformanceTestHelper");
            go.hideFlags = HideFlags.HideAndDontSave;
            var performanceTestHelper = go.AddComponent<PerformanceTestHelper>();

            string methodName = currentTest.Name.Contains("(")
                ? currentTest.Name.Remove(currentTest.Name.IndexOf("(", StringComparison.Ordinal))
                : currentTest.Name;

            var fullName = currentTest.MethodName != methodName ? $"{currentTest.ClassName}.{currentTest.MethodName}.{currentTest.Name}" : currentTest.FullName;

            var test = new PerformanceTest
            {
                Name = fullName,
                Categories = currentTest.GetAllCategoriesFromTest(),
                Version = GetVersion(currentTest),
                m_PerformanceTestHelper = performanceTestHelper
            };

            Active = test;
            performanceTestHelper.ActiveTest = test;
        }

        private static string GetVersion(ITest currentTest)
        {
            string version = "";
            var methodVersions = currentTest.Method.GetCustomAttributes<VersionAttribute>(false);
            var classVersion = currentTest.TypeInfo.Type.GetCustomAttributes(typeof(VersionAttribute), true);

            if (classVersion.Length > 0)
                version = ((VersionAttribute)classVersion[0]).Version + ".";
            if (methodVersions.Length > 0)
                version += methodVersions[0].Version;
            else
                version += "1";

            return version;
        }

        internal static void EndTest(ITest test)
        {
            if (test.IsSuite) return;

            if (Active.m_PerformanceTestHelper != null && Active.m_PerformanceTestHelper.gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(Active.m_PerformanceTestHelper.gameObject);
            }

            DisposeMeasurements();
            Active.CalculateStatisticalValues();
            OnTestEnded?.Invoke();
            Active.LogOutput();

            TestContext.Out.WriteLine("##performancetestresult2:" + Active.Serialize());
            PlayerCallbacks.LogMetadata();
            Active = null;
            GC.Collect();
        }

        private static void DisposeMeasurements()
        {
            for (var i = 0; i < Disposables.Count; i++)
            {
                Disposables[i].Dispose();
            }

            Disposables.Clear();
        }

        public static SampleGroup GetSampleGroup(string name)
        {
            if (Active == null) throw new PerformanceTestException("Trying to record samples but there is no active performance tests.");
            foreach (var sampleGroup in Active.SampleGroups)
            {
                if (sampleGroup.Name == name)
                    return sampleGroup;
            }

            return null;
        }

        public static void AddSampleGroup(SampleGroup sampleGroup)
        {
            Active.SampleGroups.Add(sampleGroup);
        }

        internal string Serialize()
        {
            return JsonUtility.ToJson(Active);
        }

        public void CalculateStatisticalValues()
        {
            foreach (var sampleGroup in SampleGroups)
            {
                sampleGroup.UpdateStatistics();
            }
        }

        private void LogOutput()
        {
            TestContext.Out.WriteLine(ToString());
        }

        public override string ToString()
        {
            var logString = new StringBuilder();

            foreach (var sampleGroup in SampleGroups)
            {
                logString.Append(sampleGroup.Name);

                if (sampleGroup.Samples.Count == 1)
                {
                    logString.AppendFormat(" {0:0.00} {1}", sampleGroup.Samples[0],  sampleGroup.Unit);
                }
                else
                {
                    logString.AppendFormat(
                        " {0} Median:{1:0.00} Min:{2:0.00} Max:{3:0.00} Avg:{4:0.00} Std:{5:0.00} SampleCount: {6} Sum: {7:0.00}",
                        sampleGroup.Unit, sampleGroup.Median, sampleGroup.Min, sampleGroup.Max,
                        sampleGroup.Average,
                        sampleGroup.StandardDeviation, sampleGroup.Samples.Count, sampleGroup.Sum
                    );
                }

                logString.Append("\n");
            }

            return logString.ToString();
        }
    }
}
