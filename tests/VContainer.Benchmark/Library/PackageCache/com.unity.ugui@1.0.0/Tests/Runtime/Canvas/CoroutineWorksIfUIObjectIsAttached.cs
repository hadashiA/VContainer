using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;

[TestFixture]
[UnityPlatform(include = new RuntimePlatform[] { RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor, RuntimePlatform.WindowsEditor })]
[Category("RegressionTest")]
[Description("CoveredBugID = 904415")]
public class CoroutineWorksIfUIObjectIsAttached
{
    GameObject m_CanvasMaster;
    GameObject m_ImageObject;

    [SetUp]
    public void TestSetup()
    {
        m_CanvasMaster = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        m_ImageObject = new GameObject("Image", typeof(Image));
        m_ImageObject.SetActive(false);
    }

    [UnityTest]
    public IEnumerator CoroutineWorksOnAttachingUIObject()
    {
        // Generating Basic scene
        m_CanvasMaster.AddComponent<CoroutineObject>();

        yield return null;

        m_ImageObject.transform.SetParent(m_CanvasMaster.transform);
        m_ImageObject.AddComponent<BugObject>();
        m_ImageObject.SetActive(true);

        yield return null;
        yield return null;
        yield return null;

        Assert.That(m_CanvasMaster.GetComponent<CoroutineObject>().coroutineCount, Is.GreaterThan(1), "The Coroutine wasn't supposed to stop but continue to run, something made it stopped");
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasMaster);
        GameObject.DestroyImmediate(m_ImageObject);
    }
}

public class BugObject : MonoBehaviour
{
    void Awake()
    {
        GameObject newObject = new GameObject("NewGameObjectThatTriggersBug");
        newObject.transform.SetParent(transform);
        newObject.AddComponent<Text>();
    }
}

public class CoroutineObject : MonoBehaviour
{
    public int coroutineCount { get; private set; }

    public IEnumerator Start()
    {
        // This coroutine should not stop and continue adding to the timer
        while (true)
        {
            coroutineCount++;
            yield return null;
        }
    }
}
