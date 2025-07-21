using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

public class FrametimeOverloadTests
{
    [UnityTest, Performance]
    public IEnumerator MeasureFrames_WhenYielding_RecordsFrametimes()
    {
        using (Measure.Frames().Scope())
        {
            yield return null;
            yield return null;
        }
        
        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(2, test.SampleGroups[0].Samples.Count);
        Assert.IsTrue(AllSamplesHigherThan0(test));
    }

    [UnityTest, Performance]
    public IEnumerator MeasureFrames_WithSampleGroups_ConvertToPassedSampleUnit()
    {
        var sg = new SampleGroup("TEST", SampleUnit.Second);
        using (Measure.Frames().Scope(sg))
        {
            yield return null;
            yield return null;
        }
        
        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(test.SampleGroups[0].Samples.Count, 2);
        Assert.AreEqual(sg.Name, test.SampleGroups[0].Name);
        Assert.AreEqual(sg.Unit, test.SampleGroups[0].Unit);
        Assert.IsTrue(AllSamplesHigherThan0(test));
    }
    
    [UnityTest, Performance]
    public IEnumerator MeasureFrames_WithNoParams_RecordsSampleGroup()
    {
        yield return Measure.Frames().Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.Greater(test.SampleGroups[0].Samples.Count, 0);
        Assert.IsTrue(AllSamplesHigherThan0(test));
    }
    
    [UnityTest, Performance]
    public IEnumerator MeasureFrames_WithCustomDefinition_AssignsDefinition()
    {
        yield return Measure.Frames()
            .SampleGroup("TIME")
            .Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual("TIME", test.SampleGroups[0].Name);
        Assert.AreEqual(SampleUnit.Millisecond, test.SampleGroups[0].Unit);
    }
    
    [UnityTest, Performance]
    public IEnumerator MeasureFrames_WithRecordingDisabled_RecordsNoSampleGroups()
    {
        yield return Measure.Frames().DontRecordFrametime().Run();
        
        var test = PerformanceTest.Active;
        Assert.AreEqual(0, test.SampleGroups.Count);
    }
    
    [UnityTest, Performance]
    public IEnumerator MeasureFrames_WithProfilerMarkers_RecordsMarkers()
    {
        var obj = new GameObject("MeasureFrames_WithRecordingDisabled_RecordsNoSampleGroups");
        obj.AddComponent<CreateMarkerOnUpdate>();
        
        yield return Measure.Frames()
            .ProfilerMarkers("TEST_MARKER")
            .DontRecordFrametime()
            .Run();
        
        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual("TEST_MARKER", test.SampleGroups[0].Name);
        Assert.AreEqual(SampleUnit.Nanosecond, test.SampleGroups[0].Unit);
        Assert.IsTrue(AllSamplesHigherThan0(test));
    }
    
    [UnityTest, Performance]
    public IEnumerator MeasureFrames_WithProfilerMarkers_string_RecordsMarkers()
    {
        var obj = new GameObject("MeasureFrames_WithRecordingDisabled_RecordsNoSampleGroups");
        obj.AddComponent<CreateMarkerOnUpdate>();
        
        yield return Measure.Frames()
            .ProfilerMarkers("TEST_MARKER")
            .DontRecordFrametime()
            .Run();
        
        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual("TEST_MARKER", test.SampleGroups[0].Name);
        Assert.AreEqual(SampleUnit.Nanosecond, test.SampleGroups[0].Unit);
        Assert.IsTrue(AllSamplesHigherThan0(test));
    }

    [UnityTest, Performance]
    public IEnumerator MeasureFrames_WithWarmupAndExecutions_RecordsSpecifiedAmount()
    {
        yield return Measure.Frames()
            .WarmupCount(10)
            .MeasurementCount(10)
            .Run();
        
        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(10, test.SampleGroups[0].Samples.Count);
    }
    
