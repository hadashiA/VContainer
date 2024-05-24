using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEditor.SceneManagement;

public class ChangingHierarchyOfCanvasRenderer
{
    [Test]
    public void ChangingHierarchyOfCanvasRenderer_DoesntCrash()
    {
        // Canvas
        //  - Middle
        //      - Renderer
        // OtherCanvas
        var canvasObject = new GameObject("Canvas");
        canvasObject.AddComponent<Canvas>();

        var otherCanvasObject = new GameObject("OtherCanvas");
        otherCanvasObject.AddComponent<Canvas>();

        var middleObject = new GameObject("Middle");
        middleObject.transform.parent = canvasObject.transform;

        var renderObject = new GameObject("Render");
        renderObject.AddComponent<CanvasRenderer>();
        renderObject.transform.parent = middleObject.transform;
        renderObject.SetActive(false);

        // Translation causes IgnoreNextTransformChanged to be set on Renderer
        canvasObject.transform.Translate(1, 1, 1);
        // Reparenting after ignore
        middleObject.transform.parent = otherCanvasObject.transform;

        // Destroy the original canvas, and create a new scene to force destruction of everything else
        GameObject.DestroyImmediate(canvasObject);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

        Assert.Pass();
    }
}
