using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;

// https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Embed-link.xml-in-UPM-package
public class Linker : IUnityLinkerProcessor
{
    public int callbackOrder { get; }

    public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
    {
        return Path.GetFullPath("Packages/com.gustavopsantos.reflex/Resources/link.xml");
    }

    public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
    {
    }

    public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
    {
    }
}