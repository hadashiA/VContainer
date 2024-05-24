using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    class ToggleGroupTests : IPrebuildSetup
    {
        const string kPrefabToggleGroupPath = "Assets/Resources/TestToggleGroup.prefab";

        protected GameObject m_PrefabRoot;
        protected List<Toggle> m_toggle = new List<Toggle>();
        protected static int nbToggleInGroup = 2;
        private TestableToggleGroup m_toggleGroup;

        public void Setup()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");
            GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasGO.transform.SetParent(rootGO.transform);

            var toggleGroupGO = new GameObject("ToggleGroup", typeof(RectTransform), typeof(TestableToggleGroup));
            toggleGroupGO.transform.SetParent(canvasGO.transform);
            toggleGroupGO.AddComponent(typeof(TestableToggleGroup));

            var toggle0GO = new GameObject("TestToggle0", typeof(RectTransform), typeof(Toggle), typeof(Image));
            toggle0GO.transform.SetParent(toggleGroupGO.transform);

            var toggle1GO = new GameObject("TestToggle1", typeof(RectTransform), typeof(Toggle), typeof(Image));
            toggle1GO.transform.SetParent(toggleGroupGO.transform);

            var toggle = toggle0GO.GetComponent<Toggle>();
            toggle.graphic = toggle0GO.GetComponent<Image>();
            toggle.graphic.canvasRenderer.SetColor(Color.white);

            var toggle1 = toggle1GO.GetComponent<Toggle>();
            toggle1.graphic = toggle1GO.GetComponent<Image>();
            toggle1.graphic.canvasRenderer.SetColor(Color.white);


            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabToggleGroupPath);

            GameObject.DestroyImmediate(rootGO);
#endif
        }

        [SetUp]
        public void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("TestToggleGroup")) as GameObject;

            m_toggleGroup = m_PrefabRoot.GetComponentInChildren<TestableToggleGroup>();
            m_toggle.AddRange(m_PrefabRoot.GetComponentsInChildren<Toggle>());
        }

        [TearDown]
        public void TearDown()
        {
            m_toggle.Clear();
            m_toggleGroup = null;
            Object.DestroyImmediate(m_PrefabRoot);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabToggleGroupPath);
#endif
        }

        [Test]
        public void TogglingOneShouldDisableOthersInGroup()
        {
            m_toggle[0].group = m_toggleGroup;
            m_toggle[1].group = m_toggleGroup;
            m_toggle[0].isOn = true;
            m_toggle[1].isOn = true;
            Assert.IsFalse(m_toggle[0].isOn);
            Assert.IsTrue(m_toggle[1].isOn);
        }

        [Test]
        public void DisallowSwitchOffShouldKeepToggleOnWhenClicking()
        {
            m_toggle[0].group = m_toggleGroup;
            m_toggle[1].group = m_toggleGroup;
            m_toggle[0].isOn = true;
            Assert.IsTrue(m_toggle[0].isOn);
            m_toggle[0].OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            Assert.IsTrue(m_toggle[0].isOn);
            Assert.IsFalse(m_toggle[1].isOn);
        }

        [Test]
        public void DisallowSwitchOffShouldDisableToggleWhenClicking()
        {
            m_toggleGroup.allowSwitchOff = true;
            m_toggle[0].group = m_toggleGroup;
            m_toggle[1].group = m_toggleGroup;
            m_toggle[0].isOn = true;
            Assert.IsTrue(m_toggle[0].isOn);
            m_toggle[0].OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            Assert.IsFalse(m_toggle[0].isOn);
            Assert.IsFalse(m_toggle[1].isOn);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void ReEnablingGameObjectWithToggleGroupRetainsPreviouslySelectedToggle(int toggleIndex)
        {
            m_toggle[0].group = m_toggleGroup;
            m_toggle[1].group = m_toggleGroup;

            m_toggle[toggleIndex].isOn = true;
            m_PrefabRoot.SetActive(false);
            m_PrefabRoot.SetActive(true);
            Assert.IsTrue(m_toggle[toggleIndex].isOn);
        }

        [Test]
        public void ChangingToggleGroupUnregistersFromOriginalGroup()
        {
            m_toggle[0].group = m_toggleGroup;
            Assert.IsTrue(m_toggleGroup.ToggleListContains(m_toggle[0]));
            m_toggle[0].group = null;
            Assert.IsFalse(m_toggleGroup.ToggleListContains(m_toggle[0]));
        }

        [Test]
        public void DisabledToggleGroupDoesntControlChildren()
        {
            m_toggleGroup.enabled = false;
            m_toggle[0].group = m_toggleGroup;
            m_toggle[1].group = m_toggleGroup;
            m_toggle[0].isOn = true;
            m_toggle[1].isOn = true;
            Assert.IsTrue(m_toggle[0].isOn);
            Assert.IsTrue(m_toggle[1].isOn);

            m_toggleGroup.enabled = true;
            IEnumerable<Toggle> activeToggles = m_toggleGroup.ActiveToggles();
            Assert.IsTrue(activeToggles.Count() == 1);
        }
    }
}
