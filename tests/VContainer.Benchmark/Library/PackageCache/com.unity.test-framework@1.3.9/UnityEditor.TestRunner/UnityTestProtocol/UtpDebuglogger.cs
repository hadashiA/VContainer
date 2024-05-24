using System;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class UtpDebugLogger : IUtpLogger
    {
        internal const string UtpPrefix = "\n##utp:";

        public void Log(Message msg)
        {
            var msgJson = JsonUtility.ToJson(msg);
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}{1}", UtpPrefix, msgJson);
        }
    }
}
