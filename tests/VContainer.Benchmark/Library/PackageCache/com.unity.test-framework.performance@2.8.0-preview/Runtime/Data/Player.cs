using System;

namespace Unity.PerformanceTesting.Data
{
    [Serializable]
    public class Player
    {
        [RequiredMember]
        public bool Development;
        [RequiredMember]
        public int ScreenWidth;
        [RequiredMember]
        public int ScreenHeight;
        [RequiredMember]
        public int ScreenRefreshRate;
        [RequiredMember]
        public bool Fullscreen;
        [RequiredMember]
        public int Vsync;
        [RequiredMember]
        public int AntiAliasing;
        [RequiredMember]
        public bool Batchmode;
        [RequiredMember]
        public string RenderThreadingMode;
        [RequiredMember]
        public bool GpuSkinning;
        
        // enum to string converter is stripped out in il2cpp builds
        // and numbers are too unreadable so parsing to strings
        [RequiredMember]
        public string Platform;
        [RequiredMember]
        public string ColorSpace;
        [RequiredMember]
        public string AnisotropicFiltering;
        [RequiredMember]
        public string BlendWeights;
        [RequiredMember]
        public string GraphicsApi;
        
        // strings because their enums are editor only
        [RequiredMember]
        public string ScriptingBackend;
        [RequiredMember]
        public string AndroidTargetSdkVersion;
        [RequiredMember]
        public string AndroidBuildSystem;
        [RequiredMember]
        public string BuildTarget;
        [RequiredMember]
        public string StereoRenderingPath;
    }
}
