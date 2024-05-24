using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.Rendering;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEditor.TestTools.TestRunner
{
    internal class TestSettings : ITestSettings
    {
        private readonly TestSetting[] m_Settings =
        {
#if UNITY_2021_2_OR_NEWER
            new TestSetting<ScriptingImplementation?>(
                settings => settings.scriptingBackend,
                () => PlayerSettings.GetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.activeBuildTargetGroup)),
                implementation => PlayerSettings.SetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.activeBuildTargetGroup), implementation.Value)),
#else
            new TestSetting<ScriptingImplementation?>(
                settings => settings.scriptingBackend,
                () => PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.activeBuildTargetGroup),
                implementation => PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.activeBuildTargetGroup, implementation.Value)),
#endif
            new TestSetting<string>(
                settings => settings.Architecture,
                () => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? PlayerSettings.Android.targetArchitectures.ToString() : null,
                architecture =>
                {
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                    {
                        if (!string.IsNullOrEmpty(architecture))
                        {
                            var targetArchitectures = (AndroidArchitecture)Enum.Parse(typeof(AndroidArchitecture), architecture, true);
                            PlayerSettings.Android.targetArchitectures = targetArchitectures;
                        }
                    }
                }),
#if UNITY_2021_2_OR_NEWER
            new TestSetting<ApiCompatibilityLevel?>(
                settings => settings.apiProfile,
                () => PlayerSettings.GetApiCompatibilityLevel(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.activeBuildTargetGroup)),
                implementation =>
                {
                    if (Enum.IsDefined(typeof(ApiCompatibilityLevel), implementation.Value))
                    {
                        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.activeBuildTargetGroup),
                            implementation.Value);
                    }
                }),
#else
            new TestSetting<ApiCompatibilityLevel?>(
                settings => settings.apiProfile,
                () => PlayerSettings.GetApiCompatibilityLevel(EditorUserBuildSettings.activeBuildTargetGroup),
                implementation =>
                {
                    if (Enum.IsDefined(typeof(ApiCompatibilityLevel), implementation.Value))
                    {
                        PlayerSettings.SetApiCompatibilityLevel(EditorUserBuildSettings.activeBuildTargetGroup,
                            implementation.Value);
                    }
                }),
#endif
            new TestSetting<bool?>(
                settings => settings.appleEnableAutomaticSigning,
                () => PlayerSettings.iOS.appleEnableAutomaticSigning,
                enableAutomaticSigning =>
                {
                    if (enableAutomaticSigning != null)
                        PlayerSettings.iOS.appleEnableAutomaticSigning = enableAutomaticSigning.Value;
                }),
            new TestSetting<string>(
                settings => settings.appleDeveloperTeamID,
                () => PlayerSettings.iOS.appleDeveloperTeamID,
                developerTeam =>
                {
                    if (developerTeam != null)
                        PlayerSettings.iOS.appleDeveloperTeamID = developerTeam;
                }),
            new TestSetting<ProvisioningProfileType?>(
                settings => settings.iOSManualProvisioningProfileType,
                () => PlayerSettings.iOS.iOSManualProvisioningProfileType,
                profileType =>
                {
                    if (profileType != null)
                        PlayerSettings.iOS.iOSManualProvisioningProfileType = profileType.Value;
                }),
            new TestSetting<string>(
                settings => settings.iOSManualProvisioningProfileID,
                () => PlayerSettings.iOS.iOSManualProvisioningProfileID,
                provisioningUUID =>
                {
                    if (provisioningUUID != null)
                        PlayerSettings.iOS.iOSManualProvisioningProfileID = provisioningUUID;
                }),
            new TestSetting<string>(
                settings => settings.iOSTargetSDK,
                () => (PlayerSettings.iOS.sdkVersion).ToString(),
                targetSDK =>
                {
                    if (targetSDK != null)
                    {
                        if (targetSDK == "DeviceSDK")
                            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
                        else if (targetSDK == "SimulatorSDK")
                            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
                    }
                }),
            new TestSetting<ProvisioningProfileType?>(
                settings => settings.tvOSManualProvisioningProfileType,
                () => PlayerSettings.iOS.tvOSManualProvisioningProfileType,
                profileType =>
                {
                    if (profileType != null)
                        PlayerSettings.iOS.tvOSManualProvisioningProfileType = profileType.Value;
                }),
            new TestSetting<string>(
                settings => settings.tvOSManualProvisioningProfileID,
                () => PlayerSettings.iOS.tvOSManualProvisioningProfileID,
                provisioningUUID =>
                {
                    if (provisioningUUID != null)
                        PlayerSettings.iOS.tvOSManualProvisioningProfileID = provisioningUUID;
                }),
            new TestSetting<string>(
                settings => settings.tvOSTargetSDK,
                () => (PlayerSettings.tvOS.sdkVersion).ToString(),
                targetSDK =>
                {
                    if (targetSDK != null)
                    {
                        if (targetSDK == "DeviceSDK" || targetSDK == "Device")
                            PlayerSettings.tvOS.sdkVersion = tvOSSdkVersion.Device;
                        else if (targetSDK == "SimulatorSDK" || targetSDK == "Simulator")
                            PlayerSettings.tvOS.sdkVersion = tvOSSdkVersion.Simulator;
                    }
                }),
            new TestSetting<bool>(
                settings => settings.autoGraphicsAPIs,
                () => PlayerSettings.GetUseDefaultGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget),
                autoGraphicsAPIs =>
                {
                    PlayerSettings.SetUseDefaultGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget, autoGraphicsAPIs);
                }),
            new TestSetting<string[]>(
                settings => settings.playerGraphicsAPIs,
                () => PlayerSettings.GetGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget).Select(api => api.ToString()).ToArray(),
                playerGraphicsAPIs =>
                {
                    if (playerGraphicsAPIs != null && playerGraphicsAPIs.Length > 0)
                    {
                        var graphicsAPIs = new List<GraphicsDeviceType>();
                        foreach (var graphicsAPI in playerGraphicsAPIs)
                        {
                            if (Enum.TryParse(graphicsAPI, true, out GraphicsDeviceType playerGraphicsAPI))
                                graphicsAPIs.Add(playerGraphicsAPI);
                        }

                        if (graphicsAPIs.Count > 0)
                            PlayerSettings.SetGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget, graphicsAPIs.ToArray());
                    }
                }),
            new TestSetting<bool?>(
                settings => settings.androidBuildAppBundle,
                () => EditorUserBuildSettings.buildAppBundle,
                androidAppBundle =>
                {
                    EditorUserBuildSettings.buildAppBundle = androidAppBundle.Value;
#if UNITY_2023_1_OR_NEWER
                    PlayerSettings.Android.splitApplicationBinary = androidAppBundle.Value;
#else
                    PlayerSettings.Android.useAPKExpansionFiles = androidAppBundle.Value;
#endif
                }),
            new TestSetting<bool?>(
                settings => settings.featureFlags.requiresSplashScreen,
                () => PlayerSettings.SplashScreen.show,
                requiresSplashScreen =>
                {
                    if (requiresSplashScreen != null)
                    {
                        PlayerSettings.SplashScreen.show = requiresSplashScreen.Value;
                    }
                }),
            new TestSetting<bool?>(
                settings => settings.featureFlags.requiresSplashScreen,
                () => PlayerSettings.SplashScreen.showUnityLogo,
                requiresSplashScreen =>
                {
                    if (requiresSplashScreen != null)
                    {
                        PlayerSettings.SplashScreen.showUnityLogo = requiresSplashScreen.Value;
                    }
                }),
