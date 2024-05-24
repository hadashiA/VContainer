using NUnit.Framework;
using System.Threading;
using Unity.PerformanceTesting;
using SampleGroup = Unity.PerformanceTesting.SampleGroup;

public class MeasureScope
{
    [Test, Performance]
    public void MeasureScope_WithoutDefinition_MeasuresDefaultSample()
    {
        using (Measure.Scope())
        {
            Thread.Sleep(1);
        }

        var result = PerformanceTest.Active;
        Assert.AreEqual(1, result.SampleGroups.Count);
        Assert.Greater(result.SampleGroups[0].Samples[0], 0.0f);
        AssertDefinition(result.SampleGroups[0], "Time", SampleUnit.Millisecond, increaseIsBetter: false);
    }

    [Test, Performance]
    public void MeasureScope_WithDefinition_MeasuresSample()
    {
        using (Measure.Scope("TEST"))
        {
            Thread.Sleep(1);
        }

        var result = PerformanceTest.Active;
        Assert.AreEqual(1, result.SampleGroups.Count);
        Assert.Greater(result.SampleGroups[0].Samples[0], 0.0f);
        AssertDefinition(result.SampleGroups[0], "TEST", SampleUnit.Millisecond, increaseIsBetter: false);
    }
    
    [Test, Performance]
    public void MeasureMultipleScopes_WithDefinition_MeasuresSamples()
    {
        using (Measure.Scope("TEST"))
        {
            Thread.Sleep(1);
        }
        
        using (Measure.Scope("TEST"))
        {
            Thread.Sleep(1);
        }
        
        using (Measure.Scope("TEST"))
        {
            Thread.Sleep(1);
        }
        
        using (Measure.Scope("TEST"))
        {
            Thread.Sleep(1);
        }

        var result = PerformanceTest.Active;
        Assert.AreEqual(1, result.SampleGroups.Count);
        Assert.AreEqual(4, result.SampleGroups[0].Samples.Count);
        Assert.Greater(result.SampleGroups[0].Samples[0], 0.0f);
        AssertDefinition(result.SampleGroups[0], "TEST", SampleUnit.Millisecond, increaseIsBetter: false);
    }

    [Test, Performance]
    public void MeasureScope_WithSampleGroup_ResultsIsInTheDefinedSampleUnit()
    {
        var sg = new SampleGroup("TEST", SampleUnit.Microsecond);
        using (Measure.Scope(sg))
        {
        }

        var result = PerformanceTest.Active;
        Assert.AreEqual(1, result.SampleGroups.Count);
        AssertDefinition(result.SampleGroups[0], sg.Name, sg.Unit, increaseIsBetter: false);
    }

    private static void AssertDefinition(SampleGroup sampleGroup, string name, SampleUnit sampleUnit, bool increaseIsBetter)
    {
        Assert.AreEqual(name, sampleGroup.Name);
        Assert.AreEqual(sampleUnit, sampleGroup.Unit);
        Assert.AreEqual(increaseIsBetter, sampleGroup.IncreaseIsBetter);
    }
}