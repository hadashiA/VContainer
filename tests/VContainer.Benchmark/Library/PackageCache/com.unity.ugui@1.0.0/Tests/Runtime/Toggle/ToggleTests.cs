using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UI.Tests;
using Object = UnityEngine.Object;

namespace ToggleTest
{
    class ToggleTests : IPrebuildSetup
    {
        const string kPrefabTogglePath = "Assets/Resources/TestToggle.prefab";

        protected GameObject m_PrefabRoot;
        protected List<Toggle> m_toggle = new List<Toggle>();
        protected static int nbToggleInGroup = 2;

        public void Setup()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");

            GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasGO.transform.SetParent(rootGO.transform);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.referencePixelsPerUnit = 100;

            var toggleGO = new GameObject("TestToggle", typeof(RectTransform), typeof(Toggle), typeof(Image));
            toggleGO.transform.SetParent(canvasGO.transform);

            var toggle = toggleGO.GetComponent<Toggle>();
            toggle.enabled = true;
            toggle.graphic = toggleGO.GetComponent<Image>();
            toggle.graphic.canvasRenderer.SetColor(Color.white);

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabTogglePath);

            GameObject.DestroyImmediate(rootGO);
#endif
        }

        [SetUp]
        public virtual void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("TestToggle")) as GameObject;
            m_toggle.Add(m_PrefabRoot.GetComponentInChildren<Toggle>());
        }

        [TearDown]
        public virtual void TearDown()
        {
            m_toggle.Clear();
            Object.DestroyImmediate(m_PrefabRoot);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabTogglePath);
#endif
        }

        [Test]
        public void SetIsOnWithoutNotifyWillNotNotify()
        {
            m_toggle[0].isOn = false;
            bool calledOnValueChanged = false;
            m_toggle[0].onValueChanged.AddListener(b => { calledOnValueChanged = true; });
            m_toggle[0].SetIsOnWithoutNotify(true);
            Assert.IsTrue(m_toggle[0].isOn);
            Assert.IsFalse(calledOnValueChanged);
        }

        [Test]
        public void NonInteractableCantBeToggled()
        {
            m_toggle[0].isOn = true;
            Assert.IsTrue(m_toggle[0].isOn);
            m_toggle[0].interactable = false;
            m_toggle[0].OnSubmit(null);
            Assert.IsTrue(m_toggle[0].isOn);
        }

        [Test]
        public void InactiveCantBeToggled()
        {
            m_toggle[0].isOn = true;
            Assert.IsTrue(m_toggle[0].isOn);
            m_toggle[0].enabled = false;
            m_toggle[0].OnSubmit(null);
            Assert.IsTrue(m_toggle[0].isOn);
        }
    }
}
