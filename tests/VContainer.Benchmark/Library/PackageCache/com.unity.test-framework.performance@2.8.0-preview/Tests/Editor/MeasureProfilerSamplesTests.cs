using System.Collections;
using System.Threading;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

public class MeasureProfilerSamplesTests
{
    [Test, Performance]
    public void MeasureProfilerSamples_WithNoSamples_DoesNotRecordSampleGroups()
    {
        LogAssert.NoUnexpectedReceived();
        using (Measure.ProfilerMarkers(new string[0]))
        {
        }

        var result = PerformanceTest.Active;
        Assert.AreEqual(0, result.SampleGroups.Count);
    }

    [Test, Performance]
    public void MeasureProfilerSamples_SingleFrame_RecordsSample()
    {
        using (Measure.ProfilerMarkers("TestMarker"))
        {
            CreatePerformanceMarker("TestMarker", 1);
        }
        
        var result = PerformanceTest.Active;
        Assert.AreEqual(1, result.SampleGroups.Count);
        Assert.AreEqual(1, result.SampleGroups[0].Samples.Count);
        Assert.Greater(result.SampleGroups[0].Samples[0], 0);
    }
    
    [Test, Performance]
    public void MeasureProfilerSamples_strings_SingleFrame_RecordsSample()
    {
        using (Measure.ProfilerMarkers("TestMarker"))
        {
            CreatePerformanceMarker("TestMarker", 1);
        }
        
        var result = PerformanceTest.Active;
        Assert.AreEqual(1, result.SampleGroups.Count);
        Assert.AreEqual(1, result.SampleGroups[0].Samples.Count);
        Assert.Greater(result.SampleGroups[0].Samples[0], 0);
    }

    [Test, Performance]
    public void MeasureProfilerSamples_WhenNewScene_RecordsSample()
    {
        using (Measure.ProfilerMarkers("TestMarker"))
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            CreatePerformanceMarker("TestMarker", 1);
        }

        var result = PerformanceTest.Active;
        Assert.AreEqual(1, result.SampleGroups.Count);
        Assert.AreEqual(1, result.SampleGroups[0].Samples.Count);
        Assert.Greater(result.SampleGroups[0].Samples[0], 0);
    }

    [UnityTest, Performance, Ignore("Not supported yet")]
    public IEnumerator MeasureProfilerSamples_ManyFrames_RecordsSamples()
    {
        yield return null;
        yield return null;
        using (Measure.ProfilerMarkers("TestMarker"))
        {
            yield return null;
            CreatePerformanceMarker("TestMarker", 1);
            yield return null;            
            CreatePerformanceMarker("TestMarker", 1);
            yield return null;            
            CreatePerformanceMarker("TestMarker", 1);
            yield return null;
            CreatePerformanceMarker("TestMarker", 1);
        }
        
        var result = PerformanceTest.Active;
        Assert.AreEqual(1, result.SampleGroups.Count);
        Assert.AreEqual(4, result.SampleGroups[0].Samples.Count);
        Assert.Greater(result.SampleGroups[0].Samples[0], 0);
        Assert.Greater(result.SampleGroups[0].Samples[1], 0);
        Assert.Greater(result.SampleGroups[0].Samples[2], 0);
        Assert.Greater(result.SampleGroups[0].Samples[3], 0);
    }
    
    [UnityTest, Performance, Ignore("Not supported")]
    public IEnumerator MeasureProfilerSamples_string_ManyFrames_RecordsSamples()
    {
        using (Measure.ProfilerMarkers("TestMarker"))
        {
            CreatePerformanceMarker("TestMarker", 1);
            yield return null;            
            CreatePerformanceMarker("TestMarker", 1);
            yield return null;            
            CreatePerformanceMarker("TestMarker", 1);
            yield return null;
            CreatePerformanceMarker("TestMarker", 1);
            yield return null;
        }
        
        var result = PerformanceTest.Active;
        Assert.AreEqual(1, result.SampleGroups.Count);
        Assert.AreEqual(4, result.SampleGroups[0].Samples.Count);
        Assert.Greater(result.SampleGroups[0].Samples[0], 0);
        Assert.Greater(result.SampleGroups[0].Samples[1], 0);
        Assert.Greater(result.SampleGroups[0].Samples[2], 0);
        Assert.Greater(result.SampleGroups[0].Samples[3], 0);
    }

    internal static void CreatePerformanceMarker(string name, int sleep)
    {
        var marker = CustomSampler.Create(name);
        marker.Begin();
        Thread.Sleep(sleep);
        marker.End();
    }
}
