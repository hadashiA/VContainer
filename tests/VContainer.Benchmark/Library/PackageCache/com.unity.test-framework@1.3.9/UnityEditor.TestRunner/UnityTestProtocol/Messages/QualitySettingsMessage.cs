using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    [Serializable]
    internal class QualitySettingsMessage : Message
    {
        public QualitySettings QualitySettings;
        public QualitySettingsMessage()
        {
            type = "QualitySettings";
        }
    }
}
