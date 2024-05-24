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
    public class GenericInputFieldTests : BaseInputFieldTests, IPrebuildSetup
    {
        protected const string kPrefabPath = "Assets/Resources/GenericInputFieldPrefab.prefab";

        public void Setup()
        {
#if UNITY_EDITOR
            CreateInputFieldAsset(kPrefabPath);
#endif
        }

        [SetUp]
        public virtual void TestSetup()
        {
            m_PrefabRoot = UnityEngine.Object.Instantiate(Resources.Load("GenericInputFieldPrefab")) as GameObject;
        }

        [TearDown]
        public virtual void TearDown()
        {
            FontUpdateTracker.UntrackText(m_PrefabRoot.GetComponentInChildren<Text>());
            GameObject.DestroyImmediate(m_PrefabRoot);
        }

        [OneTimeTearDown]
        public void OnetimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabPath);
#endif
        }

        [UnityTest]
        public IEnumerator CannotFocusIfNotTextComponent()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            BaseEventData eventData = new BaseEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>());
            inputField.textComponent = null;

            inputField.OnSelect(eventData);
            yield return null;

            Assert.False(inputField.isFocused);
        }

        [UnityTest]
        public IEnumerator CannotFocusIfNullFont()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            BaseEventData eventData = new BaseEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>());
            inputField.textComponent.font = null;

            inputField.OnSelect(eventData);
            yield return null;

            Assert.False(inputField.isFocused);
        }

        [UnityTest]
        public IEnumerator CannotFocusIfNotActive()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            BaseEventData eventData = new BaseEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>());
            inputField.enabled = false;

            inputField.OnSelect(eventData);
            yield return null;

            Assert.False(inputField.isFocused);
        }

        [UnityTest]
        public IEnumerator CannotFocusWithoutEventSystem()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            UnityEngine.Object.DestroyImmediate(m_PrefabRoot.GetComponentInChildren<FakeInputModule>());

            yield return null;

            UnityEngine.Object.DestroyImmediate(m_PrefabRoot.GetComponentInChildren<EventSystem>());
            BaseEventData eventData = new BaseEventData(null);

            yield return null;

            inputField.OnSelect(eventData);
            yield return null;

            Assert.False(inputField.isFocused);
        }

        [Test]
        [UnityPlatform(exclude = new[] { RuntimePlatform.Switch })] // Currently InputField.ActivateInputFieldInternal calls Switch SoftwareKeyboard screen ; without user input or a command to close the SoftwareKeyboard this blocks the tests suite
        public void FocusesOnSelect()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            BaseEventData eventData = new BaseEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>());
            inputField.OnSelect(eventData);

            MethodInfo lateUpdate = typeof(InputField).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
            lateUpdate.Invoke(inputField, null);

            Assert.True(inputField.isFocused);
        }

        [Test]
        public void DoesNotFocusesOnSelectWhenShouldActivateOnSelect_IsFalse()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            inputField.shouldActivateOnSelect = false;
            BaseEventData eventData = new BaseEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>());
            inputField.OnSelect(eventData);

            MethodInfo lateUpdate = typeof(InputField).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
            lateUpdate.Invoke(inputField, null);

            Assert.False(inputField.isFocused);
        }

        [Test]
        public void InputFieldSetTextWithoutNotifyWillNotNotify()
        {
            InputField i = m_PrefabRoot.GetComponentInChildren<InputField>();
            i.text = "Hello";

            bool calledOnValueChanged = false;

            i.onValueChanged.AddListener(s => { calledOnValueChanged = true; });

            i.SetTextWithoutNotify("Goodbye");

            Assert.IsTrue(i.text == "Goodbye");
            Assert.IsFalse(calledOnValueChanged);
        }

        [Test]
        public void ContentTypeSetsValues()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            inputField.contentType = InputField.ContentType.Standard;
            Assert.AreEqual(InputField.InputType.Standard, inputField.inputType);
            Assert.AreEqual(TouchScreenKeyboardType.Default, inputField.keyboardType);
            Assert.AreEqual(InputField.CharacterValidation.None, inputField.characterValidation);

            inputField.contentType = InputField.ContentType.Autocorrected;
            Assert.AreEqual(InputField.InputType.AutoCorrect, inputField.inputType);
            Assert.AreEqual(TouchScreenKeyboardType.Default, inputField.keyboardType);
            Assert.AreEqual(InputField.CharacterValidation.None, inputField.characterValidation);

            inputField.contentType = InputField.ContentType.IntegerNumber;
            Assert.AreEqual(InputField.LineType.SingleLine, inputField.lineType);
            Assert.AreEqual(InputField.InputType.Standard, inputField.inputType);
            Assert.AreEqual(TouchScreenKeyboardType.NumberPad, inputField.keyboardType);
            Assert.AreEqual(InputField.CharacterValidation.Integer, inputField.characterValidation);

            inputField.contentType = InputField.ContentType.DecimalNumber;
            Assert.AreEqual(InputField.LineType.SingleLine, inputField.lineType);
            Assert.AreEqual(InputField.InputType.Standard, inputField.inputType);
            Assert.AreEqual(TouchScreenKeyboardType.NumbersAndPunctuation, inputField.keyboardType);
            Assert.AreEqual(InputField.CharacterValidation.Decimal, inputField.characterValidation);

            inputField.contentType = InputField.ContentType.Alphanumeric;
            Assert.AreEqual(InputField.LineType.SingleLine, inputField.lineType);
            Assert.AreEqual(InputField.InputType.Standard, inputField.inputType);
            Assert.AreEqual(TouchScreenKeyboardType.ASCIICapable, inputField.keyboardType);
            Assert.AreEqual(InputField.CharacterValidation.Alphanumeric, inputField.characterValidation);

            inputField.contentType = InputField.ContentType.Name;
            Assert.AreEqual(InputField.LineType.SingleLine, inputField.lineType);
            Assert.AreEqual(InputField.InputType.Standard, inputField.inputType);
            Assert.AreEqual(TouchScreenKeyboardType.NamePhonePad, inputField.keyboardType);
            Assert.AreEqual(InputField.CharacterValidation.Name, inputField.characterValidation);

            inputField.contentType = InputField.ContentType.EmailAddress;
            Assert.AreEqual(InputField.LineType.SingleLine, inputField.lineType);
            Assert.AreEqual(InputField.InputType.Standard, inputField.inputType);
            Assert.AreEqual(TouchScreenKeyboardType.EmailAddress, inputField.keyboardType);
            Assert.AreEqual(InputField.CharacterValidation.EmailAddress, inputField.characterValidation);

            inputField.contentType = InputField.ContentType.Password;
            Assert.AreEqual(InputField.LineType.SingleLine, inputField.lineType);
            Assert.AreEqual(InputField.InputType.Password, inputField.inputType);
            Assert.AreEqual(TouchScreenKeyboardType.Default, inputField.keyboardType);
            Assert.AreEqual(InputField.CharacterValidation.None, inputField.characterValidation);

            inputField.contentType = InputField.ContentType.Pin;
            Assert.AreEqual(InputField.LineType.SingleLine, inputField.lineType);
            Assert.AreEqual(InputField.InputType.Password, inputField.inputType);
            Assert.AreEqual(TouchScreenKeyboardType.NumberPad, inputField.keyboardType);
            Assert.AreEqual(InputField.CharacterValidation.Integer, inputField.characterValidation);
        }

        [Test]
        public void SettingLineTypeDoesNotChangesContentTypeToCustom([Values(InputField.ContentType.Standard, InputField.ContentType.Autocorrected)] InputField.ContentType type)
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            inputField.contentType = type;

            inputField.lineType = InputField.LineType.MultiLineNewline;

            Assert.AreEqual(type, inputField.contentType);
        }

        [Test]
        public void SettingLineTypeChangesContentTypeToCustom()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            inputField.contentType = InputField.ContentType.Name;

            inputField.lineType = InputField.LineType.MultiLineNewline;

            Assert.AreEqual(InputField.ContentType.Custom, inputField.contentType);
        }

        [Test]
        public void SettingInputChangesContentTypeToCustom()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            inputField.contentType = InputField.ContentType.Name;

            inputField.inputType = InputField.InputType.Password;

            Assert.AreEqual(InputField.ContentType.Custom, inputField.contentType);
        }

        [Test]
        public void SettingCharacterValidationChangesContentTypeToCustom()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            inputField.contentType = InputField.ContentType.Name;

            inputField.characterValidation = InputField.CharacterValidation.None;

            Assert.AreEqual(InputField.ContentType.Custom, inputField.contentType);
        }

        [Test]
        public void SettingKeyboardTypeChangesContentTypeToCustom()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            inputField.contentType = InputField.ContentType.Name;

            inputField.keyboardType = TouchScreenKeyboardType.ASCIICapable;

            Assert.AreEqual(InputField.ContentType.Custom, inputField.contentType);
        }

        [UnityTest]
        public IEnumerator CaretRectSameSizeAsTextRect()
        {
            InputField inputfield = m_PrefabRoot.GetComponentInChildren<InputField>();
            HorizontalLayoutGroup lg = inputfield.gameObject.AddComponent<HorizontalLayoutGroup>();
            lg.childControlWidth = true;
            lg.childControlHeight = false;
            lg.childForceExpandWidth = true;
            lg.childForceExpandHeight = true;
            ContentSizeFitter csf = inputfield.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            inputfield.text = "Hello World!";

            yield return new WaitForSeconds(1.0f);

            Rect prevTextRect = inputfield.textComponent.rectTransform.rect;
            Rect prevCaretRect = (inputfield.textComponent.transform.parent.GetChild(0) as RectTransform).rect;
            inputfield.text = "Hello World!Hello World!Hello World!";

            LayoutRebuilder.MarkLayoutForRebuild(inputfield.transform as RectTransform);

            yield return new WaitForSeconds(1.0f);

            Rect newTextRect = inputfield.textComponent.rectTransform.rect;
            Rect newCaretRect = (inputfield.textComponent.transform.parent.GetChild(0) as RectTransform).rect;

            Assert.IsFalse(prevTextRect == newTextRect);
            Assert.IsTrue(prevTextRect == prevCaretRect);
            Assert.IsFalse(prevCaretRect == newCaretRect);
            Assert.IsTrue(newTextRect == newCaretRect);
        }
    }
}
