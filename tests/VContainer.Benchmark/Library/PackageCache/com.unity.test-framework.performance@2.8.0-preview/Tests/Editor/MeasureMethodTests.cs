using System.Threading;
using Unity.PerformanceTesting.Meters;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.PerformanceTesting.Measurements;

public class MeasureMethodTests
{
    [Test, Performance]
    public void MeasureMethod_With_NoArguments()
    {
        var call_count = 0;
        Measure.Method(() =>
        {
            call_count++;
            Thread.Sleep(1);
        }).Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(MethodMeasurement.k_MeasurementCount, test.SampleGroups[0].Samples.Count);
        Assert.IsTrue(AllSamplesHigherThan0(test));
        Assert.Greater(call_count, 9);
    }

    [Test, Performance]
    public void MeasureMethod_With_MeasurementCount()
    {
        var s_CallCount = 0;
        Measure.Method(() => { s_CallCount++; })
            .MeasurementCount(10)
            .Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(10, test.SampleGroups[0].Samples.Count);
        Assert.AreEqual(10, s_CallCount);
    }

    [Test, Performance]
    public void MeasureMethod_With_MeasurementCountAndSetupCleanup()
    {
        var s_CallCount = 0;
        Measure.Method(() => { s_CallCount++; })
            .MeasurementCount(10)
            .SetUp(() => s_CallCount++)
            .CleanUp(() => s_CallCount++)
            .Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(10, test.SampleGroups[0].Samples.Count);
        Assert.AreEqual(30, s_CallCount);
    }

    [Test, Performance]
    public void MeasureMethod_With_MeasurementCountAndSetup()
    {
        var s_CallCount = 0;
        Measure.Method(() => { })
            .MeasurementCount(10)
            .SetUp(() => s_CallCount++)
            .Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(10, test.SampleGroups[0].Samples.Count);
        Assert.AreEqual(10, s_CallCount);
    }

    [Test, Performance]
    public void MeasureMethod_With_MeasurementCountAndCleanup()
    {
        var s_CallCount = 0;
        Measure.Method(() => { })
            .MeasurementCount(10)
            .CleanUp(() => s_CallCount++)
            .Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(10, test.SampleGroups[0].Samples.Count);
        Assert.AreEqual(10, s_CallCount);
    }

    [Test, Performance]
    public void MeasureMethod_With_MeasurementCountAndIterationCount()
    {
        var watch = new FakeWatch();

        var s_CallCount = 0;
        Measure.Method(() =>
            {
                s_CallCount++;
                watch.Sample = s_CallCount;
            })
            .StopWatch(watch)
            .WarmupCount(10)
            .MeasurementCount(10)
            .IterationsPerMeasurement(5)
            .Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(10, test.SampleGroups[0].Samples.Count);
        Assert.AreEqual(5D,test.SampleGroups[0].Samples[0], 0.1D);
        Assert.AreEqual(100, s_CallCount);
    }

    [Test, Performance]
    public void MeasureMethod_With_GarbageCollectionMarker()
    {
        Measure.Method(() => { }).GC().Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(2, test.SampleGroups.Count);
        Assert.AreEqual("Time.GC()", test.SampleGroups[0].Name);
        Assert.IsTrue(LessThanOne(test.SampleGroups[0]));
    }

    [Test, Performance]
    public void MeasureMethod_With_EmptyProfilerMarkers()
    {
        Measure.Method(() => { }).ProfilerMarkers("empty").Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
    }

    [Test, Performance]
    public void MeasureMethod_With_ValidProfilerMarkers()
    {
        Measure.Method(() => { MeasureProfilerSamplesTests.CreatePerformanceMarker("loop", 1); })
            .ProfilerMarkers("loop").Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(2, test.SampleGroups.Count);
        Assert.AreEqual("Time", test.SampleGroups[0].Name);
        Assert.AreEqual("loop", test.SampleGroups[1].Name);
    }

