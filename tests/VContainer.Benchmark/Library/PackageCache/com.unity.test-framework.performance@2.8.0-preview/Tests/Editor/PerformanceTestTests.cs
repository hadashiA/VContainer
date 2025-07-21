using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.PerformanceTesting.Editor;
using UnityEngine;

public class PerformanceTestTests
{
    [Test]
    public void Serialize_NewTest()
    {
        var test = new PerformanceTest();

        var serialized = test.Serialize();

        var expected = "{\"Name\":\"\",\"Version\":\"\",\"Categories\":[],\"SampleGroups\":[]}";
        Assert.AreEqual(expected, serialized);
    }

    [Test]
    public void Serialize_TestWithSampleGroups()
    {
        var test = new PerformanceTest()
        {
            Categories = new List<string>() { "CAT1", "CAT2" },
            Name = "TEST",
            Version = "VERSION",
            SampleGroups = new List<SampleGroup>()
            {
                new SampleGroup("SAMPLEGROUP1", SampleUnit.Microsecond, true) { Samples = { 1D, 2D, 3D } },
                new SampleGroup("SAMPLEGROUP2", SampleUnit.Byte) { Samples = { 5D } }
            }
        };

        var serialized = test.Serialize();

        var expected = "{\"Name\":\"TEST\",\"Version\":\"VERSION\",\"Categories\":[\"CAT1\",\"CAT2\"],\"SampleGroups\":[{\"Name\":\"SAMPLEGROUP1\",\"Unit\":1,\"IncreaseIsBetter\":true,\"Samples\":[1.0,2.0,3.0],\"Min\":0.0,\"Max\":0.0,\"Median\":0.0,\"Average\":0.0,\"StandardDeviation\":0.0,\"Sum\":0.0},{\"Name\":\"SAMPLEGROUP2\",\"Unit\":4,\"IncreaseIsBetter\":false,\"Samples\":[5.0],\"Min\":0.0,\"Max\":0.0,\"Median\":0.0,\"Average\":0.0,\"StandardDeviation\":0.0,\"Sum\":0.0}]}";
        Assert.AreEqual(expected, serialized);
    }

    [TearDown]
    public void CleanPerformanceTestSingleton()
    {
        PerformanceTest.Active = null;
    }
}
