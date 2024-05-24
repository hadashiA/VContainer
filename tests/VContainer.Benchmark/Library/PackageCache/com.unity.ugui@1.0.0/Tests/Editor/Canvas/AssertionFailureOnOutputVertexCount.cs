using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using UnityEditor;

public class AssertionFailureOnOutputVertexCount
{
    const string scenePath = "Assets/AssertionFailureOnOutputVertexCountTestScene.unity";
    [Test]
    public void AssertionFailureOnOutputVertexCountTest()
    {
        var newScene = EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);

        var canvasMaster = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvasChild = new GameObject("Canvas Child", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasChild.transform.SetParent(canvasMaster.transform);

        var panel1 = new GameObject("Panel", typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        panel1.transform.SetParent(canvasMaster.transform);

        var panel2 = new GameObject("Panel", typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        panel2.transform.SetParent(canvasChild.transform);

        // Saving a scene would trigger the error case 893551
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log("Success");

        LogAssert.Expect(LogType.Log, "Success");
    }

    [TearDown]
    public void TearDown()
    {
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(scenePath);
#endif
    }
}
