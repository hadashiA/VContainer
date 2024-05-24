using System;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    [Serializable]
    internal class TaskInfo
    {
        [SerializeField]
        public int index;

        [SerializeField]
        public int stopBeforeIndex;

        [SerializeField]
        public int pc;

        [SerializeField]
        public TaskMode taskMode = TaskMode.Normal;
    }
}
