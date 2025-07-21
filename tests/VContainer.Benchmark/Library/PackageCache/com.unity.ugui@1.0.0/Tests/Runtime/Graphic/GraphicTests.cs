using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UI.Tests;

namespace Graphics
{
    class GraphicTests : IPrebuildSetup
    {
        private GameObject m_PrefabRoot;
        private ConcreteGraphic m_graphic;
        private Canvas m_canvas;

        const string kPrefabPath = "Assets/Resources/GraphicTestsPrefab.prefab";

        bool m_dirtyVert;
        bool m_dirtyLayout;
        bool m_dirtyMaterial;

        public void Setup()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");
            var canvasGO = new GameObject("CanvasRoot", typeof(Canvas), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(rootGO.transform);

            var graphicGO = new GameObject("Graphic", typeof(RectTransform), typeof(ConcreteGraphic));
            graphicGO.transform.SetParent(canvasGO.transform);

            var gameObject = new GameObject("EventSystem", typeof(EventSystem));
            gameObject.transform.SetParent(rootGO.transform);

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);

            GameObject.DestroyImmediate(rootGO);
#endif
        }

        [SetUp]
        public void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("GraphicTestsPrefab")) as GameObject;

            m_canvas = m_PrefabRoot.GetComponentInChildren<Canvas>();
            m_graphic = m_PrefabRoot.GetComponentInChildren<ConcreteGraphic>();

            m_graphic.RegisterDirtyVerticesCallback(() => m_dirtyVert = true);
            m_graphic.RegisterDirtyLayoutCallback(() => m_dirtyLayout = true);
            m_graphic.RegisterDirtyMaterialCallback(() => m_dirtyMaterial = true);

            ResetDirtyFlags();
        }

        [TearDown]
        public void TearDown()
        {
            m_graphic = null;
            m_canvas = null;
            GameObject.DestroyImmediate(m_PrefabRoot);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabPath);
