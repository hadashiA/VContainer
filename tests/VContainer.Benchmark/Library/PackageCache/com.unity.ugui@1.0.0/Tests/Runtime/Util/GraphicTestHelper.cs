using System.Collections.Generic;
using NUnit.Framework;

namespace UnityEngine.UI.Tests
{
    static class GraphicTestHelper
    {
        public static void TestOnPopulateMeshDefaultBehavior(Graphic graphic, VertexHelper vh)
        {
            Assert.AreEqual(4, vh.currentVertCount);
            List<UIVertex> verts = new List<UIVertex>();
            vh.GetUIVertexStream(verts);

            // The vertices for the 2 triangles of the canvas
            UIVertex[] expectedVertices =
            {
                new UIVertex
                {
                    color = graphic.color,
                    position = new Vector3(graphic.rectTransform.rect.x, graphic.rectTransform.rect.y),
                    uv0 = new Vector2(0f, 0f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
                new UIVertex
                {
                    color = graphic.color,
                    position = new Vector3(graphic.rectTransform.rect.x, graphic.rectTransform.rect.y + graphic.rectTransform.rect.height),
                    uv0 = new Vector2(0f, 1f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
                new UIVertex
                {
                    color = graphic.color,
                    position = new Vector3(graphic.rectTransform.rect.x + graphic.rectTransform.rect.width, graphic.rectTransform.rect.y + graphic.rectTransform.rect.height),
                    uv0 = new Vector2(1f, 1f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
                new UIVertex
                {
                    color = graphic.color,
                    position = new Vector3(graphic.rectTransform.rect.x + graphic.rectTransform.rect.width, graphic.rectTransform.rect.y + graphic.rectTransform.rect.height),
                    uv0 = new Vector2(1f, 1f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
                new UIVertex
                {
                    color = graphic.color,
                    position = new Vector3(graphic.rectTransform.rect.x + graphic.rectTransform.rect.width, graphic.rectTransform.rect.y),
                    uv0 = new Vector2(1f, 0f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
                new UIVertex
                {
                    color = graphic.color,
                    position = new Vector3(graphic.rectTransform.rect.x, graphic.rectTransform.rect.y),
                    uv0 = new Vector2(0f, 0f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
            };

            for (int i = 0; i < verts.Count; i++)
            {
                Assert.AreEqual(expectedVertices[i], verts[i]);
            }
        }
    }
}
