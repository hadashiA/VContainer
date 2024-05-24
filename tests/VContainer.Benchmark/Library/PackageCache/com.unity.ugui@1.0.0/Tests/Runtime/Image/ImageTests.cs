using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;
using System.Reflection;

public class ImageTests
{
    private const int Width = 32;
    private const int Height = 32;

    GameObject m_CanvasGO;
    TestableImage m_Image;
    private Texture2D m_defaultTexture;

    private bool m_dirtyLayout;
    private bool m_dirtyMaterial;

    [SetUp]
    public void SetUp()
    {
        m_CanvasGO = new GameObject("Canvas", typeof(Canvas));
        GameObject imageObject = new GameObject("Image", typeof(TestableImage));
        imageObject.transform.SetParent(m_CanvasGO.transform);
        m_Image = imageObject.GetComponent<TestableImage>();
        m_Image.RegisterDirtyLayoutCallback(() => m_dirtyLayout = true);
        m_Image.RegisterDirtyMaterialCallback(() => m_dirtyMaterial = true);

        m_defaultTexture = new Texture2D(Width, Height);
        Color[] colors = new Color[Width * Height];
        for (int i = 0; i < Width * Height; i++)
            colors[i] = Color.magenta;
        m_defaultTexture.Apply();
    }

    [Test]
    public void TightMeshSpritePopulatedVertexHelperProperly()
    {
        Texture2D texture = new Texture2D(64, 64);
        m_Image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        m_Image.type = Image.Type.Simple;
        m_Image.useSpriteMesh = true;

        VertexHelper vh = new VertexHelper();

        m_Image.GenerateImageData(vh);

        Assert.AreEqual(vh.currentVertCount, m_Image.sprite.vertices.Length);
        Assert.AreEqual(vh.currentIndexCount, m_Image.sprite.triangles.Length);
    }

    [UnityTest]
    public IEnumerator CanvasCustomRefPixPerUnitToggleWillUpdateImageMesh()
    {
        var canvas = m_CanvasGO.GetComponent<Canvas>();
        var canvasScaler = m_CanvasGO.AddComponent<CanvasScaler>();

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        m_Image.transform.SetParent(m_CanvasGO.transform);
        m_Image.type = Image.Type.Sliced;
        var texture = new Texture2D(120, 120);
        m_Image.sprite = Sprite.Create(texture, new Rect(0, 0, 120, 120), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight, new Vector4(30, 30, 30, 30), true);
        m_Image.fillCenter = true;


        canvasScaler.referencePixelsPerUnit = 200;
        yield return null; // skip frame to update canvas properly
        //setup done

        canvas.enabled = false;

        yield return null;

        canvas.enabled = true;
        m_Image.isOnPopulateMeshCalled = false;

        yield return null;

        Assert.IsTrue(m_Image.isOnPopulateMeshCalled);
    }

    [UnityTest]
    public IEnumerator Sprite_Layout()
    {
        m_Image.sprite = Sprite.Create(m_defaultTexture, new Rect(0, 0, Width, Height), Vector2.zero);
        yield return null;

        m_Image.isGeometryUpdated = false;
        m_dirtyLayout = false;

        var Texture = new Texture2D(Width * 2, Height * 2);
        m_Image.sprite = Sprite.Create(Texture, new Rect(0, 0, Width, Height), Vector2.zero);
        yield return new WaitUntil(() => m_Image.isGeometryUpdated);

        // validate that layout change rebuil is not called
        Assert.IsFalse(m_dirtyLayout);

        m_Image.isGeometryUpdated = false;
        m_dirtyLayout = false;
        m_Image.sprite = Sprite.Create(Texture, new Rect(0, 0, Width / 2, Height / 2), Vector2.zero);
        yield return new WaitUntil(() => m_Image.isGeometryUpdated);

        // validate that layout change rebuil is called
        Assert.IsTrue(m_dirtyLayout);
    }

    [UnityTest]
    public IEnumerator Sprite_Material()
    {
        m_Image.sprite = Sprite.Create(m_defaultTexture, new Rect(0, 0, Width, Height), Vector2.zero);
        yield return null;

        m_Image.isGeometryUpdated = false;
        m_dirtyMaterial = false;
        m_Image.sprite = Sprite.Create(m_defaultTexture, new Rect(0, 0, Width / 2, Height / 2), Vector2.zero);
        yield return new WaitUntil(() => m_Image.isGeometryUpdated);

        // validate that material change rebuild is not called
        Assert.IsFalse(m_dirtyMaterial);

        m_Image.isGeometryUpdated = false;
        m_dirtyMaterial = false;
        var Texture = new Texture2D(Width * 2, Height * 2);
        m_Image.sprite = Sprite.Create(Texture, new Rect(0, 0, Width / 2, Height / 2), Vector2.zero);
        yield return new WaitUntil(() => m_Image.isGeometryUpdated);

        // validate that layout change rebuil is called
        Assert.IsTrue(m_dirtyMaterial);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasGO);
        GameObject.DestroyImmediate(m_defaultTexture);
    }
}
