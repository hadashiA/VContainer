using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UI.Tests;

namespace LayoutTests
{
    class AspectRatioFitterTests : IPrebuildSetup
    {
        const string kPrefabPath = "Assets/Resources/AspectRatioFitterTestsEnvelopeParent.prefab";
        const string kPrefabPath2 = "Assets/Resources/AspectRatioFitterTestsFitInParent.prefab";

        private GameObject m_PrefabRoot;
        private AspectRatioFitter m_AspectRatioFitter;
        private RectTransform m_RectTransform;

        [SetUp]
        public void Setup()
        {
#if UNITY_EDITOR
            var cameraGo = new GameObject("Cam").AddComponent<Camera>();

            var panelGO = new GameObject("PanelObject", typeof(RectTransform));
            var panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(200, 200);

            var panelGO2 = new GameObject("PanelObject", typeof(RectTransform));
            var panelRT2 = panelGO2.GetComponent<RectTransform>();
            panelRT2.sizeDelta = new Vector2(200, 200);

            var testGO = new GameObject("TestObject", typeof(RectTransform), typeof(AspectRatioFitter));
            var image = testGO.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets\\download.jpeg");
            m_AspectRatioFitter = testGO.GetComponent<AspectRatioFitter>();
            m_AspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            m_AspectRatioFitter.aspectRatio = 1.5f;
            testGO.transform.SetParent(panelGO.transform);

            var testGO2 = new GameObject("TestObject", typeof(RectTransform), typeof(AspectRatioFitter));
            var image2 = testGO2.AddComponent<Image>();
            image2.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets\\download.jpeg");
            m_AspectRatioFitter = testGO2.GetComponent<AspectRatioFitter>();
            m_AspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            m_AspectRatioFitter.aspectRatio = 1.5f;
            testGO2.transform.SetParent(panelGO2.transform);

            var testRT = testGO.GetComponent<RectTransform>();
            testRT.sizeDelta = new Vector2(150, 100);

            var testRT2 = testGO2.GetComponent<RectTransform>();
            testRT2.sizeDelta = new Vector2(150, 100);

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(panelGO, kPrefabPath);
            PrefabUtility.SaveAsPrefabAsset(panelGO2, kPrefabPath2);
            GameObject.DestroyImmediate(panelGO);
            GameObject.DestroyImmediate(panelGO2);
#endif
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_PrefabRoot);
            m_PrefabRoot = null;
            m_AspectRatioFitter = null;
            m_RectTransform = null;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabPath);
            AssetDatabase.DeleteAsset(kPrefabPath2);
#endif
        }

        [Test]
        public void TestEnvelopParent()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("AspectRatioFitterTestsEnvelopeParent")) as GameObject;
            m_AspectRatioFitter = m_PrefabRoot.GetComponentInChildren<AspectRatioFitter>();

            m_AspectRatioFitter.enabled = true;

            m_RectTransform = m_AspectRatioFitter.GetComponent<RectTransform>();
            Assert.AreEqual(300, m_RectTransform.rect.width);
        }

        [Test]
        public void TestFitInParent()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("AspectRatioFitterTestsFitInParent")) as GameObject;
            m_AspectRatioFitter = m_PrefabRoot.GetComponentInChildren<AspectRatioFitter>();

            m_AspectRatioFitter.enabled = true;

            m_RectTransform = m_AspectRatioFitter.GetComponent<RectTransform>();
            Assert.AreEqual(200, m_RectTransform.rect.width);
        }
    }
}
