using UnityEngine.UI;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine;

[TestFixture]
[Category("RegressionTest")]
public class ImageFilledGenerateWork
{
    GameObject m_CanvasGO;
    GameObject m_ImageGO;

    [SetUp]
    public void SetUp()
    {
        m_CanvasGO = new GameObject("Canvas");
        m_ImageGO = new GameObject("Image");
    }

    [Test]
    public void ImageFilledGenerateWorks()
    {
        m_CanvasGO.AddComponent<Canvas>();
        m_ImageGO.transform.SetParent(m_CanvasGO.transform);
        var image = m_ImageGO.AddComponent<TestableImage>();
        image.type = Image.Type.Filled;
        var texture = new Texture2D(32, 32);
        image.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.zero);
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillAmount = 0.5f;

        // Generate the image data now.
        VertexHelper vh = new VertexHelper();

        // Image is a "TestableImage" which has the Assert in the GenerateImageData as we need to validate
        // the data which is protected.
        image.GenerateImageData(vh);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasGO);
    }
}
