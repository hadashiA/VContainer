using System;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEditor.TestTools.TestRunner
{
    internal interface ITestSettings : IDisposable
    {
        ScriptingImplementation? scriptingBackend { get; set; }

        string Architecture { get; set; }

        ApiCompatibilityLevel? apiProfile { get; set; }

        bool? appleEnableAutomaticSigning { get; set; }
        string appleDeveloperTeamID { get; set; }
        ProvisioningProfileType? iOSManualProvisioningProfileType { get; set; }
        string iOSManualProvisioningProfileID { get; set; }
        string iOSTargetSDK { get; set; }
        ProvisioningProfileType? tvOSManualProvisioningProfileType { get; set; }
        string tvOSManualProvisioningProfileID { get; set; }
        string tvOSTargetSDK { get; set; }
        string[] playerGraphicsAPIs { get; set; }
        bool autoGraphicsAPIs { get; set; }
        bool? androidBuildAppBundle { get; set; }
        IgnoreTest[] ignoreTests { get; set; }
        FeatureFlags featureFlags { get; set; }
#if UNITY_2023_2_OR_NEWER
        WebGLClientBrowserType? webGLClientBrowserType { get; set; }
        string webGLClientBrowserPath { get; set; }
#endif

        void SetupProjectParameters();
    }
}
