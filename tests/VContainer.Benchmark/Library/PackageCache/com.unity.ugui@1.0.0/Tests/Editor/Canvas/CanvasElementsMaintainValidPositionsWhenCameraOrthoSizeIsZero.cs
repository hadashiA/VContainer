using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;


public class CanvasElementsMaintainValidPositionsWhenCameraOrthoSizeIsZero
{
    GameObject image;
    GameObject canvas;
    GameObject camera;

    [SetUp]
    public void TestSetup()
    {
        canvas = new GameObject("Canvas", typeof(Canvas));

        image = new GameObject("Image", typeof(Image));
        image.transform.SetParent(canvas.transform);

        camera = new GameObject("Camera", typeof(Camera));
        var cameraComponent = camera.GetComponent<Camera>();
        cameraComponent.orthographic = true;

        var canvasComponent = canvas.GetComponent<Canvas>();
        canvasComponent.worldCamera = camera.GetComponent<Camera>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceCamera;
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(canvas);
        GameObject.DestroyImmediate(camera);
    }

    [UnityTest]
    public IEnumerator TestCanvasElementsMaintainValidPositionsWhenCameraOrthoSizeIsZero()
    {
        var cameraComponent = camera.GetComponent<Camera>();
        cameraComponent.orthographicSize = 0;
        yield return null;

        Assert.AreNotEqual(image.transform.position.x, float.NaN);
        Assert.AreNotEqual(image.transform.position.y, float.NaN);


        cameraComponent.orthographicSize = 2;
        yield return null;

        Assert.AreEqual(image.transform.position.x, 0.0f);
        Assert.AreEqual(image.transform.position.y, 0.0f);

        Assert.Pass();
    }
}
