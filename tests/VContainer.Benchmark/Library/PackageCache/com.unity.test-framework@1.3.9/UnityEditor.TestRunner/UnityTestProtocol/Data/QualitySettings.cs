using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    // This class is used for serialization purposes
    // which requires public access to fields and a default empty constructor
    [Serializable]
    internal class QualitySettings
    {
        public QualitySettings(int vsync, int antiAliasing, string colorSpace, string anisotropicFiltering, string blendWeights)
        {
            Vsync = vsync;
            AntiAliasing = antiAliasing;
            ColorSpace = colorSpace;
            AnisotropicFiltering = anisotropicFiltering;
            BlendWeights = blendWeights;
        }

        public QualitySettings(){}

        public int Vsync;
        public int AntiAliasing;
        public string ColorSpace;
        public string AnisotropicFiltering;
        public string BlendWeights;
    }
}
