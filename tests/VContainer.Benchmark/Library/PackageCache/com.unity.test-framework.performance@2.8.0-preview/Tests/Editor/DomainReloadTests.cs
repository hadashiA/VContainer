using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

public class DomainReloadTests
{
    const string k_SampleGroupName = "TEST_SAMPLEGROUP";

    void MeasureSample(double sample)
    {
        var sg = new SampleGroup(k_SampleGroupName, SampleUnit.Undefined);
        Measure.Custom(sg, sample);
    }

    public void AssertSample(double expected)
    {
        Assert.AreEqual(PerformanceTest.Active.SampleGroups.Count, 1);
        var sampleGroup = PerformanceTest.Active.SampleGroups.First(s => s.Name == k_SampleGroupName);
        var sample = sampleGroup.Samples.First();
        Assert.AreEqual(expected, sample, Mathf.Epsilon);
    }

    [UnityTest, Performance]
    public IEnumerator Measure_Survives_EnterPlaymode()
    {
        MeasureSample(111D);

        yield return new EnterPlayMode();
        yield return new ExitPlayMode();

        AssertSample(111D);
    }

    [UnityTest, Performance]
    public IEnumerator Measure_Survives_ScriptCompliation()
    {
        MeasureSample(222D);

        File.WriteAllText("Assets/test.cs", "class fake{}");
        yield return new RecompileScripts(true);
        AssetDatabase.DeleteAsset("Assets/test.cs");

        AssertSample(222D);
    }

    [UnityTest, Performance]
    public IEnumerator Measure_Survives_DomainReloadWithoutCompilation()
    {
        MeasureSample(333D);

#if UNITY_2019_3_OR_NEWER
        EditorUtility.RequestScriptReload();
#else
            InternalEditorUtility.RequestScriptReload();
#endif
        yield return new RecompileScripts(false);
        yield return null;

        AssertSample(333D);
    }
}
