using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using System.Reflection;

namespace InputfieldTests
{
    public class BaseInputFieldTests
    {
        protected GameObject m_PrefabRoot;

        public void CreateInputFieldAsset(string prefabPath)
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");

            var canvasGO = new GameObject("Canvas", typeof(Canvas));
            canvasGO.transform.SetParent(rootGO.transform);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.referencePixelsPerUnit = 100;

            GameObject inputFieldGO = new GameObject("InputField", typeof(RectTransform), typeof(InputField));
            inputFieldGO.transform.SetParent(canvasGO.transform);

            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(inputFieldGO.transform);

            GameObject eventSystemGO = new GameObject("EventSystem", typeof(EventSystem), typeof(FakeInputModule));
            eventSystemGO.transform.SetParent(rootGO.transform);

            InputField inputField = inputFieldGO.GetComponent<InputField>();

            inputField.interactable = true;
            inputField.enabled = true;
            inputField.textComponent = textGO.GetComponent<Text>();
            inputField.textComponent.fontSize = 12;
            inputField.textComponent.supportRichText = false;

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, prefabPath);
            GameObject.DestroyImmediate(rootGO);
#endif
        }
    }
}
