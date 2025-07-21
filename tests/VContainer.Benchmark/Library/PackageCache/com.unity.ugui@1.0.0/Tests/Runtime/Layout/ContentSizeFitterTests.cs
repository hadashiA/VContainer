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
    class ContentSizeFitterTests : IPrebuildSetup
    {
        const string kPrefabPath = "Assets/Resources/ContentSizeFitterTests.prefab";

        private GameObject m_PrefabRoot;
        private ContentSizeFitter m_ContentSizeFitter;
        private RectTransform m_RectTransform;

        public void Setup()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");
            GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasGO.transform.SetParent(rootGO.transform);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.referencePixelsPerUnit = 100;

            var testGO = new GameObject("TestObject", typeof(RectTransform), typeof(ContentSizeFitter));
            testGO.transform.SetParent(canvasGO.transform);

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
            GameObject.DestroyImmediate(rootGO);
#endif
        }

        [SetUp]
        public void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("ContentSizeFitterTests")) as GameObject;
            m_ContentSizeFitter = m_PrefabRoot.GetComponentInChildren<ContentSizeFitter>();

            m_ContentSizeFitter.enabled = true;

            m_RectTransform = m_ContentSizeFitter.GetComponent<RectTransform>();
            m_RectTransform.sizeDelta = new Vector2(50, 50);

            GameObject testObject = m_ContentSizeFitter.gameObject;
            // set up components
            var componentA = testObject.AddComponent<LayoutElement>();
            componentA.minWidth = 5;
            componentA.minHeight = 10;
            componentA.preferredWidth = 100;
            componentA.preferredHeight = 105;
            componentA.flexibleWidth = 0;
            componentA.flexibleHeight = 0;
            componentA.enabled = true;

            var componentB = testObject.AddComponent<LayoutElement>();
            componentB.minWidth = 15;
            componentB.minHeight = 20;
            componentB.preferredWidth = 110;
            componentB.preferredHeight = 115;
            componentB.flexibleWidth = 0;
            componentB.flexibleHeight = 0;
            componentB.enabled = true;

            var componentC = testObject.AddComponent<LayoutElement>();
            componentC.minWidth = 25;
            componentC.minHeight = 30;
            componentC.preferredWidth = 120;
            componentC.preferredHeight = 125;
            componentC.flexibleWidth = 0;
            componentC.flexibleHeight = 0;
            componentC.enabled = true;
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_PrefabRoot);
            m_PrefabRoot = null;
            m_ContentSizeFitter = null;
            m_RectTransform = null;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabPath);
#endif
        }

        [Test]
        public void TestFitModeUnconstrained()
        {
            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(50, m_RectTransform.rect.width);
            Assert.AreEqual(50, m_RectTransform.rect.height);
        }

        [Test]
        public void TestFitModeMinSize()
        {
            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(25, m_RectTransform.rect.width);
            Assert.AreEqual(30, m_RectTransform.rect.height);
        }

        [Test]
        public void TestFitModePreferredSize()
        {
            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(120, m_RectTransform.rect.width);
            Assert.AreEqual(125, m_RectTransform.rect.height);
        }
    }
}