    [Test, Performance]
    public void MeasureMethod_WithCustomSampleGroupInProfilerMarker_UsesDefaultSampleGroupPlusTheCustomOne_Run()
    {
        var sgMarker = new SampleGroup("TEST_MARKER", SampleUnit.Microsecond);
        Measure.Method(() => {MeasureProfilerSamplesTests.CreatePerformanceMarker("TEST_MARKER", 1);}).ProfilerMarkers(sgMarker).Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(2, test.SampleGroups.Count);
        Assert.AreEqual(SampleUnit.Millisecond, test.SampleGroups[0].Unit);
        Assert.AreEqual(sgMarker.Unit, test.SampleGroups[1].Unit);
        Assert.AreEqual("Time", test.SampleGroups[0].Name);
        Assert.AreEqual(sgMarker.Name, test.SampleGroups[1].Name);
        Assert.Greater(test.SampleGroups[0].Samples.Count,0);
        Assert.Greater(test.SampleGroups[1].Samples.Count,0);
    }

    [Test, Performance]
    public void MeasureMethod_WithCustomSampleGroup_And_ProfilerMarker_SetsTheCorrectSampleUnit_Run()
    {
        var sg = new SampleGroup("TEST", SampleUnit.Nanosecond);
        var sgMarker = new SampleGroup("TEST_MARKER", SampleUnit.Microsecond);
        Measure.Method(() => {MeasureProfilerSamplesTests.CreatePerformanceMarker("TEST_MARKER", 1);})
            .SampleGroup(sg).ProfilerMarkers(sgMarker).Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(2, test.SampleGroups.Count);
        Assert.AreEqual(sg.Unit, test.SampleGroups[0].Unit);
        Assert.AreEqual(sg.Name, test.SampleGroups[0].Name); 
        Assert.AreEqual(sgMarker.Unit, test.SampleGroups[1].Unit);
        Assert.AreEqual(sgMarker.Name, test.SampleGroups[1].Name);
        Assert.Greater(test.SampleGroups[0].Samples.Count, 0);
        Assert.Greater(test.SampleGroups[1].Samples.Count, 0);
        //TODO: Add a way to override time so we can mock it and test if the conversion has been done.
        // Add to all the tests where we pass a SampleGroup.
    }

    [Test, Performance]
    public void MeasureMethod_WithCustomSampleGroup_And_MultipleProfilerMarkers_SetsTheCorrectSampleUnit_Run()
    {
        var sampleGroups = new []
        {
            new SampleGroup("TEST_MARKER", SampleUnit.Microsecond),
            new SampleGroup("TEST_MARKER1", SampleUnit.Second)
        };
        var sg = new SampleGroup("TEST", SampleUnit.Nanosecond);

        Measure.Method(() =>
        {
            MeasureProfilerSamplesTests.CreatePerformanceMarker(sampleGroups[0].Name, 1);
            MeasureProfilerSamplesTests.CreatePerformanceMarker(sampleGroups[1].Name, 1);
        }).SampleGroup(sg).ProfilerMarkers(sampleGroups).Run();

        var test = PerformanceTest.Active;
        Assert.AreEqual(3, test.SampleGroups.Count);
        Assert.AreEqual(SampleUnit.Nanosecond, test.SampleGroups[0].Unit);
        Assert.AreEqual(sampleGroups[0].Unit, test.SampleGroups[1].Unit);
        Assert.AreEqual(sampleGroups[1].Unit, test.SampleGroups[2].Unit);
        Assert.AreEqual(sg.Name, test.SampleGroups[0].Name);
        Assert.AreEqual(sampleGroups[0].Name, test.SampleGroups[1].Name);
        Assert.AreEqual(sampleGroups[1].Name,test.SampleGroups[2].Name);
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

    private static bool LessThanOne(SampleGroup sampleGroup)
    {
        foreach (var sample in sampleGroup.Samples)
        {
            if (sample > 1f)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Implementation of IStopWatch used to track call counts in tests
    /// </summary>
    private class FakeWatch : IStopWatch
    {
        public double Sample = 0D;

        public void Start()
        {
        }

        public double Split()
        {
            return Sample;
        }
    }
}
