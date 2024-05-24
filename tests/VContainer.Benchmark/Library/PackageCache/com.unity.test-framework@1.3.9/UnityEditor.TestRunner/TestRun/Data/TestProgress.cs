using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    [Serializable]
    internal class TestProgress
    {
        [SerializeField]
        public string CurrentTest;

        [SerializeField]
        public string[] AllTestsToRun;

        [SerializeField]
        public List<string> RemainingTests;

        [SerializeField]
        public List<string> CompletedTests;

        public TestProgress(string[] allTestsToRun)
        {
            AllTestsToRun = allTestsToRun;
            RemainingTests = allTestsToRun.ToList();
            CompletedTests = new List<string>();
        }
    }
}
