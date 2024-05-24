using NUnit.Framework;
using UnityEngine.UI;
using UnityEngine;

[Category("Slider")]
public class SliderTests
{
    private Slider slider;
    private GameObject emptyGO;
    private GameObject rootGO;

    [SetUp]
    public void Setup()
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
    public void SetSliderValueWithoutNotifyWillNotNotify()
    {
        slider.value = 0;

        bool calledOnValueChanged = false;

        slider.onValueChanged.AddListener(f => { calledOnValueChanged = true; });

        slider.SetValueWithoutNotify(1);

        Assert.IsTrue(slider.value == 1);
        Assert.IsFalse(calledOnValueChanged);
    }
}
