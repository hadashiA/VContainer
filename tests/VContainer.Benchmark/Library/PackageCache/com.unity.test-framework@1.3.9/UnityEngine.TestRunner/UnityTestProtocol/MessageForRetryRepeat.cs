using System;
using System.Diagnostics;

namespace UnityEngine.TestRunner.TestProtocol
{
    [Serializable]
    internal class MessageForRetryRepeat
    {
        public string type;
        // Milliseconds since unix epoch
        public ulong time;
        public int version;
        public string phase;
        public int processId;

        public MessageForRetryRepeat()
        {
            type = "TestStatus";
            version = 2;
            phase = "Immediate";
            processId = Process.GetCurrentProcess().Id;
            AddTimeStamp();
        }

        public void AddTimeStamp()
        {
            time = Convert.ToUInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
        }

        internal const string UtpPrefix = "\n##utp:";

        public override string ToString()
        {
            var msgJson = JsonUtility.ToJson(this);
            return $"{UtpPrefix}{msgJson}";
        }
    }
}