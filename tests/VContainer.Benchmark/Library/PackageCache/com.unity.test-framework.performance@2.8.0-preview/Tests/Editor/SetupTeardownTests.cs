using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

public class TestSetupTeardown
{
    private static int s_Counter;

    [SetUp]
    public void Setup()
    {
        Debug.Log("Setup");
        s_Counter = 1;
    }

    [Test, Performance]
    public void Test()
    {
        Assert.AreEqual(1, s_Counter);
        s_Counter = 2;
    }

    [TearDown]
    public void Teardown()
    {
        Debug.Log("Teardown");
        Assert.AreEqual(2, s_Counter);
    }
}

public class UnityTestSetupTeardown
{
    private static int s_Counter;

    [SetUp]
    public void Setup()
    {
        Debug.Log("Setup");
        s_Counter = 1;
    }

    [UnityTest, Performance]
    public IEnumerator Test()
    {
        Assert.AreEqual(1, s_Counter);
        s_Counter = 2;
        yield return null;
    }

    [TearDown]
    public void Teardown()
    {
        Debug.Log("Teardown");
        Assert.AreEqual(2, s_Counter);
    }
}