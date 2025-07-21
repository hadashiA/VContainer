using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace UnityEngine.UI.Tests
{
    [TestFixture]
    class ImageTests
    {
        Image m_Image;
        private Sprite m_Sprite;
        private Sprite m_OverrideSprite;
        Texture2D texture = new Texture2D(128, 128);
        Texture2D overrideTexture = new Texture2D(512, 512);

        bool m_dirtyVert;
        bool m_dirtyLayout;
        bool m_dirtyMaterial;

        Camera m_camera;
        GameObject m_CanvasRoot;

        [SetUp]
        public void TestSetup()
        {
            m_CanvasRoot = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            GameObject gameObject = new GameObject("Image", typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(m_CanvasRoot.transform);

            m_camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();

            m_Image = gameObject.GetComponent<Image>();

            Color[] colors = new Color[128 * 128];
            for (int y = 0; y < 128; y++)
                for (int x = 0; x < 128; x++)
                    colors[x + 128 * y] = new Color(0, 0, 0, 1 - x / 128f);
            texture.SetPixels(colors);
            texture.Apply();

            Color[] overrideColors = new Color[512 * 512];
            for (int y = 0; y < 512; y++)
                for (int x = 0; x < 512; x++)
                    overrideColors[x + 512 * y] = new Color(0, 0, 0, y / 512f);
            overrideTexture.SetPixels(overrideColors);
            overrideTexture.Apply();

            m_Sprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 100);
            m_OverrideSprite = Sprite.Create(overrideTexture, new Rect(0, 0, 512, 512), new Vector2(0.5f, 0.5f), 200);

            m_Image.rectTransform.anchoredPosition = new Vector2(0, 0);
            m_Image.rectTransform.sizeDelta = new Vector2(100, 100);
            m_Image.RegisterDirtyVerticesCallback(() => m_dirtyVert = true);
            m_Image.RegisterDirtyLayoutCallback(() => m_dirtyLayout = true);
            m_Image.RegisterDirtyMaterialCallback(() => m_dirtyMaterial = true);

            ResetDirtyFlags();
        }

        [TearDown]
        public void TearDown()
        {
            m_Image = null;
            m_Sprite = null;

            GameObject.DestroyImmediate(m_CanvasRoot);
            GameObject.DestroyImmediate(m_camera.gameObject);
            m_camera = null;
        }

        private void ResetDirtyFlags()
        {
            m_dirtyVert = m_dirtyLayout = m_dirtyMaterial = false;
        }

        [Test]
        public void SetTestSprite()
        {
            m_Image.sprite = m_Sprite;
            m_Image.overrideSprite = m_OverrideSprite;
            Assert.AreEqual(m_Sprite, m_Image.sprite);
            m_Image.sprite = null;
            Assert.AreEqual(null, m_Image.sprite);
        }

        [Test]
        public void TestPixelsPerUnit()
        {
            m_Image.sprite = m_Sprite;
            Assert.AreEqual(1.0f, m_Image.pixelsPerUnit);

            m_Image.overrideSprite = m_OverrideSprite;
            Assert.AreEqual(2.0f, m_Image.pixelsPerUnit);

            m_Image.overrideSprite = null;
            Assert.AreEqual(1.0f, m_Image.pixelsPerUnit);
        }

        [Test]
        public void RaycastOverImageWithoutASpriteReturnTrue()
        {
            m_Image.sprite = null;
            bool raycast = m_Image.Raycast(new Vector2(10, 10), m_camera);
            Assert.AreEqual(true, raycast);
        }

        [Test]
        [TestCase(0.0f, 1000, 1000)]
        [TestCase(1.0f, 1000, 1000)]
        [TestCase(0.0f, -1000, 1000)]
        [TestCase(1.0f, -1000, 1000)]
        [TestCase(0.0f, 1000, -1000)]
        [TestCase(1.0f, 1000, -1000)]
        [TestCase(0.0f, -1000, -1000)]
        [TestCase(1.0f, -1000, -1000)]
        public void RaycastOverImageWithoutASpriteReturnsTrueWithCoordinatesOutsideTheBoundaries(float alphaThreshold, float x, float y)
        {
            m_Image.alphaHitTestMinimumThreshold = 1.0f - alphaThreshold;
            bool raycast = m_Image.Raycast(new Vector2(x, y), m_camera);
            Assert.IsTrue(raycast);
        }

        [Test]
        public void RaycastOverImage_IgnoresDisabledCanvasGroup()
        {
            var canvasGroup = m_CanvasRoot.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.enabled = false;

            bool raycast = m_Image.Raycast(new Vector2(1000, 1000), m_camera);
            Assert.IsTrue(raycast);
        }

        [Test]
        public void SettingSpriteMarksAllAsDirty()
        {
            m_Image.sprite = m_Sprite;
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.True(m_dirtyLayout, "Layout has not been dirtied");
            Assert.True(m_dirtyMaterial, "Material has not been dirtied");
        }

        [Test]
        public void SettingOverrideSpriteMarksAllAsDirty()
        {
            m_Image.overrideSprite = m_OverrideSprite;
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.True(m_dirtyLayout, "Layout has not been dirtied");
            Assert.True(m_dirtyMaterial, "Material has not been dirtied");
        }

        [Test]
        public void SettingTypeMarksVerticesAsDirty()
        {
            m_Image.type = Image.Type.Filled;
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.False(m_dirtyLayout, "Layout has been dirtied");
            Assert.False(m_dirtyMaterial, "Material has been dirtied");
        }

        [Test]
        public void SettingPreserveAspectMarksVerticesAsDirty()
        {
            m_Image.preserveAspect = true;
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.False(m_dirtyLayout, "Layout has been dirtied");
            Assert.False(m_dirtyMaterial, "Material has been dirtied");
        }

        [Test]
        public void SettingFillCenterMarksVerticesAsDirty()
        {
            m_Image.fillCenter = false;
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.False(m_dirtyLayout, "Layout has been dirtied");
            Assert.False(m_dirtyMaterial, "Material has been dirtied");
        }

        [Test]
        public void SettingFillMethodMarksVerticesAsDirty()
        {
            m_Image.fillMethod = Image.FillMethod.Horizontal;
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.False(m_dirtyLayout, "Layout has been dirtied");
            Assert.False(m_dirtyMaterial, "Material has been dirtied");
        }

        [Test]
        public void SettingFillAmountMarksVerticesAsDirty()
        {
            m_Image.fillAmount = 0.5f;
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.False(m_dirtyLayout, "Layout has been dirtied");
            Assert.False(m_dirtyMaterial, "Material has been dirtied");
        }

        [Test]
        public void SettingFillClockwiseMarksVerticesAsDirty()
        {
            m_Image.fillClockwise = false;
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.False(m_dirtyLayout, "Layout has been dirtied");
            Assert.False(m_dirtyMaterial, "Material has been dirtied");
        }

        [Test]
        public void SettingFillOriginMarksVerticesAsDirty()
        {
            m_Image.fillOrigin = 1;
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.False(m_dirtyLayout, "Layout has been dirtied");
            Assert.False(m_dirtyMaterial, "Material has been dirtied");
        }

        [Test]
        public void SettingEventAlphaThresholdMarksNothingAsDirty()
        {
            m_Image.alphaHitTestMinimumThreshold = 0.5f;
            Assert.False(m_dirtyVert, "Vertices have been dirtied");
            Assert.False(m_dirtyLayout, "Layout has been dirtied");
            Assert.False(m_dirtyMaterial, "Material has been dirtied");
        }

        [Test]
        public void OnAfterDeserializeMakeFillOriginZeroIfNotBetweenZeroAndThree()
        {
            for (int i = -10; i < 0; i++)
            {
                m_Image.fillOrigin = i;
                m_Image.OnAfterDeserialize();
                Assert.AreEqual(0, m_Image.fillOrigin);
            }

            for (int i = 0; i < 4; i++)
            {
                m_Image.fillOrigin = i;
                m_Image.OnAfterDeserialize();
                Assert.AreEqual(i, m_Image.fillOrigin);
            }

            for (int i = 4; i < 10; i++)
            {
                m_Image.fillOrigin = i;
                m_Image.OnAfterDeserialize();
                Assert.AreEqual(0, m_Image.fillOrigin);
            }
        }

        [Test]
        public void OnAfterDeserializeMakeFillOriginZeroIfFillOriginGreaterThan1AndFillMethodHorizontalOrVertical()
        {
            m_Image.fillMethod = Image.FillMethod.Horizontal;
            Image.FillMethod[] fillMethodsToTest = {Image.FillMethod.Horizontal, Image.FillMethod.Vertical};

            foreach (var fillMethod in fillMethodsToTest)
            {
                m_Image.fillMethod = fillMethod;
                for (int i = -10; i < 0; i++)
                {
                    m_Image.fillOrigin = i;
                    m_Image.OnAfterDeserialize();
                    Assert.AreEqual(0, m_Image.fillOrigin);
                }

                for (int i = 0; i < 2; i++)
                {
                    m_Image.fillOrigin = i;
                    m_Image.OnAfterDeserialize();
                    Assert.AreEqual(i, m_Image.fillOrigin);
                }

                for (int i = 2; i < 100; i++)
                {
                    m_Image.fillOrigin = i;
                    m_Image.OnAfterDeserialize();
                    Assert.AreEqual(0, m_Image.fillOrigin);
                }
            }
        }

        [Test]
        public void OnAfterDeserializeClampsFillAmountBetweenZeroAndOne()
        {
            for (float f = -5; f < 0; f += 0.1f)
            {
                m_Image.fillAmount = f;
                m_Image.OnAfterDeserialize();
                Assert.AreEqual(0, m_Image.fillAmount);
            }

            for (float f = 0; f < 1; f += 0.1f)
            {
                m_Image.fillAmount = f;
                m_Image.OnAfterDeserialize();
                Assert.AreEqual(f, m_Image.fillAmount);
            }

            for (float f = 1; f < 5; f += 0.1f)
            {
                m_Image.fillAmount = f;
                m_Image.OnAfterDeserialize();
                Assert.AreEqual(1, m_Image.fillAmount);
            }
        }

        [Test]
        public void SetNativeSizeSetsAllAsDirtyAndSetsAnchorMaxAndSizeDeltaWhenOverrideSpriteIsNotNull()
        {
            m_Image.sprite = m_Sprite;
            m_Image.overrideSprite = m_OverrideSprite;
            m_Image.rectTransform.anchorMax = new Vector2(100, 100);
            m_Image.rectTransform.anchorMin = new Vector2(0, 0);
            m_Image.SetNativeSize();
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.True(m_dirtyLayout, "Layout has not been dirtied");
            Assert.True(m_dirtyMaterial, "Material has not been dirtied");
            Assert.AreEqual(m_Image.rectTransform.anchorMin, m_Image.rectTransform.anchorMax);
            Assert.AreEqual(m_OverrideSprite.rect.size / m_Image.pixelsPerUnit, m_Image.rectTransform.sizeDelta);
        }

        [Test]
        public void OnPopulateMeshWhenNoOverrideSpritePresentDefersToGraphicImplementation()
        {
            m_OverrideSprite = null;
            m_Image.rectTransform.anchoredPosition = new Vector2(100, 452);
            m_Image.rectTransform.sizeDelta = new Vector2(881, 593);
            m_Image.color = new Color(0.1f, 0.2f, 0.8f, 0);

            VertexHelper vh = new VertexHelper();

            m_Image.InvokeOnPopulateMesh(vh);
            Assert.AreEqual(4, vh.currentVertCount);
            List<UIVertex> verts = new List<UIVertex>();
            vh.GetUIVertexStream(verts);

            // The vertices for the 2 triangles of the canvas
            UIVertex[] expectedVertices =
            {
                new UIVertex
                {
                    color = m_Image.color,
                    position = m_Image.rectTransform.rect.min,
                    uv0 = new Vector2(0f, 0f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
                new UIVertex
                {
                    color = m_Image.color,
                    position = new Vector3(m_Image.rectTransform.rect.xMin, m_Image.rectTransform.rect.yMax),
                    uv0 = new Vector2(0f, 1f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
                new UIVertex
                {
                    color = m_Image.color,
                    position = m_Image.rectTransform.rect.max,
                    uv0 = new Vector2(1f, 1f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
                new UIVertex
                {
                    color = m_Image.color,
                    position = m_Image.rectTransform.rect.max,
                    uv0 = new Vector2(1f, 1f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
                new UIVertex
                {
                    color = m_Image.color,
                    position = new Vector3(m_Image.rectTransform.rect.xMax, m_Image.rectTransform.rect.yMin),
                    uv0 = new Vector2(1f, 0f),
                    normal = new Vector3(0, 0, -1),
                    tangent = new Vector4(1, 0, 0, -1)
                },
                new UIVertex
                {
                    color = m_Image.color,
                    position = m_Image.rectTransform.rect.min,
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

        private void TestOnPopulateMeshTypeSimple(VertexHelper vh, Vector4 UVs)
        {
            List<UIVertex> verts = new List<UIVertex>();
            vh.GetUIVertexStream(verts);

            Assert.AreEqual(4, vh.currentVertCount);
            Assert.AreEqual(6, vh.currentIndexCount);

            var imgRect = m_Image.rectTransform.rect;
            Vector3[] expectedVertices =
            {
                imgRect.min,
                new Vector3(imgRect.xMin, imgRect.yMax),
                imgRect.max,
                imgRect.max,
                new Vector3(imgRect.xMax, imgRect.yMin),
                imgRect.min
            };

            Vector2[] expectedUV0s =
            {
                new Vector2(UVs.x, UVs.y),
                new Vector2(UVs.x, UVs.w),
                new Vector2(UVs.z, UVs.w),
                new Vector2(UVs.z, UVs.w),
                new Vector2(UVs.z, UVs.y),
                new Vector2(UVs.x, UVs.y),
            };

            var expectedNormal = new Vector3(0, 0, -1);
            var expectedTangent = new Vector4(1, 0, 0, -1);
            Color32 expectedColor = m_Image.color;
            Vector2 expectedUV1 = new Vector2(0, 0);

            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(expectedVertices[i], verts[i].position);
                Assert.AreEqual(expectedUV0s[i], verts[i].uv0);
                Assert.AreEqual(expectedUV1, verts[i].uv1);
                Assert.AreEqual(expectedNormal, verts[i].normal);
                Assert.AreEqual(expectedTangent, verts[i].tangent);
                Assert.AreEqual(expectedColor, verts[i].color);
            }
        }

        [Test]
        public void OnPopulateMeshWithTypeTiledNoBorderGeneratesExpectedResults()
        {
            m_Image.sprite = m_Sprite;
            m_Image.sprite.texture.wrapMode = TextureWrapMode.Repeat;

            m_Image.type = Image.Type.Tiled;

            VertexHelper vh = new VertexHelper();

            m_Image.InvokeOnPopulateMesh(vh);

            Assert.AreEqual(4, vh.currentVertCount);
            Assert.AreEqual(6, vh.currentIndexCount);
        }

        [Test]
        public void MinWidthHeightAreZeroWithNoImage()
        {
            Assert.AreEqual(0, m_Image.minWidth);
            Assert.AreEqual(0, m_Image.minHeight);
        }

        [Test]
        public void FlexibleWidthHeightAreCorrectWithNoImage()
        {
            Assert.AreEqual(-1, m_Image.flexibleWidth);
            Assert.AreEqual(-1, m_Image.flexibleHeight);
        }

        [Test]
        public void PreferredWidthHeightAreCorrectWithNoImage()
        {
            Assert.AreEqual(0, m_Image.preferredWidth);
            Assert.AreEqual(0, m_Image.preferredHeight);
        }

        [Test]
        public void MinWidthHeightAreZeroWithImage()
        {
            m_Image.sprite = m_Sprite;
            Assert.AreEqual(0, m_Image.minWidth);
            Assert.AreEqual(0, m_Image.minHeight);
        }

        [Test]
        public void FlexibleWidthHeightAreCorrectWithImage()
        {
            m_Image.sprite = m_Sprite;
            Assert.AreEqual(-1, m_Image.flexibleWidth);
            Assert.AreEqual(-1, m_Image.flexibleHeight);
        }

        [Test]
        public void PreferredWidthHeightAreCorrectWithImage()
        {
            m_Image.sprite = m_Sprite;
            Assert.AreEqual(128, m_Image.preferredWidth);
            Assert.AreEqual(128, m_Image.preferredHeight);
        }

        [Test]
        public void MinWidthHeightAreZeroWithOverrideImage()
        {
            m_Image.sprite = m_Sprite;
            m_Image.overrideSprite = m_OverrideSprite;
            Assert.AreEqual(0, m_Image.minWidth);
            Assert.AreEqual(0, m_Image.minHeight);
        }

        [Test]
        public void FlexibleWidthHeightAreCorrectWithOverrideImage()
        {
            m_Image.sprite = m_Sprite;
            m_Image.overrideSprite = m_OverrideSprite;
            Assert.AreEqual(-1, m_Image.flexibleWidth);
            Assert.AreEqual(-1, m_Image.flexibleHeight);
        }

        [Test]
        public void PreferredWidthHeightAreCorrectWithOverrideImage()
        {
            m_Image.sprite = m_Sprite;
            m_Image.overrideSprite = m_OverrideSprite;
            Assert.AreEqual(256, m_Image.preferredWidth);
            Assert.AreEqual(256, m_Image.preferredHeight);
        }
    }
}
