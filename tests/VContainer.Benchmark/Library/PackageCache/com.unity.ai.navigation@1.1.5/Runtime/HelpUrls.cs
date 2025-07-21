namespace Unity.AI.Navigation
{
    /// <summary>
    /// Helper for compile-time constant strings for the [HelpURL](xref:UnityEngine.HelpURLAttribute) attribute.
    /// </summary>
    static class HelpUrls
    {
        const string Version = "1.1";

        const string BaseUrl = "https://docs.unity3d.com/Packages/com.unity.ai.navigation@" + Version;

        internal const string Manual = BaseUrl + "/manual/";

        internal const string Api = BaseUrl + "/api/";
    }
}
