using System;

namespace UnityEditor.TestTools.TestRunner
{
    internal interface ITestSettingsDeserializer
    {
        ITestSettings GetSettingsFromJsonFile(string jsonFilePath);
    }
}
