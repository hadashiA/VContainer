
namespace Unity.PerformanceTesting.Data
{
    public class Project
    {
        [RequiredMember]
        public string Name;
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
