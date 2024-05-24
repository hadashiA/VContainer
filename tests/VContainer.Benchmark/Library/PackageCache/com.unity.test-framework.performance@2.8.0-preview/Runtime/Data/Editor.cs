using System;

namespace Unity.PerformanceTesting.Data
{
    [Serializable]
    public class Editor
    {
        [RequiredMember]
        public string Version;
        [RequiredMember]
        public string Branch;
        [RequiredMember]
        public string Changeset;
        [RequiredMember]
        public int Date;
    }
}
