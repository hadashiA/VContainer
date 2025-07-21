using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    /// <summary>
    /// Displays a Texture2D for the UI System.
    /// </summary>
    /// <remarks>
    /// If you don't have or don't wish to create an atlas, you can simply use this script to draw a texture.
    /// Keep in mind though that this will create an extra draw call with each RawImage present, so it's
    /// best to use it only for backgrounds or temporary visible graphics.
    /// </remarks>

    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/Raw Image", 12)]
    public class RawImage : MaskableGraphic
    {
        [FormerlySerializedAs("m_Tex")]
        [SerializeField] Texture m_Texture;
        [SerializeField] Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

        protected RawImage()
        {
            useLegacyMeshGeneration = false;
        }

        /// <summary>
        /// Returns the texture used to draw this Graphic.
        /// </summary>
        public override Texture mainTexture
        {
            get
            {
                if (m_Texture == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return m_Texture;
            }
        }

        /// <summary>
        /// The RawImage's texture to be used.
        /// </summary>
        /// <remarks>
        /// Use this to alter or return the Texture the RawImage displays. The Raw Image can display any Texture whereas an Image component can only show a Sprite Texture.
        /// Note : Keep in mind that using a RawImage creates an extra draw call with each RawImage present, so it's best to use it only for backgrounds or temporary visible graphics.Note: Keep in mind that using a RawImage creates an extra draw call with each RawImage present, so it's best to use it only for backgrounds or temporary visible graphics.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //Create a new RawImage by going to Create>UI>Raw Image in the hierarchy.
        /// //Attach this script to the RawImage GameObject.
        ///
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class RawImageTexture : MonoBehaviour
        /// {
        ///     RawImage m_RawImage;
        ///     //Select a Texture in the Inspector to change to
        ///     public Texture m_Texture;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the RawImage component from the GameObject
        ///         m_RawImage = GetComponent<RawImage>();
        ///         //Change the Texture to be the one you define in the Inspector
        ///         m_RawImage.texture = m_Texture;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public Texture texture
        {
            get
            {
                return m_Texture;
            }
            set
            {
                if (m_Texture == value)
                    return;

                m_Texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        /// <summary>
        /// UV rectangle used by the texture.
        /// </summary>
        public Rect uvRect
        {
            get
            {
                return m_UVRect;
            }
            set
            {
                if (m_UVRect == value)
                    return;
                m_UVRect = value;
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// Adjust the scale of the Graphic to make it pixel-perfect.
        /// </summary>
        /// <remarks>
        /// This means setting the RawImage's RectTransform.sizeDelta  to be equal to the Texture dimensions.
        /// </remarks>
        public override void SetNativeSize()
        {
            Texture tex = mainTexture;
            if (tex != null)
            {
                int w = Mathf.RoundToInt(tex.width * uvRect.width);
                int h = Mathf.RoundToInt(tex.height * uvRect.height);
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            Texture tex = mainTexture;
            vh.Clear();
            if (tex != null)
            {
                var r = GetPixelAdjustedRect();
                var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
                var scaleX = tex.width * tex.texelSize.x;
                var scaleY = tex.height * tex.texelSize.y;
                {
                    var color32 = color;
                    vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(m_UVRect.xMin * scaleX, m_UVRect.yMin * scaleY));
                    vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(m_UVRect.xMin * scaleX, m_UVRect.yMax * scaleY));
                    vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(m_UVRect.xMax * scaleX, m_UVRect.yMax * scaleY));
                    vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(m_UVRect.xMax * scaleX, m_UVRect.yMin * scaleY));

                    vh.AddTriangle(0, 1, 2);
                    vh.AddTriangle(2, 3, 0);
                }
            }
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetMaterialDirty();
            SetVerticesDirty();
            SetRaycastDirty();
        }
    }
}
