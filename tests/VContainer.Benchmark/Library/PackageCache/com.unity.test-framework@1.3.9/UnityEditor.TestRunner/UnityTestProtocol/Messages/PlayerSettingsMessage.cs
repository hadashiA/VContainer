using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    [Serializable]
    internal class PlayerSettingsMessage : Message
    {
        public PlayerSettings PlayerSettings;
        public PlayerSettingsMessage()
        {
            type = "PlayerSettings";
        }
    }
}
