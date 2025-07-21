#if UNITY_2022_2_OR_NEWER
using System;
#endif
using UnityEditor.Build;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal static class UtpMessageBuilder
    {
        internal static ScreenSettingsMessage BuildScreenSettings()
        {
#if UNITY_2022_2_OR_NEWER
            // casting to int and rounding to ensure backwards compatibility with older package versions
            var screenRefreshRate = (int)Math.Round(Screen.currentResolution.refreshRateRatio.value);
#else
            var screenRefreshRate = Screen.currentResolution.refreshRate;
#endif

            var screenSettingsMessage = new ScreenSettingsMessage()
            {
                ScreenSettings = new ScreenSettings(Screen.currentResolution.width, Screen.currentResolution.height, screenRefreshRate, Screen.fullScreen)
            };

            return screenSettingsMessage;
        }

        public static QualitySettingsMessage BuildQualitySettings()
        {
            var qualitySettingsMessage = new QualitySettingsMessage()
            {
                QualitySettings = new QualitySettings(
                    UnityEngine.QualitySettings.vSyncCount,
                    UnityEngine.QualitySettings.antiAliasing,
                    UnityEngine.QualitySettings.activeColorSpace.ToString(),
                    UnityEngine.QualitySettings.anisotropicFiltering.ToString(),
                    UnityEngine.QualitySettings.skinWeights.ToString()
                ),
            };
            return qualitySettingsMessage;
        }

        internal static PlayerSystemInfoMessage BuildPlayerSystemInfo()
        {
            var xrDevice = string.Empty;
            var XrModel = string.Empty;
#if ENABLE_XR
            xrDevice = UnityEngine.XR.XRSettings.loadedDeviceName;
            XrModel = UnityEngine.XR.XRDevice.model;
#endif

            var playerSystemInfoMessage = new PlayerSystemInfoMessage()
            {
                PlayerSystemInfo = new PlayerSystemInfo(
                    SystemInfo.operatingSystem,
                    SystemInfo.deviceModel,
                    SystemInfo.deviceName,
                    SystemInfo.processorType,
                    SystemInfo.processorCount,
                    SystemInfo.graphicsDeviceName,
                    SystemInfo.systemMemorySize,
                    XrModel,
                    xrDevice
                ),
            };
            return playerSystemInfoMessage;
        }

        internal static PlayerSettingsMessage BuildPlayerSettings()
        {
            var scriptingBackend = string.Empty;
#if UNITY_2021_2_OR_NEWER
            scriptingBackend =
                UnityEditor.PlayerSettings.GetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup)).ToString();
#else
            scriptingBackend = UnityEditor.PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup).ToString();
#endif

            var playerSettingsMessage = new PlayerSettingsMessage()
            {
                PlayerSettings = new PlayerSettings(
                    scriptingBackend,
                    UnityEditor.PlayerSettings.gpuSkinning,
                    string.Empty,
                    UnityEditorInternal.InternalEditorUtility.inBatchMode.ToString(),
                    UnityEditor.PlayerSettings.stereoRenderingPath.ToString(),
                    UnityEditor.PlayerSettings.graphicsJobs ? "GraphicsJobs" : UnityEditor.PlayerSettings.MTRendering ? "MultiThreaded" : "SingleThreaded",
                    UnityEditor.PlayerSettings.Android.targetSdkVersion.ToString(),
                    UnityEditor.PlayerSettings.Android.minSdkVersion.ToString(),
                    UnityEditor.PlayerSettings.graphicsJobs,
                    UnityEditor.PlayerSettings.MTRendering
                ),
            };

            return playerSettingsMessage;
        }

        internal static BuildSettingsMessage BuildBuildSettings()
        {
            var buildSettingsMessage = new BuildSettingsMessage()
            {
                BuildSettings = new BuildSettings(
                    Application.platform.ToString(),
                    EditorUserBuildSettings.activeBuildTarget.ToString(),
                    EditorUserBuildSettings.development,
                    EditorUserBuildSettings.androidBuildSystem.ToString()
                )
            };

            return buildSettingsMessage;
        }
    }
}
