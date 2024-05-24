using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;

public class ParentCanvasIsSane
{
    GameObject rootCanvas;
    GameObject rootObject;
    GameObject child1;
    CanvasGroup c1CanvasGroup;
    GameObject child2;
    GameObject child3;

    [SetUp]
    public void TestSetup()
    {
        // root GO
        // root Canvas
        //  L child1 GO (RectTransform, CanvasGroup)
        //     L child2 GO (RectTransform)
        //        L child3 GO (Image)

        rootCanvas = new GameObject("root Canvas");
        rootCanvas.AddComponent<Canvas>();
        rootCanvas.AddComponent<CanvasScaler>();

        rootObject = new GameObject("root GO");

        child1 = new GameObject("child1 GO");
        child1.AddComponent<RectTransform>();
        c1CanvasGroup = child1.AddComponent<CanvasGroup>();

        child2 = new GameObject("child2 GO");
        child2.AddComponent<RectTransform>();

        child3 = new GameObject("child3 GO");
        child3.AddComponent<Image>();

        child3.transform.SetParent(child2.transform);
        child2.transform.SetParent(child1.transform);
        child1.transform.SetParent(rootCanvas.transform);
    }

    [UnityTest]
    public IEnumerator RecalculatingAlphaOnReparentedInactiveObjectsDoesNotCrash()
    {
        Assert.IsNotNull(child3.GetComponent<CanvasRenderer>());

        c1CanvasGroup.alpha = 0.5f;
        child1.SetActive(false);
        child1.transform.SetParent(rootObject.transform, true);

        // This will crash if child3.GetComponent<CanvasRenderer>().m_ParentCanvas is not null.
        yield return null;
    }
}
