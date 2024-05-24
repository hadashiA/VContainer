using System;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.Api
{
    [Serializable]
    internal class TestRunProgress
    {
        [SerializeField]
        public string RunGuid;
        [SerializeField]
        public ExecutionSettings ExecutionSettings;

        [SerializeField]
        public bool HasFinished;

        [SerializeField]
        public float Progress;
        [SerializeField]
        public string CurrentStepName;
        [SerializeField]
        public string CurrentStageName;
    }
}
