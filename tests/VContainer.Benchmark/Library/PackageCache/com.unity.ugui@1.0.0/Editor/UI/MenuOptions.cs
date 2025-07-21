using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    /// <summary>
    /// This script adds the UI menu options to the Unity Editor.
    /// </summary>

    static internal class MenuOptions
    {
        enum MenuOptionsPriorityOrder {
            // 2000 - Text (TMP)
            Image = 2001,
            RawImage = 2002,
            Panel = 2003,
            // 2020 - Button (TMP)
            Toggle = 2021,
            // 2022 - Dropdown (TMP)
            // 2023 - Input Field (TMP)
            Slider = 2024,
            Scrollbar = 2025,
            ScrollView = 2026,
            Canvas = 2060,
            EventSystem = 2061,
            Text = 2080,
            Button = 2081,
            Dropdown = 2082,
            InputField = 2083,
        };

        private const string kUILayerName = "UI";

        private const string kStandardSpritePath       = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpritePath     = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath                 = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath            = "UI/Skin/Checkmark.psd";
        private const string kDropdownArrowPath        = "UI/Skin/DropdownArrow.psd";
        private const string kMaskPath                 = "UI/Skin/UIMask.psd";

        static private DefaultControls.Resources s_StandardResources;

        static private DefaultControls.Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
                s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpritePath);
                s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);
                s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(kDropdownArrowPath);
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(kMaskPath);
            }
            return s_StandardResources;
        }

        private class DefaultEditorFactory : DefaultControls.IFactoryControls
        {
            public static DefaultEditorFactory Default = new DefaultEditorFactory();

            public GameObject CreateGameObject(string name, params Type[] components)
            {
                return ObjectFactory.CreateGameObject(name, components);
            }
        }

        private class FactorySwapToEditor : IDisposable
        {
            DefaultControls.IFactoryControls factory;

            public FactorySwapToEditor()
            {
                factory = DefaultControls.factory;
                DefaultControls.factory = DefaultEditorFactory.Default;
            }

            public void Dispose()
            {
                DefaultControls.factory = factory;
            }
        }

        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            bool explicitParentChoice = true;
            if (parent == null)
            {
                parent = GetOrCreateCanvasGameObject();
                explicitParentChoice = false;

                // If in Prefab Mode, Canvas has to be part of Prefab contents,
                // otherwise use Prefab root instead.
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
                    parent = prefabStage.prefabContentsRoot;
            }
            if (parent.GetComponentsInParent<Canvas>(true).Length == 0)
            {
                // Create canvas under context GameObject,
                // and make that be the parent which UI element is added under.
                GameObject canvas = MenuOptions.CreateNewUI();
                Undo.SetTransformParent(canvas.transform, parent.transform, "");
                parent = canvas;
            }

            GameObjectUtility.EnsureUniqueNameForSibling(element);

            SetParentAndAlign(element, parent);
            if (!explicitParentChoice) // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            // This call ensure any change made to created Objects after they where registered will be part of the Undo.
            Undo.RegisterFullObjectHierarchyUndo(parent == null ? element : parent, "");

            // We have to fix up the undo name since the name of the object was only known after reparenting it.
            Undo.SetCurrentGroupName("Create " + element.name);

            Selection.activeGameObject = element;
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            Undo.SetTransformParent(child.transform, parent.transform, "");

            RectTransform rectTransform = child.transform as RectTransform;
            if (rectTransform)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                Vector3 localPosition = rectTransform.localPosition;
                localPosition.z = 0;
                rectTransform.localPosition = localPosition;
            }
            else
            {
                child.transform.localPosition = Vector3.zero;
            }
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;

            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        // Graphic elements

        [MenuItem("GameObject/UI/Image", false, (int)MenuOptionsPriorityOrder.Image)]
        static public void AddImage(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreateImage(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI/Raw Image", false, (int)MenuOptionsPriorityOrder.RawImage)]
        static public void AddRawImage(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreateRawImage(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI/Panel", false, (int)MenuOptionsPriorityOrder.Panel)]
        static public void AddPanel(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreatePanel(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);

            // Panel is special, we need to ensure there's no padding after repositioning.
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        // Controls

        // Toggle is a control you just click on.

        [MenuItem("GameObject/UI/Toggle", false, (int)MenuOptionsPriorityOrder.Toggle)]
        static public void AddToggle(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreateToggle(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // Slider and Scrollbar modify a number

        [MenuItem("GameObject/UI/Slider", false, (int)MenuOptionsPriorityOrder.Slider)]
        static public void AddSlider(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreateSlider(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI/Scrollbar", false, (int)MenuOptionsPriorityOrder.Scrollbar)]
        static public void AddScrollbar(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreateScrollbar(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI/Scroll View", false, (int)MenuOptionsPriorityOrder.ScrollView)]
        static public void AddScrollView(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreateScrollView(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // Containers

        [MenuItem("GameObject/UI/Canvas", false, (int)MenuOptionsPriorityOrder.Canvas)]
        static public void AddCanvas(MenuCommand menuCommand)
        {
            var go = CreateNewUI();
            SetParentAndAlign(go, menuCommand.context as GameObject);
            if (go.transform.parent as RectTransform)
            {
                RectTransform rect = go.transform as RectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }
            Selection.activeGameObject = go;
        }

        // Legacy Elements

        [MenuItem("GameObject/UI/Legacy/Text", false, (int)MenuOptionsPriorityOrder.Text)]
        static public void AddText(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreateText(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI/Legacy/Button", false, (int)MenuOptionsPriorityOrder.Button)]
        static public void AddButton(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreateButton(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI/Legacy/Dropdown", false, (int)MenuOptionsPriorityOrder.Dropdown)]
        static public void AddDropdown(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreateDropdown(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI/Legacy/Input Field", false, (int)MenuOptionsPriorityOrder.InputField)]
        public static void AddInputField(MenuCommand menuCommand)
        {
            GameObject go;
            using (new FactorySwapToEditor())
                go = DefaultControls.CreateInputField(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // Helper methods

        static public GameObject CreateNewUI()
        {
            // Root for the UI
            var root = ObjectFactory.CreateGameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.layer = LayerMask.NameToLayer(kUILayerName);
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Works for all stages.
            StageUtility.PlaceGameObjectInCurrentStage(root);
            bool customScene = false;
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                Undo.SetTransformParent(root.transform, prefabStage.prefabContentsRoot.transform, "");
                customScene = true;
            }

            Undo.SetCurrentGroupName("Create " + root.name);

            // If there is no event system add one...
            // No need to place event system in custom scene as these are temporary anyway.
            // It can be argued for or against placing it in the user scenes,
            // but let's not modify scene user is not currently looking at.
            if (!customScene)
                CreateEventSystem(false);
            return root;
        }

        [MenuItem("GameObject/UI/Event System", false, (int)MenuOptionsPriorityOrder.EventSystem)]
        public static void CreateEventSystem(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            CreateEventSystem(true, parent);
        }

        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
            StageHandle stage = parent == null ? StageUtility.GetCurrentStageHandle() : StageUtility.GetStageHandle(parent);
            var esys = stage.FindComponentOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = ObjectFactory.CreateGameObject("EventSystem");
                if (parent == null)
                    StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
                else
                    SetParentAndAlign(eventSystem, parent);
                esys = ObjectFactory.AddComponent<EventSystem>(eventSystem);
                ObjectFactory.AddComponent<StandaloneInputModule>(eventSystem);

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }

        // Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        static public GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (IsValidCanvas(canvas))
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use any valid canvas.
            // We have to find all loaded Canvases, not just the ones in main scenes.
            Canvas[] canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
            for (int i = 0; i < canvasArray.Length; i++)
                if (IsValidCanvas(canvasArray[i]))
                    return canvasArray[i].gameObject;

            // No canvas in the scene at all? Then create a new one.
            return MenuOptions.CreateNewUI();
        }

        static bool IsValidCanvas(Canvas canvas)
        {
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
                return false;

            // It's important that the non-editable canvas from a prefab scene won't be rejected,
            // but canvases not visible in the Hierarchy at all do. Don't check for HideAndDontSave.
            if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0)
                return false;

            return StageUtility.GetStageHandle(canvas.gameObject) == StageUtility.GetCurrentStageHandle();
        }
    }
}
