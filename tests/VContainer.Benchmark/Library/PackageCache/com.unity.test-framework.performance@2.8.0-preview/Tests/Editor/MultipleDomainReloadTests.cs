using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

public class MultipleDomainReloadTests
{
    const string k_SampleGroupName = "TEST_SAMLEGROUP";
    SampleGroup m_SampleGroup = new SampleGroup(k_SampleGroupName);
    [SerializeField]
    double m_Sample;

    public enum MeasureType
    {
        ByReference,
        ByValue
    }

    void MeasureSample(MeasureType measureType)
    {
        m_Sample++;
        if (measureType == MeasureType.ByValue)
        {
            Measure.Custom(k_SampleGroupName, m_Sample);
        }
        else if (measureType == MeasureType.ByReference)
        {
            Measure.Custom(m_SampleGroup, m_Sample);
        }
    }

    [UnityTest, Performance]
    public IEnumerator MeasureCustom_Survives_MultipleSessionEvents([Values] MeasureType measureType)
    {
        m_Sample = 0D;

        MeasureSample(measureType);
        yield return new EnterPlayMode();
        MeasureSample(measureType);
        yield return new ExitPlayMode();
        MeasureSample(measureType);
        yield return new EnterPlayMode();
        MeasureSample(measureType);
        yield return new ExitPlayMode();
        MeasureSample(measureType);

        Assert1To5();
    }

    void Assert1To5()
    {
        Assert.AreEqual(1, PerformanceTest.Active.SampleGroups.Count, "SampleGroup count does not match");
        var sampleGroup = PerformanceTest.Active.SampleGroups.First(s => s.Name == k_SampleGroupName);
        Assert.AreEqual(5, sampleGroup.Samples.Count, "Sample count does not match");
        Assert.AreEqual(SampleUnit.Millisecond, sampleGroup.Unit);
        Assert.AreEqual(false, sampleGroup.IncreaseIsBetter);

        for (var i = 0; i < 5; i++)
        {
            var sample = sampleGroup.Samples[i];
            Assert.AreEqual(i + 1, sample, Mathf.Epsilon);
        }
    }
}
