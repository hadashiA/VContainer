using System;
using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.PerformanceTesting.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

public class GenericTests
{
    [Performance, Test]
    public void Utils_ConvertFromUnixTimestamp()
    {
        var date = new DateTime(2000, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

        var dt = Utils.ConvertFromUnixTimestamp(946688461001);

        Assert.AreEqual(date.Ticks, dt.Ticks);
    }

    [Performance, Test]
    public void Utils_ConvertToUnixTimestamp()
    {
        var dt = new DateTime(2000, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);
        DateTime.SpecifyKind(dt, DateTimeKind.Utc);

        var stamp = Utils.ConvertToUnixTimestamp(dt);

        Assert.AreEqual(946688461001, stamp);
    }

    [Performance, Test]
    public void Default_VersionAttribute_IsSet()
    {
        Assert.AreEqual("1", PerformanceTest.Active.Version);
    }

    [Performance, Test, Version("TEST")]
    public void VersionAttribute_IsSet()
    {
        Assert.AreEqual("TEST", PerformanceTest.Active.Version);
    }

    [TestCase("1"), TestCase("2")]
    [Performance, Version("TEST")]
    public void VersionAttribute_IsSet_OnTestCase(string name)
    {
        Assert.AreEqual("TEST", PerformanceTest.Active.Version);
    }

    [Performance, Test]
    public void ZeroSampleGroups_Highlighted_SingleSample()
    {
        var sg = new SampleGroup("TEST");
        Measure.Custom(sg, 0);
    }

    [Performance, Test]
    public void ZeroSampleGroups_Highlighted_MultipleSamples()
    {
        var sg = new SampleGroup("TEST");
        Measure.Custom(sg, 0);
        Measure.Custom(sg, 0);
        Measure.Custom(sg, 0);
        Measure.Custom(sg, 0);
    }

    [UnityTest, Performance]
    public IEnumerator EnterPlaymode_NoFailure()
    {
        yield return new EnterPlayMode();
        yield return new ExitPlayMode();
    }
}

[Version("1")]
public class ClassVersionTests
{
    [Test, Performance]
    public void Default_VersionAttribute_IsSet()
    {
        Assert.AreEqual("1.1", PerformanceTest.Active.Version);
    }

    [Test, Performance, Version("TEST")]
    public void VersionAttribute_IsSet()
    {
        Assert.AreEqual("1.TEST", PerformanceTest.Active.Version);
    }

    [TestCase("1"), TestCase("2")]
    [Test, Performance, Version("TEST")]
    public void VersionAttribute_IsSet_OnTestCase(string name)
    {
        Assert.AreEqual("1.TEST", PerformanceTest.Active.Version);
    }
}
