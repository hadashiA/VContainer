using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    [Serializable]
    internal class PlayerSystemInfoMessage : Message
    {
        public PlayerSystemInfo PlayerSystemInfo;
        public PlayerSystemInfoMessage()
        {
            type = "PlayerSystemInfo";
        }
    }
}
