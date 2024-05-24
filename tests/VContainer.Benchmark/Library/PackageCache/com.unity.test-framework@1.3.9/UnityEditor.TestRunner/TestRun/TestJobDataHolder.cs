using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    internal class TestJobDataHolder : ScriptableSingleton<TestJobDataHolder>, ITestJobDataHolder
    {
        [SerializeField]
        public List<TestJobData> TestRuns = new List<TestJobData>();

        [NonSerialized]
        private readonly Dictionary<string, ITestJobRunner> m_Runners = new Dictionary<string, ITestJobRunner>();

        public void RegisterRun(ITestJobRunner runner, TestJobData data)
        {
            TestRuns.Add(data);
            m_Runners.Add(data.guid, runner);
        }

        public void UnregisterRun(ITestJobRunner runner, TestJobData data)
        {
            TestRuns.Remove(data);
            m_Runners.Remove(data.guid);
        }

        public ITestJobRunner GetRunner(string guid)
        {
            return m_Runners.ContainsKey(guid) ? m_Runners[guid] : null;
        }

        public ITestJobRunner[] GetAllRunners()
        {
            return m_Runners.Values.ToArray();
        }

        [InitializeOnLoadMethod]
        private static void ResumeRunningJobs()
        {
            try
            {
                foreach (var testRun in instance.TestRuns.ToArray())
                {
                    if (testRun.isRunning)
                    {
                        var runner = new TestJobRunner();
                        runner.RunJob(testRun);
                        instance.m_Runners[testRun.guid] = runner;
                    }
                    else
                    {
                        instance.TestRuns.Remove(testRun);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.ClearProgressBar();
                EditorApplication.UnlockReloadAssemblies();
                instance.TestRuns.Clear();
            }
        }
    }
}
