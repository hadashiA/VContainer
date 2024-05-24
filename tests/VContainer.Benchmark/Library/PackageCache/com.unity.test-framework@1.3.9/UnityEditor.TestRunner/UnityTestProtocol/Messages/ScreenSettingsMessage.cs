using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class ScreenSettingsMessage : Message
    {
        public ScreenSettings ScreenSettings;

        public ScreenSettingsMessage()
        {
            type = "ScreenSettings";
        }
    }
}
