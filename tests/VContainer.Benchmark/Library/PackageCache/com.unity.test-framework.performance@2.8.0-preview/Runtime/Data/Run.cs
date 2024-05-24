using System;
using System.Collections.Generic;

namespace Unity.PerformanceTesting.Data
{
    [Serializable]
    public class Run
    {
        [RequiredMember]
        public string TestSuite;
        [RequiredMember]
        public long Date;
        [RequiredMember]
        public Player Player;
        [RequiredMember]
        public Hardware Hardware;
        [RequiredMember]
        public Editor Editor;
        [RequiredMember]
        public List<string> Dependencies = new List<string>();
        [RequiredMember]
        public List<PerformanceTestResult> Results = new List<PerformanceTestResult>();
    }
}
