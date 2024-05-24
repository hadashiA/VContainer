using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEditor.TestTools.TestRunner
{
    /// <summary>
    /// Handles deserialization of TestSettings from a provided json file path.
    /// </summary>
    internal class TestSettingsDeserializer : ITestSettingsDeserializer
    {
        private static readonly SettingsMap[] s_SettingsMapping =
        {
            new SettingsMap<ScriptingImplementation>("scriptingBackend", (settings, value) => settings.scriptingBackend = value),
            new SettingsMap<string>("architecture", (settings, value) => settings.Architecture = value),
            new SettingsMap<ApiCompatibilityLevel>("apiProfile", (settings, value) => settings.apiProfile = value),
            new SettingsMap<bool>("appleEnableAutomaticSigning", (settings, value) => settings.appleEnableAutomaticSigning = value),
            new SettingsMap<string>("appleDeveloperTeamID", (settings, value) => settings.appleDeveloperTeamID = value),
            new SettingsMap<ProvisioningProfileType>("iOSManualProvisioningProfileType", (settings, value) => settings.iOSManualProvisioningProfileType = value),
            new SettingsMap<string>("iOSManualProvisioningProfileID", (settings, value) => settings.iOSManualProvisioningProfileID = value),
            new SettingsMap<string>("iOSTargetSDK", (settings, value) => settings.iOSTargetSDK = value),
            new SettingsMap<ProvisioningProfileType>("tvOSManualProvisioningProfileType", (settings, value) => settings.tvOSManualProvisioningProfileType = value),
            new SettingsMap<string>("tvOSManualProvisioningProfileID", (settings, value) => settings.tvOSManualProvisioningProfileID = value),
            new SettingsMap<string>("tvOSTargetSDK", (settings, value) => settings.tvOSTargetSDK = value),
            new SettingsMap<string>("playerGraphicsAPI", (settings, value) =>
            {
                settings.autoGraphicsAPIs = false;
                settings.playerGraphicsAPIs = new[] {value};
            }),
            new SettingsMap<bool>("androidBuildAppBundle", (settings, value) =>
            {
                settings.androidBuildAppBundle = value;
            }),
            new SettingsMap<List<object>>("ignoreTests", (settings, list) =>
            {
                settings.ignoreTests = list.Select(item =>
                {
                    var dictionary = (Dictionary<string, object>)item;
                    if (dictionary.ContainsKey("test") && dictionary.ContainsKey("ignoreComment"))
                    {
                        return new IgnoreTest()
                        {
                            test = dictionary["test"] as string,
                            ignoreComment = dictionary["ignoreComment"] as string
                        };
                    }

                    throw new Exception("Wrong format for ignore test. Expected \"test\" and \"ignoreComment\".");
                }).ToArray();
            }),
            new SettingsMap<Dictionary<string, object>>("featureFlags", (settings, dictionary) =>
            {
                var converted = dictionary.ToDictionary(pair => pair.Key, pair => (bool)pair.Value);
                var featureFlags = new FeatureFlags();
                if (converted.ContainsKey("fileCleanUpCheck"))
                {
                    featureFlags.fileCleanUpCheck = converted["fileCleanUpCheck"];
                }
                if (converted.ContainsKey("strictDomainReload"))
                {
                    featureFlags.strictDomainReload = converted["strictDomainReload"];
                }
                if (converted.ContainsKey("requiresSplashScreen"))
                {
                    featureFlags.requiresSplashScreen = converted["requiresSplashScreen"];
                }

                settings.featureFlags = featureFlags;
            }),
#if UNITY_2023_2_OR_NEWER
            new SettingsMap<WebGLClientBrowserType>("webGLClientBrowserType", (settings, value) => settings.webGLClientBrowserType = value),
            new SettingsMap<string>("webGLClientBrowserPath", (settings, value) => settings.webGLClientBrowserPath = value),
#endif
        };

        private readonly Func<ITestSettings> m_TestSettingsFactory;
        public TestSettingsDeserializer(Func<ITestSettings> testSettingsFactory)
        {
            m_TestSettingsFactory = testSettingsFactory;
        }

        public ITestSettings GetSettingsFromJsonFile(string jsonFilePath)
        {
            var text = File.ReadAllText(jsonFilePath);
            var settingsDictionary = Json.Deserialize(text) as Dictionary<string, object>;

            var testSettings = m_TestSettingsFactory();
            if (settingsDictionary == null)
            {
                return testSettings;
            }

            foreach (var settingsMap in s_SettingsMapping)
            {
                if (!settingsDictionary.ContainsKey(settingsMap.Key))
                {
                    continue;
                }

                if (settingsMap.Type.IsEnum)
                {
                    SetEnumValue(settingsMap.Key, settingsDictionary[settingsMap.Key], settingsMap.Type, value => settingsMap.ApplyToSettings(testSettings, value));
                }
                else
                {
                    SetValue(settingsMap.Key, settingsDictionary[settingsMap.Key], settingsMap.Type, value => settingsMap.ApplyToSettings(testSettings, value));
                }
            }

            return testSettings;
        }

        private abstract class SettingsMap
        {
            public string Key { get; }
            public Type Type { get; }
            protected SettingsMap(string key, Type type)
            {
                Key = key;
                Type = type;
            }

            public abstract void ApplyToSettings(ITestSettings settings, object value);
        }

        private class SettingsMap<T> : SettingsMap
        {
            private Action<ITestSettings, T> m_Setter;
            public SettingsMap(string key, Action<ITestSettings, T> setter) : base(key, typeof(T))
            {
                m_Setter = setter;
            }

            public override void ApplyToSettings(ITestSettings settings, object value)
            {
                m_Setter(settings, (T)value);
            }
        }

        private static void SetEnumValue(string key, object value, Type type, Action<object> setter)
        {
            object enumValue;
            if (TryGetEnum(value as string, type, out enumValue))
            {
                setter(enumValue);
                return;
            }

            var acceptedValues = string.Join(", ", Enum.GetValues(type).OfType<object>().Select(val => val.ToString()).ToArray());

            Debug.LogFormat("Could not convert '{0}' argument '{1}' to a valid {2}. Accepted values: {3}.", key, value, type.Name, acceptedValues);
        }

        private static bool TryGetEnum(string value, Type type, out object enumValue)
        {
            try
            {
                enumValue = Enum.Parse(type, value, true);
                return true;
            }
            catch (Exception)
            {
                enumValue = null;
                return false;
            }
        }

        private static void SetValue(string key, object value, Type type, Action<object> setter)
        {
            if (type.IsInstanceOfType(value))
            {
                setter(value);
                return;
            }

            Debug.LogFormat("Could not convert '{0}' argument '{1}' to a valid {2}.", key, value, type.Name);
        }
    }
}
