using NUnit.Framework;
using UnityEngine;

public class TestBehaviourBase<T> where T : Behaviour
{
    protected T m_TestObject;

    [SetUp]
    public virtual void TestSetup()
    {
        var gameObject = new GameObject();
        m_TestObject = gameObject.AddComponent<T>();
    }

    [TearDown]
    public virtual void Teardown()
    {
        GameObject.DestroyImmediate(m_TestObject.gameObject);
    }
}
