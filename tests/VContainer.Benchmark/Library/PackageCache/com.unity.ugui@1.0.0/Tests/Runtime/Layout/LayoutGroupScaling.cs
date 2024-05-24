using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// test for case 879374 - Checks that layout group children scale properly when scaleWidth / scaleHeight are toggled
namespace LayoutTests
{
    public class LayoutGroupScaling : IPrebuildSetup
    {
        GameObject m_PrefabRoot;
        GameObject m_CameraGO;

        const string kPrefabPath = "Assets/Resources/LayoutGroupScalingPrefab.prefab";

        public void Setup()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("RootGO");
            var rootCanvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
            rootCanvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvasGO.transform.SetParent(rootGO.transform);

            var layoutGroupGO = new GameObject("LayoutGroup");
            layoutGroupGO.transform.SetParent(rootCanvasGO.transform);
            HorizontalLayoutGroup layoutGroup = layoutGroupGO.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            ContentSizeFitter contentSizeFitter = layoutGroupGO.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < 10; i++)
            {
                var elementGO = new GameObject("image(" + i + ")", typeof(Image));
                var layoutElement = elementGO.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = 50;
                layoutElement.preferredHeight = 50;
                elementGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                elementGO.transform.SetParent(layoutGroupGO.transform);
            }

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            UnityEditor.PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
            GameObject.DestroyImmediate(rootGO);

#endif
        }

        [SetUp]
        public void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("LayoutGroupScalingPrefab")) as GameObject;
            m_CameraGO = new GameObject("Camera", typeof(Camera));
        }

        [UnityTest]
        public IEnumerator LayoutGroup_CorrectChildScaling()
        {
            GameObject layoutGroupGO = m_PrefabRoot.GetComponentInChildren<HorizontalLayoutGroup>().gameObject;
            Rect dimentions = (layoutGroupGO.transform as RectTransform).rect;
            layoutGroupGO.GetComponent<HorizontalLayoutGroup>().childScaleWidth = true;
            layoutGroupGO.GetComponent<HorizontalLayoutGroup>().childScaleHeight = true;
            yield return null;
            Rect newDimentions = (layoutGroupGO.transform as RectTransform).rect;
            Assert.IsTrue(Mathf.Approximately(dimentions.width * 0.5f, newDimentions.width));
            Assert.IsTrue(Mathf.Approximately(dimentions.height * 0.5f, newDimentions.height));
            yield return null;
            Object.DestroyImmediate(layoutGroupGO.GetComponent<HorizontalLayoutGroup>());
            VerticalLayoutGroup layoutGroup = layoutGroupGO.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            yield return null;
            dimentions = (layoutGroupGO.transform as RectTransform).rect;
            layoutGroup.childScaleWidth = true;
            layoutGroup.childScaleHeight = true;
            yield return null;
            newDimentions = (layoutGroupGO.transform as RectTransform).rect;
            Assert.IsTrue(Mathf.Approximately(dimentions.width * 0.5f, newDimentions.width));
            Assert.IsTrue(Mathf.Approximately(dimentions.height * 0.5f, newDimentions.height));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_PrefabRoot);
            GameObject.DestroyImmediate(m_CameraGO);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabPath);
#endif
        }
    }
}
