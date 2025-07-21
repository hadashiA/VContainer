using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Graphics
{
    class MaskTests : IPrebuildSetup
    {
        GameObject m_PrefabRoot;
        const string kPrefabPath = "Assets/Resources/MaskTestsPrefab.prefab";

        Mask m_mask;

        public void Setup()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");
            GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasGO.transform.SetParent(rootGO.transform);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.referencePixelsPerUnit = 100;

            var gameObject = new GameObject("Mask", typeof(RectTransform), typeof(Mask), typeof(Image));
            gameObject.transform.SetParent(canvasGO.transform);

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
            GameObject.DestroyImmediate(rootGO);
#endif
        }

        [SetUp]
        public void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("MaskTestsPrefab")) as GameObject;
            m_mask = m_PrefabRoot.GetComponentInChildren<Mask>();
        }

        [TearDown]
        public void TearDown()
        {
            m_mask = null;
            Object.DestroyImmediate(m_PrefabRoot);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabPath);
#endif
        }

        [UnityTest]
        public IEnumerator GetModifiedMaterialReturnsOriginalMaterialWhenNoGraphicComponentIsAttached()
        {
            Object.DestroyImmediate(m_mask.gameObject.GetComponent<Image>());
            yield return null;
            Material material = new Material(Graphic.defaultGraphicMaterial);
            Material modifiedMaterial = m_mask.GetModifiedMaterial(material);
            Assert.AreEqual(material, modifiedMaterial);
        }

        private Dictionary<string, GameObject> CreateMaskHierarchy(string suffix, int hierarchyDepth, out GameObject root)
        {
            var nameToObjectMapping = new Dictionary<string, GameObject>();

            root = new GameObject("root", typeof(RectTransform), typeof(Canvas));
            nameToObjectMapping["root"] = root;

            GameObject current = root;

            for (int i = 0; i < hierarchyDepth; i++)
            {
                string name = suffix + (i + 1);
                var gameObject = new GameObject(name, typeof(RectTransform), typeof(Mask), typeof(Image));
                gameObject.transform.SetParent(current.transform);
                nameToObjectMapping[name] = gameObject;
                current = gameObject;
            }

            return nameToObjectMapping;
        }

        [Test]
        public void GetModifiedMaterialReturnsOriginalMaterialWhenDepthIsEightOrMore()
        {
            GameObject root;
            var objectsMap = CreateMaskHierarchy("subMask", 9, out root);
            Mask mask = objectsMap["subMask" + 9].GetComponent<Mask>();
            Material material = new Material(Graphic.defaultGraphicMaterial);
            Material modifiedMaterial = mask.GetModifiedMaterial(material);
            Assert.AreEqual(material, modifiedMaterial);
            GameObject.DestroyImmediate(root);
        }

        [Test]
        public void GetModifiedMaterialReturnsDesiredMaterialWithSingleMask()
        {
            Material material = new Material(Graphic.defaultGraphicMaterial);
            Material modifiedMaterial = m_mask.GetModifiedMaterial(material);

            Assert.AreNotEqual(material, modifiedMaterial);
            Assert.AreEqual(1, modifiedMaterial.GetInt("_Stencil"));
            Assert.AreEqual(StencilOp.Replace, (StencilOp)modifiedMaterial.GetInt("_StencilOp"));
            Assert.AreEqual(CompareFunction.Always, (CompareFunction)modifiedMaterial.GetInt("_StencilComp"));
            Assert.AreEqual(255, modifiedMaterial.GetInt("_StencilReadMask"));
            Assert.AreEqual(255, modifiedMaterial.GetInt("_StencilWriteMask"));
            Assert.AreEqual(ColorWriteMask.All, (ColorWriteMask)modifiedMaterial.GetInt("_ColorMask"));
            Assert.AreEqual(1, modifiedMaterial.GetInt("_UseUIAlphaClip"));

            Assert.IsTrue(modifiedMaterial.IsKeywordEnabled("UNITY_UI_ALPHACLIP"));
        }

        [Test]
        public void GetModifiedMaterialReturnsDesiredMaterialWithMultipleMasks()
        {
            for (int i = 2; i < 8; i++)
            {
                GameObject root;
                var objectsMap = CreateMaskHierarchy("subMask", i, out root);
                Mask mask = objectsMap["subMask" + i].GetComponent<Mask>();

                int stencilDepth = MaskUtilities.GetStencilDepth(mask.transform, objectsMap["root"].transform);

                int desiredStencilBit = 1 << stencilDepth;
                Material material = new Material(Graphic.defaultGraphicMaterial);
                Material modifiedMaterial = mask.GetModifiedMaterial(material);
                int stencil = modifiedMaterial.GetInt("_Stencil");

                Assert.AreNotEqual(material, modifiedMaterial);
                Assert.AreEqual(desiredStencilBit | (desiredStencilBit - 1), stencil);
                Assert.AreEqual(StencilOp.Replace, (StencilOp)modifiedMaterial.GetInt("_StencilOp"));
                Assert.AreEqual(CompareFunction.Equal, (CompareFunction)modifiedMaterial.GetInt("_StencilComp"));
                Assert.AreEqual(desiredStencilBit - 1, modifiedMaterial.GetInt("_StencilReadMask"));
                Assert.AreEqual(desiredStencilBit | (desiredStencilBit - 1), modifiedMaterial.GetInt("_StencilWriteMask"));
                Assert.AreEqual(ColorWriteMask.All, (ColorWriteMask)modifiedMaterial.GetInt("_ColorMask"));
                Assert.AreEqual(1, modifiedMaterial.GetInt("_UseUIAlphaClip"));
                Assert.IsTrue(modifiedMaterial.IsKeywordEnabled("UNITY_UI_ALPHACLIP"));

                GameObject.DestroyImmediate(root);
            }
        }

        [Test]
        public void GraphicComponentWithMaskIsMarkedAsIsMaskingGraphicWhenEnabled()
        {
            var graphic = m_PrefabRoot.GetComponentInChildren<Image>();
            Assert.AreEqual(true, graphic.isMaskingGraphic);
            m_mask.enabled = false;
            Assert.AreEqual(false, graphic.isMaskingGraphic);
        }
    }
}
