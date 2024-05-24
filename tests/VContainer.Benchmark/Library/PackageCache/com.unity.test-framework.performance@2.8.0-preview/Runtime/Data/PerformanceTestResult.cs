using System;
using System.Collections.Generic;

namespace Unity.PerformanceTesting.Data
{
    [Serializable]
    public class PerformanceTestResult
    {
        [RequiredMember]
        public string Name;
        [RequiredMember]
        public string Version;
        [RequiredMember]
        public List<string> Categories = new List<string>();
        [RequiredMember]
        public List<SampleGroup> SampleGroups = new List<SampleGroup>();
    }
}
