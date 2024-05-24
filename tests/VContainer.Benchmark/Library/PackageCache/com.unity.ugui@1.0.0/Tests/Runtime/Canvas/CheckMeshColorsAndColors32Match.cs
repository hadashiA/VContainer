using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools.Utils;

[TestFixture]
public class CheckMeshColorsAndColors32Match
{
    GameObject m_CanvasGO;
    GameObject m_ColorMeshGO;
    GameObject m_Color32MeshGO;
    Texture2D m_ScreenTexture;

    [SetUp]
    public void TestSetup()
    {
        // Create Canvas
        m_CanvasGO = new GameObject("Canvas");
        Canvas canvas = m_CanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Create Color UI GameObject
        m_ColorMeshGO = new GameObject("ColorMesh");
        CanvasRenderer colorMeshCanvasRenderer = m_ColorMeshGO.AddComponent<CanvasRenderer>();
        RectTransform colorMeshRectTransform = m_ColorMeshGO.AddComponent<RectTransform>();
        colorMeshRectTransform.pivot = colorMeshRectTransform.anchorMin = colorMeshRectTransform.anchorMax = Vector2.zero;
        m_ColorMeshGO.transform.SetParent(m_CanvasGO.transform);

        // Create Color32 UI GameObject
        m_Color32MeshGO = new GameObject("Color32Mesh");
        CanvasRenderer color32MeshCanvasRenderer = m_Color32MeshGO.AddComponent<CanvasRenderer>();
        RectTransform color32MeshRectTransform = m_Color32MeshGO.AddComponent<RectTransform>();
        color32MeshRectTransform.pivot = color32MeshRectTransform.anchorMin = color32MeshRectTransform.anchorMax = Vector2.zero;
        m_Color32MeshGO.transform.SetParent(m_CanvasGO.transform);

        Material material = new Material(Shader.Find("UI/Default"));

        // Setup Color mesh and add it to Color CanvasRenderer
        Mesh meshColor = new Mesh();
        meshColor.vertices = new Vector3[3] { new Vector3(0, 0, 0), new Vector3(0, 10, 0), new Vector3(10, 0, 0) };
        meshColor.triangles = new int[3] { 0, 1, 2 };
        meshColor.normals = new Vector3[3] { Vector3.zero, Vector3.zero, Vector3.zero };
        meshColor.colors = new Color[3] { Color.white, Color.white, Color.white };
        meshColor.uv = new Vector2[3] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0) };

        colorMeshCanvasRenderer.SetMesh(meshColor);
        colorMeshCanvasRenderer.SetMaterial(material, null);

        // Setup Color32 mesh and add it to Color32 CanvasRenderer
        Mesh meshColor32 = new Mesh();
        meshColor32.vertices = new Vector3[3] { new Vector3(10, 0, 0), new Vector3(10, 10, 0), new Vector3(20, 0, 0) };
        meshColor32.triangles = new int[3] { 0, 1, 2 };
        meshColor32.normals = new Vector3[3] { Vector3.zero, Vector3.zero, Vector3.zero };
        meshColor32.colors32 = new Color32[3] { Color.white, Color.white, Color.white };
        meshColor32.uv = new Vector2[3] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0) };

        color32MeshCanvasRenderer.SetMesh(meshColor32);
        color32MeshCanvasRenderer.SetMaterial(material, null);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasGO);
        GameObject.DestroyImmediate(m_ScreenTexture);
    }

    [UnityTest]
    public IEnumerator CheckMeshColorsAndColors32Matches()
    {
        yield return new WaitForEndOfFrame();

        // Create a Texture2D
        m_ScreenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        m_ScreenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        m_ScreenTexture.Apply();

        Color screenPixelColorForMeshColor = m_ScreenTexture.GetPixel(1, 0);
        Color screenPixelColorForMesh32Color = m_ScreenTexture.GetPixel(11, 0);

        Assert.That(screenPixelColorForMesh32Color, Is.EqualTo(screenPixelColorForMeshColor).Using(new ColorEqualityComparer(0.0f)), "UI Mesh with Colors does not match UI Mesh with Colors32");
    }
}
