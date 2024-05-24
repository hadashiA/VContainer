using NUnit.Framework;
using UnityEngine.UI;
using System.Reflection;

public class TestableImage : Image
{
    public bool isOnPopulateMeshCalled = false;
    public bool isGeometryUpdated = false;

    // Hook into the mesh generation so we can do our check.
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        Assert.That(toFill.currentVertCount, Is.GreaterThan(0), "Expected the mesh to be filled but it was not. Should not have a mesh with zero vertices.");
        isOnPopulateMeshCalled = true;
    }

    protected override void UpdateGeometry()
    {
        base.UpdateGeometry();
        isGeometryUpdated = true;
    }

    public void GenerateImageData(VertexHelper vh)
    {
        OnPopulateMesh(vh);
    }
}
