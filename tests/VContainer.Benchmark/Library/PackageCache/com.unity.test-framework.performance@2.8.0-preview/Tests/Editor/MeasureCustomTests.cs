using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.PerformanceTesting.Exceptions;
using SampleGroup = Unity.PerformanceTesting.SampleGroup;

public class MeasureCustomTests
{
    [Test, Performance]
    public void MeasureCustom_SampleGroup_CorrectValues()
    {
        var sg = new SampleGroup("REGULAR", SampleUnit.Byte, true);
        Measure.Custom(sg, 10D);

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(1, test.SampleGroups[0].Samples.Count);
        Assert.AreEqual(10D, test.SampleGroups[0].Samples[0], 0.001D);
        AssertDefinition(test.SampleGroups[0], "REGULAR", SampleUnit.Byte, increaseIsBetter: true);
    }

    [Test, Performance]
    public void MeasureCustom_SampleGroupWithSamples_CorrectValues()
    {
        var sg = new SampleGroup("REGULAR", SampleUnit.Byte, true);

        Measure.Custom(sg, 10D);
        Measure.Custom(sg, 20D);

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(2, test.SampleGroups[0].Samples.Count);
        Assert.AreEqual(10D, test.SampleGroups[0].Samples[0], 0.001D);
        Assert.AreEqual(20D, test.SampleGroups[0].Samples[1], 0.001D);
        AssertDefinition(test.SampleGroups[0], "REGULAR", SampleUnit.Byte, increaseIsBetter: true);
    }

    [Test, Performance]
    public void MeasureCustom_PercentileSample_CorrectValues()
    {
        var sg = new SampleGroup("PERCENTILE", SampleUnit.Second, true);
        Measure.Custom(sg, 10D);

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(1, test.SampleGroups[0].Samples.Count);
        Assert.AreEqual(10D, test.SampleGroups[0].Samples[0], 0.001D);
        AssertDefinition(test.SampleGroups[0], "PERCENTILE", SampleUnit.Second, increaseIsBetter: true);
    }

    [Test, Performance]
    public void MeasureCustom_PercentileSamples_CorrectValues()
    {
        var sg = new SampleGroup("PERCENTILE", SampleUnit.Second, increaseIsBetter: true);

        Measure.Custom(sg, 10D);
        Measure.Custom(sg, 20D);

        var test = PerformanceTest.Active;
        Assert.AreEqual(1, test.SampleGroups.Count);
        Assert.AreEqual(2, test.SampleGroups[0].Samples.Count);
        Assert.AreEqual(10D, test.SampleGroups[0].Samples[0], 0.001D);
        Assert.AreEqual(20D, test.SampleGroups[0].Samples[1], 0.001D);
        AssertDefinition(test.SampleGroups[0], "PERCENTILE", SampleUnit.Second, increaseIsBetter: true);
    }

    [Test, Performance]
    public void MeasureCustom_MultipleSampleGroups()
    {
        var sg = new SampleGroup("REGULAR", SampleUnit.Byte, true);
        var sg2 = new SampleGroup("PERCENTILE", SampleUnit.Second, true);
        Measure.Custom(sg, 20D);
        Measure.Custom(sg2, 10D);

        var test = PerformanceTest.Active;
        Assert.AreEqual(2, test.SampleGroups.Count);
        AssertDefinition(test.SampleGroups[0], "REGULAR", SampleUnit.Byte, increaseIsBetter: true);
        AssertDefinition(test.SampleGroups[1], "PERCENTILE", SampleUnit.Second, increaseIsBetter: true);
    }

    [Test, Performance]
    public void MeasureCustom_NaN_Throws()
    {
        var sg = new SampleGroup("REGULAR", SampleUnit.Byte, true);

        Assert.Throws<PerformanceTestException>(() => Measure.Custom(sg, double.NaN));
    }

    private static void AssertDefinition(SampleGroup sampleGroup, string name, SampleUnit sampleUnit,
        bool increaseIsBetter)
    {
        Assert.AreEqual(name, sampleGroup.Name);
        Assert.AreEqual(sampleUnit, sampleGroup.Unit);
        Assert.AreEqual(increaseIsBetter, sampleGroup.IncreaseIsBetter);
    }
}