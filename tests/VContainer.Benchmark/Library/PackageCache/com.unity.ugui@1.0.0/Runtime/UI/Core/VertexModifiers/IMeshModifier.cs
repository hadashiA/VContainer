using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("Use IMeshModifier instead", true)]
    public interface IVertexModifier
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("use IMeshModifier.ModifyMesh (VertexHelper verts)  instead", true)]
        void ModifyVertices(List<UIVertex> verts);
    }

    /// <summary>
    /// Interface which allows for the modification of verticies in a Graphic before they are passed to the CanvasRenderer.
    /// When a Graphic generates a list of vertices they are passed (in order) to any components on the GameObject that implement IMeshModifier. This component can modify the given Mesh.
    /// </summary>
    public interface IMeshModifier
    {
        [Obsolete("use IMeshModifier.ModifyMesh (VertexHelper verts) instead", false)]
        void ModifyMesh(Mesh mesh);
        /// <summary>
        /// Call used to modify mesh.
        /// Place any custom mesh processing in this function.
        /// </summary>
        void ModifyMesh(VertexHelper verts);
    }
}
