using System.Collections;
using System.Threading;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

public class MeasureProfilerSamplesTests
{
    [Test, Performance]
    public void MeasureProfilerSamples_string_WithNoSamples_DoesNotRecordSampleGroups()
    {
        LogAssert.NoUnexpectedReceived();
        using (Measure.ProfilerMarkers(new string[0]))
        {
        }

        var result = PerformanceTest.Active;
        Assert.AreEqual(0, result.SampleGroups.Count);
    }

    [Test, Performance]
    public void MeasureProfilerSamples_string_SingleFrame_RecordsSample()
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

    [UnityTest, Performance]
    public IEnumerator MeasureProfilerSamples_ManyFrames_RecordsSamples()
    {
        using (Measure.ProfilerMarkers(("TestMarker")))
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

    [UnityTest, Performance]
    public IEnumerator MeasureProfilerSamples_string_ManyFrames_RecordsSamples()
    {
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

    [Test, Performance]
    public void MeasureProfilerSamples_WithNullProfilerMarkers_DoesNotRecordSampleGroups()
    {
        string[] s = null;
        using (Measure.ProfilerMarkers(s))
        {
        }
        var result = PerformanceTest.Active;
        Assert.AreEqual(0, result.SampleGroups.Count);    
    }

    [Test, Performance]
    public void MeasureProfilerSamples_WithNoGroupSamples_DoesNotRecordSampleGroups()
    {
        using (Measure.ProfilerMarkers(new SampleGroup[]{}))
        {
        }
    
        var result = PerformanceTest.Active;
        Assert.AreEqual(0, result.SampleGroups.Count);
    }
    
    [Test, Performance]
    public void MeasureProfilerSamples_SampleGroup_SingleFrame_RecordsSample()
    {
        var sg = new SampleGroup("TestMarker", SampleUnit.Second);
        using (Measure.ProfilerMarkers(sg))
        {
            CreatePerformanceMarker(sg.Name, 1);
        }
        
        var result = PerformanceTest.Active;
        Assert.AreEqual(1, result.SampleGroups.Count);
        Assert.AreEqual(1, result.SampleGroups[0].Samples.Count);
        Assert.AreEqual(sg.Unit, result.SampleGroups[0].Unit);
        Assert.AreEqual(sg.Name, result.SampleGroups[0].Name); 
    }
    
    [UnityTest, Performance]
    public IEnumerator MeasureProfilerSamples_SampleGroup_ManyFrames_RecordsSamples()
    {
        var sampleGroups = new []{
            new SampleGroup("Test", SampleUnit.Second), 
            new SampleGroup("Test1", SampleUnit.Nanosecond),
            new SampleGroup("Test2", SampleUnit.Microsecond)
        };

        using (Measure.ProfilerMarkers(sampleGroups))
        {
            yield return null;
            CreatePerformanceMarker("Test", 1);
            yield return null;            
            CreatePerformanceMarker("Test1", 1);
            yield return null;            
            CreatePerformanceMarker("Test2", 1);
            yield return null;
            CreatePerformanceMarker("Test2", 1);
        }
        
        var result = PerformanceTest.Active;
        Assert.AreEqual(sampleGroups.Length, result.SampleGroups.Count);
        Assert.AreEqual(4, result.SampleGroups[0].Samples.Count);
        Assert.AreEqual(sampleGroups[0].Name, result.SampleGroups[0].Name);
        Assert.AreEqual(sampleGroups[1].Name, result.SampleGroups[1].Name);
        Assert.AreEqual(sampleGroups[2].Name, result.SampleGroups[2].Name);
        Assert.AreEqual(sampleGroups[0].Unit, result.SampleGroups[0].Unit);
        Assert.AreEqual(sampleGroups[1].Unit, result.SampleGroups[1].Unit);
        Assert.AreEqual(sampleGroups[2].Unit, result.SampleGroups[2].Unit);
    }

    private static void CreatePerformanceMarker(string name, int sleep)
    {
        var marker = CustomSampler.Create(name);
        marker.Begin();
        Thread.Sleep(sleep);
        marker.End();
    }
}
