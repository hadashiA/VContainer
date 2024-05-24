using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    // This class is used for serialization purposes
    // which requires public access to fields and a default empty constructor
    [Serializable]
    internal class PlayerSystemInfo
    {
        public PlayerSystemInfo() { }

        public PlayerSystemInfo(string operatingSystem, string deviceModel, string deviceName, string processorType, int processorCount, string graphicsDeviceName, int systemMemorySize, string xrModel = "", string xrDevice = "")
        {
            OperatingSystem = operatingSystem;
            DeviceModel = deviceModel;
            DeviceName = deviceName;
            ProcessorType = processorType;
            ProcessorCount = processorCount;
            GraphicsDeviceName = graphicsDeviceName;
            SystemMemorySize = systemMemorySize;
            XrModel = xrModel;
            XrDevice = xrDevice;
        }

        public string OperatingSystem;
        public string DeviceModel;
        public string DeviceName;
        public string ProcessorType;
        public int ProcessorCount;
        public string GraphicsDeviceName;
        public int SystemMemorySize;
        public string XrModel;
        public string XrDevice;
    }
}
