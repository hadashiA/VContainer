using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal interface IUtpLogger
    {
        void Log(Message msg);
    }
}
