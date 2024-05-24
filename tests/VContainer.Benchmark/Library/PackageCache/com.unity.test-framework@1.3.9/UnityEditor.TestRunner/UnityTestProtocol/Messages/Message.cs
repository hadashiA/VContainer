using System;
using System.Diagnostics;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    [Serializable]
    internal abstract class Message
    {
        public string type;
        // Milliseconds since unix epoch
        public ulong time;
        public int version;
        public string phase;
        public int processId;

        protected Message()
        {
            version = 2;
            phase = "Immediate";
            processId = Process.GetCurrentProcess().Id;
            AddTimeStamp();
        }

        public void AddTimeStamp()
        {
            time = Convert.ToUInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
        }
    }
}
