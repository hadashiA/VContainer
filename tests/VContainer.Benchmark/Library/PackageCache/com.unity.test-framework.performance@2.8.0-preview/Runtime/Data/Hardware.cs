using System;

namespace Unity.PerformanceTesting.Data
{
    [Serializable]
    public class Hardware
    {
        [RequiredMember]
        public string OperatingSystem;
        [RequiredMember]
        public string DeviceModel;
        [RequiredMember]
        public string DeviceName;
        [RequiredMember]
        public string ProcessorType;
        [RequiredMember]
        public int ProcessorCount;
        [RequiredMember]
        public string GraphicsDeviceName;
        [RequiredMember]
        public int SystemMemorySizeMB;
    }
}
