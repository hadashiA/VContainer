using NUnit.Framework;
using UnityEngine.UI;
using UnityEngine;

[Category("Slider")]
public class SliderRectRefernces : Behaviour
{
    private Slider slider;
    private GameObject emptyGO;
    private GameObject rootGO;

    [SetUp]
    public void TestSetup()
    {
        rootGO = new GameObject("root child");
        rootGO.AddComponent<Canvas>();

        var sliderGameObject = new GameObject("Slider");
        slider = sliderGameObject.AddComponent<Slider>();

        emptyGO = new GameObject("base", typeof(RectTransform));

        sliderGameObject.transform.SetParent(rootGO.transform);
        emptyGO.transform.SetParent(sliderGameObject.transform);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(rootGO);
    }

    [Test]
    public void AssigningSelfResultsInNullReferenceField()
    {
        slider.fillRect = (RectTransform)slider.transform;
        Assert.IsNull(slider.fillRect);

        slider.handleRect = (RectTransform)slider.transform;
        Assert.IsNull(slider.handleRect);
    }

    [Test]
    public void AssigningOtherObjectResultsInCorrectReferenceField()
    {
        slider.fillRect = (RectTransform)emptyGO.transform;
        Assert.IsNotNull(slider.fillRect);
        Assert.AreEqual(slider.fillRect, (RectTransform)emptyGO.transform);

        slider.handleRect = (RectTransform)emptyGO.transform;
        Assert.IsNotNull(slider.handleRect);
        Assert.AreEqual(slider.handleRect, (RectTransform)emptyGO.transform);
    }
}
