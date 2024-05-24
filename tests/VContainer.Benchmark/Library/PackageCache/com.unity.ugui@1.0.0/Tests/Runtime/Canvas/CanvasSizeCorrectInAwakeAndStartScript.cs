using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

public class CanvasSizeCorrectInAwakeAndStartScript : MonoBehaviour
{
    public bool isStartCalled { get; private set; }
    public bool isAwakeCalled { get; private set; }

    protected void Awake()
    {
        Assert.That(transform.position, Is.Not.EqualTo(Vector3.zero).Using(new Vector3EqualityComparer(0.0f)));
        isAwakeCalled = true;
    }

    protected void Start()
    {
        Assert.That(transform.position, Is.Not.EqualTo(Vector3.zero).Using(new Vector3EqualityComparer(0.0f)));
        isStartCalled = true;
    }
}
