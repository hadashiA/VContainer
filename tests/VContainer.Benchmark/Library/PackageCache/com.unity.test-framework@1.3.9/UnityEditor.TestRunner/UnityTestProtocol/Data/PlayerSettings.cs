using System;
using System.Collections.Generic;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    // This class is used for serialization purposes
    // which requires public access to fields and a default empty constructor
    [Serializable]
    internal class PlayerSettings
    {
        public string ScriptingBackend;
        public bool MtRendering;
        public bool GraphicsJobs;
        public bool GpuSkinning;
        public string GraphicsApi;
        public string Batchmode;
        public string StereoRenderingPath;
        public string RenderThreadingMode;
        public string AndroidMinimumSdkVersion;
        public string AndroidTargetSdkVersion;
        public string ScriptingRuntimeVersion;
        public string AndroidTargetArchitecture;
        public bool StripEngineCode;

        public PlayerSettings(
            string scriptingBackend,
            bool gpuSkinning,
            string graphicsApi,
            string batchmode,
            string stereoRenderingPath,
            string renderThreadingMode,
            string androidTargetSdkVersion,
            string androidMinimumSdkVersion = "",
            bool graphicsJobs = false,
            bool mtRendering = false
            )
        {
            ScriptingBackend = scriptingBackend;
            GpuSkinning = gpuSkinning;
            GraphicsApi = graphicsApi;
            Batchmode = batchmode;
            StereoRenderingPath = stereoRenderingPath;
            RenderThreadingMode = renderThreadingMode;
            AndroidTargetSdkVersion = androidTargetSdkVersion;
            AndroidMinimumSdkVersion = androidMinimumSdkVersion;
            GraphicsJobs = graphicsJobs;
            MtRendering = mtRendering;
        }

        public PlayerSettings() { }
    }
}
