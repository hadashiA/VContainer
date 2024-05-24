using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [Obsolete("Use BaseMeshEffect instead", true)]
    /// <summary>
    /// Obsolete class use BaseMeshEffect instead.
    /// </summary>
    public abstract class BaseVertexEffect
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use BaseMeshEffect.ModifyMeshes instead", true)] //We can't upgrade automatically since the signature changed.
        public abstract void ModifyVertices(List<UIVertex> vertices);
    }

    /// <summary>
    /// Base class for effects that modify the generated Mesh.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    ///using UnityEngine;
    ///using UnityEngine.UI;
    ///
    ///public class PositionAsUV1 : BaseMeshEffect
    ///{
    ///    protected PositionAsUV1()
    ///    {}
    ///
    ///    public override void ModifyMesh(Mesh mesh)
    ///    {
    ///        if (!IsActive())
    ///            return;
    ///
    ///        var verts = mesh.vertices.ToList();
    ///        var uvs = ListPool<Vector2>.Get();
    ///
    ///        for (int i = 0; i < verts.Count; i++)
    ///        {
    ///            var vert = verts[i];
    ///            uvs.Add(new Vector2(verts[i].x, verts[i].y));
    ///            verts[i] = vert;
    ///        }
    ///        mesh.SetUVs(1, uvs);
    ///        ListPool<Vector2>.Release(uvs);
    ///    }
    ///}
    /// ]]>
    ///</code>
    ///</example>

    [ExecuteAlways]
    public abstract class BaseMeshEffect : UIBehaviour, IMeshModifier
    {
        [NonSerialized]
        private Graphic m_Graphic;

        /// <summary>
        /// The graphic component that the Mesh Effect will aplly to.
        /// </summary>
        protected Graphic graphic
        {
            get
            {
                if (m_Graphic == null)
                    m_Graphic = GetComponent<Graphic>();

                return m_Graphic;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (graphic != null)
                graphic.SetVerticesDirty();
        }

        protected override void OnDisable()
        {
            if (graphic != null)
                graphic.SetVerticesDirty();
            base.OnDisable();
        }

        /// <summary>
        /// Called from the native side any time a animation property is changed.
        /// </summary>
        protected override void OnDidApplyAnimationProperties()
        {
            if (graphic != null)
                graphic.SetVerticesDirty();
            base.OnDidApplyAnimationProperties();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (graphic != null)
                graphic.SetVerticesDirty();
        }

#endif

        /// <summary>
        /// Function that is called when the Graphic is populating the mesh.
        /// </summary>
        /// <param name="mesh">The generated mesh of the Graphic element that needs modification.</param>
        public virtual void ModifyMesh(Mesh mesh)
        {
            using (var vh = new VertexHelper(mesh))
            {
                ModifyMesh(vh);
                vh.FillMesh(mesh);
            }
        }

        public abstract void ModifyMesh(VertexHelper vh);
    }
}
