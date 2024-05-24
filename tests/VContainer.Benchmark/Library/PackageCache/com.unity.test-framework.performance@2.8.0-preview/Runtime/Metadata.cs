using System;
using Unity.PerformanceTesting.Data;
using Unity.PerformanceTesting.Runtime;
using UnityEngine;
#if USE_CUSTOM_METADATA
using com.unity.test.metadatamanager;
#endif


namespace Unity.PerformanceTesting
{
    public class Metadata
    {
        public static Run GetPerformanceTestRun()
        {
            try
            {
                var runResource = Resources.Load<TextAsset>(Utils.TestRunInfo.Replace(".json", ""));
                var json = Application.isEditor ? PlayerPrefs.GetString(Utils.PlayerPrefKeyRunJSON) : runResource.text;
                var run = JsonUtility.FromJson<Run>(json);
                return run;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }

        public static Hardware GetHardware()
        {
            return new Hardware
            {
                OperatingSystem = SystemInfo.operatingSystem,
                DeviceModel = SystemInfo.deviceModel,
                DeviceName = SystemInfo.deviceName,
                ProcessorType = SystemInfo.processorType,
                ProcessorCount = SystemInfo.processorCount,
                GraphicsDeviceName = SystemInfo.graphicsDeviceName,
                SystemMemorySizeMB = SystemInfo.systemMemorySize
            };
        }

        public static void SetPlayerSettings(Run run)
        {
            run.Player.Vsync = QualitySettings.vSyncCount;
            run.Player.AntiAliasing = QualitySettings.antiAliasing;
            run.Player.ColorSpace = QualitySettings.activeColorSpace.ToString();
            run.Player.AnisotropicFiltering = QualitySettings.anisotropicFiltering.ToString();
            run.Player.BlendWeights = QualitySettings.skinWeights.ToString();
            run.Player.ScreenRefreshRate = Screen.currentResolution.refreshRate;
            run.Player.ScreenWidth = Screen.currentResolution.width;
            run.Player.ScreenHeight = Screen.currentResolution.height;
            run.Player.Fullscreen = Screen.fullScreen;
            run.Player.Batchmode = Application.isBatchMode;
            run.Player.Development = Application.isEditor ? true : Debug.isDebugBuild;
            run.Player.Platform = Application.platform.ToString();
            run.Player.GraphicsApi = SystemInfo.graphicsDeviceType.ToString();
        }

        public static Run GetFromResources()
        {
            var run = GetPerformanceTestRun();
            SetRuntimeSettings(run);

            return run;
        }

        public static void SetRuntimeSettings(Run run)
        {
            run.Hardware = GetHardware();
            SetPlayerSettings(run);
            run.TestSuite = Application.isPlaying ? "Playmode" : "Editmode";
#if USE_CUSTOM_METADATA
            SetCustomMetadata(run);
#endif
        }

#if USE_CUSTOM_METADATA
        private static void SetCustomMetadata(Run run)
        {
            var customMetadataManager = new CustomMetadataManager(run.Dependencies);
            // This field is historically not used so we can safely store additional string delimited
            // metadata here, then parse the metadata values out on the SQL side to give us access
            // to additional metadata that would normally require a schema change, or a property back field
            run.Player.AndroidTargetSdkVersion = customMetadataManager.GetCustomMetadata();
        }
#endif
    }
}