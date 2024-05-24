using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    // This class is used for serialization purposes
    // which requires public access to fields and a default empty constructor
    [Serializable]
    internal class ScreenSettings
    {
        public ScreenSettings(int screenWidth, int screenHeight, int screenRefreshRate, bool fullscreen)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            ScreenRefreshRate = screenRefreshRate;
            Fullscreen = fullscreen;
        }

        public ScreenSettings() { }
        public int ScreenWidth;
        public int ScreenHeight;
        public int ScreenRefreshRate;
        public bool Fullscreen;

    }
}