    [UnityTest, Performance]
    public IEnumerator MeasureFrames_WithWarmup_ThrowsException()
    {
        LogAssert.Expect(LogType.Error, new Regex(".+frames measurement"));
        yield return Measure.Frames()
            .WarmupCount(10)
            .Run();
        
        var test = PerformanceTest.Active;
        Assert.AreEqual(0, test.SampleGroups.Count);
    }

    [UnityTest, Performance]
    public IEnumerator MeasureFrame_WithSampleGroupAndProfilerMarker_RecordSpecifiedMarkers_WithCorrectSampleUnit()
    {
        var sg = new SampleGroup("TEST", SampleUnit.Second);
        var sgMarker = new SampleGroup("TEST_MARKER", SampleUnit.Nanosecond);

        yield return Measure.Frames().SampleGroup(sg).ProfilerMarkers(sgMarker).Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(2, test.SampleGroups.Count);
        Assert.AreEqual(sg.Unit,test.SampleGroups[0].Unit);
        Assert.AreEqual(sg.Name,test.SampleGroups[0].Name);
        Assert.AreEqual(sgMarker.Unit,test.SampleGroups[1].Unit);
        Assert.AreEqual(sgMarker.Name,test.SampleGroups[1].Name);
        Assert.Greater(test.SampleGroups[0].Samples.Count, 0);
        Assert.Greater(test.SampleGroups[1].Samples.Count, 0);
    }

    [UnityTest, Performance]
    public IEnumerator MeasureFrame_WithEmptySampleGroupsAsProfilerMarker_RecordsDefaultTimeMarkerInMilliseconds()
    {
        yield return Measure.Frames().ProfilerMarkers(new SampleGroup[]{}).Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(SampleUnit.Millisecond,test.SampleGroups[0].Unit);
        Assert.AreEqual("FrameTime",test.SampleGroups[0].Name);
        Assert.Greater(test.SampleGroups[0].Samples.Count, 0);
    }

    [UnityTest, Performance]
    public IEnumerator MeasureFrame_WithMultipleSampleGroups_RecordSpecifiedMarkers_WithCorrectSampleUnits()
    {
        var sg = new SampleGroup("TEST", SampleUnit.Microsecond);

        var sampleGroups = new []{
            new SampleGroup("TEST_MARKER", SampleUnit.Second),
            new SampleGroup("TEST_MARKER1", SampleUnit.Nanosecond)
        };
        
        yield return Measure.Frames().SampleGroup(sg).ProfilerMarkers(sampleGroups).Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(3, test.SampleGroups.Count);
        Assert.AreEqual(sg.Unit,test.SampleGroups[0].Unit);
        Assert.AreEqual(sg.Name,test.SampleGroups[0].Name);
        Assert.AreEqual(sampleGroups[0].Unit,test.SampleGroups[1].Unit);
        Assert.AreEqual(sampleGroups[0].Name,test.SampleGroups[1].Name);
        Assert.AreEqual(sampleGroups[1].Name,test.SampleGroups[2].Name);
        Assert.AreEqual(sampleGroups[1].Unit, test.SampleGroups[2].Unit);

        Assert.Greater(test.SampleGroups[0].Samples.Count, 0);
        Assert.Greater(test.SampleGroups[1].Samples.Count, 0);
        Assert.Greater(test.SampleGroups[2].Samples.Count, 0);
    }

    private static bool AllSamplesHigherThan0(PerformanceTest test)
    {
        foreach (var sampleGroup in test.SampleGroups)
        {
            foreach (var sample in sampleGroup.Samples)
            {
                if (sample <= 0) return false;
            }
        }

        return true;
    }

    public class CreateMarkerOnUpdate : MonoBehaviour
    {
        private CustomSampler m_CustomSampler;

        private void OnEnable()
        {
            m_CustomSampler = CustomSampler.Create("TEST_MARKER");
        }

        private void Update()
        {
            m_CustomSampler.Begin();
            Thread.Sleep(1);
            m_CustomSampler.End();
        }
    }
}