#if UNITY_2023_2_OR_NEWER
            new TestSetting<WebGLClientBrowserType?>(
                settings => settings.webGLClientBrowserType,
                () => EditorUserBuildSettings.webGLClientBrowserType,
                browserType =>
                {
                    if (browserType != null)
                        EditorUserBuildSettings.webGLClientBrowserType = browserType.Value;
                }),
            new TestSetting<string>(
                settings => settings.webGLClientBrowserPath,
                () => EditorUserBuildSettings.webGLClientBrowserPath,
                browserPath =>
                {
                    if (!string.IsNullOrEmpty(browserPath))
                        EditorUserBuildSettings.webGLClientBrowserPath = browserPath;
                }),
#endif
        };

        private bool m_Disposed;

        public ScriptingImplementation? scriptingBackend { get; set; }

        public string Architecture { get; set; }

        public ApiCompatibilityLevel? apiProfile { get; set; }

        public bool? appleEnableAutomaticSigning { get; set; }
        public string appleDeveloperTeamID { get; set; }
        public ProvisioningProfileType? iOSManualProvisioningProfileType { get; set; }
        public string iOSManualProvisioningProfileID { get; set; }
        public string iOSTargetSDK { get; set; }
        public ProvisioningProfileType? tvOSManualProvisioningProfileType { get; set; }
        public string tvOSManualProvisioningProfileID { get; set; }
        public string tvOSTargetSDK { get; set; }
        public string[] playerGraphicsAPIs { get; set; }
        public bool autoGraphicsAPIs { get; set; }
        public bool? androidBuildAppBundle { get; set; }
#if UNITY_2023_2_OR_NEWER
        public WebGLClientBrowserType? webGLClientBrowserType { get; set; }
        public string webGLClientBrowserPath { get; set; }
#endif
        public IgnoreTest[] ignoreTests { get; set; }
        public FeatureFlags featureFlags { get; set; } = new FeatureFlags();

        public void Dispose()
        {
            if (!m_Disposed)
            {
                foreach (var testSetting in m_Settings)
                {
                    testSetting.Cleanup();
                }

                m_Disposed = true;
            }
        }

        public void SetupProjectParameters()
        {
            foreach (var testSetting in m_Settings)
            {
                testSetting.Setup(this);
            }
        }

        private abstract class TestSetting
        {
            public abstract void Setup(TestSettings settings);
            public abstract void Cleanup();
        }

        private class TestSetting<T> : TestSetting
        {
            private T m_ValueBeforeSetup;
            private Func<TestSettings, T> m_GetFromSettings;
            private Func<T> m_GetCurrentValue;
            private Action<T> m_SetValue;

            public TestSetting(Func<TestSettings, T> getFromSettings, Func<T> getCurrentValue, Action<T> setValue)
            {
                m_GetFromSettings = getFromSettings;
                m_GetCurrentValue = getCurrentValue;
                m_SetValue = setValue;
            }

            public override void Setup(TestSettings settings)
            {
                m_ValueBeforeSetup = m_GetCurrentValue();
                var newValue = m_GetFromSettings(settings);
                if (newValue != null)
                {
                    m_SetValue(newValue);
                }
            }

            public override void Cleanup()
            {
                m_SetValue(m_ValueBeforeSetup);
            }
        }
    }
}
