using System;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    [Serializable]
    internal class RunProgress
    {
        public const float progressPrTask = 0.0075f;

        [SerializeField]
        public float progressPrTest;

        [SerializeField]
        public float progress;

        [SerializeField]
        public string stageName;

        [SerializeField]
        public string stepName;
    }
}