#endif
        }

        private void ResetDirtyFlags()
        {
            m_dirtyVert = m_dirtyLayout = m_dirtyMaterial = false;
        }

        [Test]
        public void SettingDirtyOnActiveGraphicCallsCallbacks()
        {
            m_graphic.enabled = false;
            m_graphic.enabled = true;
            m_graphic.SetAllDirty();

            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.True(m_dirtyLayout, "Layout has not been dirtied");
            Assert.True(m_dirtyMaterial, "Material has not been dirtied");

            ResetDirtyFlags();

            m_graphic.SetLayoutDirty();
            Assert.True(m_dirtyLayout, "Layout has not been dirtied");

            m_graphic.SetVerticesDirty();
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");

            m_graphic.SetMaterialDirty();
            Assert.True(m_dirtyMaterial, "Material has not been dirtied");
        }

        private bool ContainsGraphic(IList<Graphic> array, int size, Graphic element)
        {
            for (int i = 0; i < size; ++i)
            {
                if (array[i] == element)
                    return true;
            }

            return false;
        }

        private void RefreshGraphicByRaycast()
        {
            // force refresh by raycast
            List<RaycastResult> raycastResultCache = new List<RaycastResult>();
            PointerEventData data = new PointerEventData(EventSystem.current);
            var raycaster = m_canvas.GetComponent<GraphicRaycaster>();
            raycaster.Raycast(data, raycastResultCache);
        }

        private bool CheckGraphicAddedToGraphicRegistry()
        {
            var graphicList = GraphicRegistry.GetGraphicsForCanvas(m_canvas);
            var graphicListSize = graphicList.Count;
            return ContainsGraphic(graphicList, graphicListSize, m_graphic);
        }

        private bool CheckGraphicAddedToRaycastGraphicRegistry()
        {
            var graphicList = GraphicRegistry.GetRaycastableGraphicsForCanvas(m_canvas);
            var graphicListSize = graphicList.Count;
            return ContainsGraphic(graphicList, graphicListSize, m_graphic);
        }

        private void CheckGraphicRaycastDisableValidity()
        {
            RefreshGraphicByRaycast();
            Assert.False(CheckGraphicAddedToGraphicRegistry(), "Graphic should no longer be registered in m_CanvasGraphics");
            Assert.False(CheckGraphicAddedToRaycastGraphicRegistry(), "Graphic should no longer be registered in m_RaycastableGraphics");
        }

        [Test]
        public void OnEnableLeavesGraphicInExpectedState()
        {
            m_graphic.enabled = false;
            m_graphic.enabled = true; // on enable is called directly

            Assert.True(CheckGraphicAddedToGraphicRegistry(), "Graphic should be registered in m_CanvasGraphics");
            Assert.True(CheckGraphicAddedToRaycastGraphicRegistry(), "Graphic should be registered in m_RaycastableGraphics");

            Assert.AreEqual(Texture2D.whiteTexture, m_graphic.mainTexture, "mainTexture should be Texture2D.whiteTexture");

            Assert.NotNull(m_graphic.canvas);

            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.True(m_dirtyLayout, "Layout has not been dirtied");
            Assert.True(m_dirtyMaterial, "Material has not been dirtied");
        }

        [Test]
        public void OnEnableTwiceLeavesGraphicInExpectedState()
        {
            m_graphic.enabled = true;

            // force onEnable by reflection to call it second time
            m_graphic.InvokeOnEnable();

            m_graphic.enabled = false;

            CheckGraphicRaycastDisableValidity();
        }

        [Test]
        public void OnDisableLeavesGraphicInExpectedState()
        {
            m_graphic.enabled = true;
            m_graphic.enabled = false;

            CheckGraphicRaycastDisableValidity();
        }

        [Test]
        public void OnPopulateMeshWorksAsExpected()
        {
            m_graphic.rectTransform.anchoredPosition = new Vector2(100, 100);
            m_graphic.rectTransform.sizeDelta = new Vector2(150, 742);
            m_graphic.color = new Color(50, 100, 150, 200);

            VertexHelper vh = new VertexHelper();
            m_graphic.InvokeOnPopulateMesh(vh);

            GraphicTestHelper.TestOnPopulateMeshDefaultBehavior(m_graphic, vh);
        }

        [Test]
        public void OnDidApplyAnimationPropertiesSetsAllDirty()
        {
            m_graphic.enabled = true; // Usually set through SetEnabled, from Behavior

            m_graphic.InvokeOnDidApplyAnimationProperties();
            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.True(m_dirtyLayout, "Layout has not been dirtied");
            Assert.True(m_dirtyMaterial, "Material has not been dirtied");
        }

        [Test]
        public void MakingGraphicNonRaycastableRemovesGraphicFromProperLists()
        {
            Assert.True(CheckGraphicAddedToGraphicRegistry(), "Graphic should be registered in m_CanvasGraphics");
            Assert.True(CheckGraphicAddedToRaycastGraphicRegistry(), "Graphic should be registered in m_RaycastableGraphics");

            m_graphic.raycastTarget = false;

            Assert.True(CheckGraphicAddedToGraphicRegistry(), "Graphic should be registered in m_CanvasGraphics");
            Assert.False(CheckGraphicAddedToRaycastGraphicRegistry(), "Graphic should no longer be registered in m_RaycastableGraphics");
        }

        [Test]
        public void OnEnableLeavesNonRaycastGraphicInExpectedState()
        {
            m_graphic.enabled = false;
            m_graphic.raycastTarget = false;
            m_graphic.enabled = true; // on enable is called directly

            Assert.True(CheckGraphicAddedToGraphicRegistry(), "Graphic should be registered in m_CanvasGraphics");
            Assert.False(CheckGraphicAddedToRaycastGraphicRegistry(), "Graphic should no longer be registered in m_RaycastableGraphics");

            Assert.AreEqual(Texture2D.whiteTexture, m_graphic.mainTexture, "mainTexture should be Texture2D.whiteTexture");

            Assert.NotNull(m_graphic.canvas);

            Assert.True(m_dirtyVert, "Vertices have not been dirtied");
            Assert.True(m_dirtyLayout, "Layout has not been dirtied");
            Assert.True(m_dirtyMaterial, "Material has not been dirtied");
        }

        [Test]
        public void SettingRaycastTargetOnDisabledGraphicDoesntAddItRaycastList()
        {
            Assert.True(CheckGraphicAddedToGraphicRegistry(), "Graphic should be registered in m_CanvasGraphics");
            Assert.True(CheckGraphicAddedToRaycastGraphicRegistry(), "Graphic should be registered in m_RaycastableGraphics");

            m_graphic.raycastTarget = false;

            Assert.True(CheckGraphicAddedToGraphicRegistry(), "Graphic should be registered in m_CanvasGraphics");
            Assert.False(CheckGraphicAddedToRaycastGraphicRegistry(), "Graphic should no longer be registered in m_RaycastableGraphics");

            m_graphic.enabled = false;

            Assert.False(CheckGraphicAddedToGraphicRegistry(), "Graphic should NOT be registered in m_CanvasGraphics");
            Assert.False(CheckGraphicAddedToRaycastGraphicRegistry(), "Graphic should no longer be registered in m_RaycastableGraphics");

            m_graphic.raycastTarget = true;

            Assert.False(CheckGraphicAddedToGraphicRegistry(), "Graphic should NOT be registered in m_CanvasGraphics");
            Assert.False(CheckGraphicAddedToRaycastGraphicRegistry(), "Graphic should no longer be registered in m_RaycastableGraphics");
        }
    }
}
