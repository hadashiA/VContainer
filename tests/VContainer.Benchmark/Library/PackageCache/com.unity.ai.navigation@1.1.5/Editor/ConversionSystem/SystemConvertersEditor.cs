#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.Assertions;

namespace Unity.AI.Navigation.Editor.Converter
{
    // Status for each row item to say in which state they are in.
    // This will make sure they are showing the correct icon
    [Serializable]
    enum Status
    {
        Pending,
        Warning,
        Error,
        Success
    }

    // This is the serialized class that stores the state of each item in the list of items to convert
    [Serializable]
    class ConverterItemState
    {
        public bool isActive;

        // Message that will be displayed on the icon if warning or failed.
        public string message;

        // Status of the converted item, Pending, Warning, Error or Success
        public Status status;

        internal bool hasConverted = false;
    }

    // Each converter uses the active bool
    // Each converter has a list of active items/assets
    // We do this so that we can use the binding system of the UI Elements
    [Serializable]
    class ConverterState
    {
        // This is the enabled state of the whole converter
        public bool isEnabled;
        public bool isActive;
        public bool isLoading; // to name
        public bool isInitialized;
        public List<ConverterItemState> items = new List<ConverterItemState>();

        public int pending;
        public int warnings;
        public int errors;
        public int success;
        internal int index;

        public bool isActiveAndEnabled => isEnabled && isActive;
        public bool requiresInitialization => !isInitialized && isActiveAndEnabled;
    }

    [Serializable]
    internal struct ConverterItems
    {
        public List<ConverterItemDescriptor> itemDescriptors;
    }

    [Serializable]
    internal class SystemConvertersEditor : EditorWindow
    {
        public VisualTreeAsset converterEditorAsset;
        public VisualTreeAsset converterListAsset;
        public VisualTreeAsset converterItem;

        ScrollView m_ScrollView;

        List<SystemConverter> m_CoreConvertersList = new List<SystemConverter>();

        private bool convertButtonActive = false;

        // This list needs to be as long as the amount of converters
        List<ConverterItems> m_ItemsToConvert = new List<ConverterItems>();
        //List<List<ConverterItemDescriptor>> m_ItemsToConvert = new List<List<ConverterItemDescriptor>>();
        SerializedObject m_SerializedObject;

        List<string> m_ContainerChoices = new List<string>();
        List<SystemConverterContainer> m_Containers = new List<SystemConverterContainer>();
        int m_ContainerChoiceIndex = 0;

        // This is a list of Converter States which holds a list of which converter items/assets are active
        // There is one for each Converter.
        [SerializeField] List<ConverterState> m_ConverterStates = new List<ConverterState>();

        TypeCache.TypeCollection m_ConverterContainers;

        // Name of the index file
        string m_ConverterIndex = "SystemConverterIndex";

        public void DontSaveToLayout(EditorWindow wnd)
        {
            // Making sure that the window is not saved in layouts.
            Assembly assembly = typeof(EditorWindow).Assembly;
            var editorWindowType = typeof(EditorWindow);
            var hostViewType = assembly.GetType("UnityEditor.HostView");
            var containerWindowType = assembly.GetType("UnityEditor.ContainerWindow");
            var parentViewField = editorWindowType.GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            var parentViewValue = parentViewField.GetValue(wnd);
            // window should not be saved to layout
            var containerWindowProperty =
                hostViewType.GetProperty("window", BindingFlags.Instance | BindingFlags.Public);
            var parentContainerWindowValue = containerWindowProperty.GetValue(parentViewValue);
            var dontSaveToLayoutField =
                containerWindowType.GetField("m_DontSaveToLayout", BindingFlags.Instance | BindingFlags.NonPublic);
            dontSaveToLayoutField.SetValue(parentContainerWindowValue, true);
        }

        void OnEnable()
        {
            InitIfNeeded();
        }

        void InitIfNeeded()
        {
            if (m_CoreConvertersList.Any())
                return;
            m_CoreConvertersList = new List<SystemConverter>();

            // This is the drop down choices.
            m_ConverterContainers = TypeCache.GetTypesDerivedFrom<SystemConverterContainer>();
            foreach (var continerType in m_ConverterContainers)
            {
                var container = (SystemConverterContainer)Activator.CreateInstance(continerType);
                m_Containers.Add(container);
                m_ContainerChoices.Add(container.name);
            }

            if (m_ConverterContainers.Any())
            {
                GetConverters();
            }
            else
            {
                ClearConverterStates();
            }
        }

        void ClearConverterStates()
        {
            m_CoreConvertersList.Clear();
            m_ConverterStates.Clear();
            m_ItemsToConvert.Clear();
        }

        void GetConverters()
        {
            ClearConverterStates();
            var converterList = TypeCache.GetTypesDerivedFrom<SystemConverter>();

            for (int i = 0; i < converterList.Count; ++i)
            {
                // Iterate over the converters that are used by the current container
                SystemConverter conv = (SystemConverter)Activator.CreateInstance(converterList[i]);
                if (conv.container == m_ConverterContainers[m_ContainerChoiceIndex])
                {
                    m_CoreConvertersList.Add(conv);
                }
            }

            // this need to be sorted by Priority property
            m_CoreConvertersList = m_CoreConvertersList
                .OrderBy(o => o.priority).ToList();

            for (int i = 0; i < m_CoreConvertersList.Count; i++)
            {
                // Create a new ConvertState which holds the active state of the converter
                var converterState = new ConverterState
                {
                    isEnabled = m_CoreConvertersList[i].isEnabled,
                    isActive = false,
                    isInitialized = false,
                    items = new List<ConverterItemState>(),
                    index = i,
                };
                m_ConverterStates.Add(converterState);

                // This just creates empty entries in the m_ItemsToConvert.
                // This list need to have the same amount of entries as the converters
                List<ConverterItemDescriptor> converterItemInfos = new List<ConverterItemDescriptor>();
                //m_ItemsToConvert.Add(converterItemInfos);
                m_ItemsToConvert.Add(new ConverterItems { itemDescriptors = converterItemInfos });
            }
        }

        public void CreateGUI()
        {
            InitIfNeeded();
            if (m_ConverterContainers.Any())
            {
                m_SerializedObject = new SerializedObject(this);
                converterEditorAsset.CloneTree(rootVisualElement);

                rootVisualElement.Q<DropdownField>("conversionsDropDown").choices = m_ContainerChoices;
                rootVisualElement.Q<DropdownField>("conversionsDropDown").index = m_ContainerChoiceIndex;
                RecreateUI();

                var button = rootVisualElement.Q<Button>("convertButton");
                button.RegisterCallback<ClickEvent>(Convert);
                button.SetEnabled(false);

                var initButton = rootVisualElement.Q<Button>("initializeButton");
                initButton.RegisterCallback<ClickEvent>(InitializeAllActiveConverters);
            }
        }

        void RecreateUI()
        {
            m_SerializedObject.Update();
            // This is temp now to get the information filled in
            rootVisualElement.Q<DropdownField>("conversionsDropDown").RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                m_ContainerChoiceIndex = rootVisualElement.Q<DropdownField>("conversionsDropDown").index;
                GetConverters();
                RecreateUI();
            });

            var currentContainer = m_Containers[m_ContainerChoiceIndex];
            rootVisualElement.Q<Label>("conversionName").text = currentContainer.name;
            rootVisualElement.Q<TextElement>("conversionInfo").text = currentContainer.info;

            rootVisualElement.Q<Image>("converterContainerHelpIcon").image = EditorStyles.iconHelp;

            // Getting the scrollview where the converters should be added
            m_ScrollView = rootVisualElement.Q<ScrollView>("convertersScrollView");
            m_ScrollView.Clear();
            for (int i = 0; i < m_CoreConvertersList.Count; ++i)
            {
                // Making an item using the converterListAsset as a template.
                // Then adding the information needed for each converter
                VisualElement item = new VisualElement();
                converterListAsset.CloneTree(item);
                var conv = m_CoreConvertersList[i];
                item.SetEnabled(conv.isEnabled);
                item.Q<Label>("converterName").text = conv.name;
                item.Q<Label>("converterInfo").text = conv.info;
                item.Q<VisualElement>("converterTopVisualElement").tooltip = conv.info;

                // setup the images
                item.Q<Image>("pendingImage").image = EditorStyles.iconPending;
                item.Q<Image>("pendingImage").tooltip = "Pending";
                var pendingLabel = item.Q<Label>("pendingLabel");
                item.Q<Image>("warningImage").image = EditorStyles.iconWarn;
                item.Q<Image>("warningImage").tooltip = "Warnings";
                var warningLabel = item.Q<Label>("warningLabel");
                item.Q<Image>("errorImage").image = EditorStyles.iconFail;
                item.Q<Image>("errorImage").tooltip = "Failed";
                var errorLabel = item.Q<Label>("errorLabel");
                item.Q<Image>("successImage").image = EditorStyles.iconSuccess;
                item.Q<Image>("successImage").tooltip = "Success";
                var successLabel = item.Q<Label>("successLabel");

                var converterEnabledToggle = item.Q<Toggle>("converterEnabled");
                converterEnabledToggle.bindingPath =
                    $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.isActive)}";
                pendingLabel.bindingPath =
                    $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.pending)}";
                warningLabel.bindingPath =
                    $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.warnings)}";
                errorLabel.bindingPath =
                    $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.errors)}";
                successLabel.bindingPath =
                    $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.success)}";

                VisualElement child = item;
                ListView listView = child.Q<ListView>("converterItems");

                listView.showBoundCollectionSize = false;
                listView.bindingPath = $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.items)}";

                int id = i;
                listView.makeItem = () =>
                {
                    var convertItem = converterItem.CloneTree();
                    // Adding the contextual menu for each item
                    convertItem.AddManipulator(new ContextualMenuManipulator(evt => AddToContextMenu(evt, id)));
                    return convertItem;
                };

                listView.bindItem = (element, index) =>
                {
                    m_SerializedObject.Update();
                    var property = m_SerializedObject.FindProperty($"{listView.bindingPath}.Array.data[{index}]");

                    // ListView doesn't bind the child elements for us properly, so we do that for it
                    // In the UXML our root is a BindableElement, as we can't bind otherwise.
                    var bindable = (BindableElement)element;
                    bindable.BindProperty(property);

                    // Adding index here to userData so it can be retrieved later
                    element.userData = index;

                    Status status = (Status)property.FindPropertyRelative("status").enumValueIndex;
                    string info = property.FindPropertyRelative("message").stringValue;

                    // Update the amount of things to convert
                    child.Q<Label>("converterStats").text = $"{m_ItemsToConvert[id].itemDescriptors.Count} items";

                    ConverterItemDescriptor convItemDesc = m_ItemsToConvert[id].itemDescriptors[index];

                    element.Q<Label>("converterItemName").text = convItemDesc.name;
                    element.Q<Label>("converterItemPath").text = convItemDesc.info;

                    element.Q<Image>("converterItemHelpIcon").image = EditorStyles.iconHelp;
                    element.Q<Image>("converterItemHelpIcon").tooltip = convItemDesc.helpLink;

                    // Changing the icon here depending on the status.
                    Texture2D icon = null;

                    switch (status)
                    {
                        case Status.Pending:
                            icon = EditorStyles.iconPending;
                            break;
                        case Status.Error:
                            icon = EditorStyles.iconFail;
                            break;
                        case Status.Warning:
                            icon = EditorStyles.iconWarn;
                            break;
                        case Status.Success:
                            icon = EditorStyles.iconSuccess;
                            break;
                    }

                    element.Q<Image>("converterItemStatusIcon").image = icon;
                    element.Q<Image>("converterItemStatusIcon").tooltip = info;
                };
                listView.selectionChanged += obj => { m_CoreConvertersList[id].OnClicked(listView.selectedIndex); };
                listView.unbindItem = (element, index) =>
                {
                    var bindable = (BindableElement)element;
                    bindable.Unbind();
                };

                m_ScrollView.Add(item);
            }
            rootVisualElement.Bind(m_SerializedObject);
            var button = rootVisualElement.Q<Button>("convertButton");
            button.RegisterCallback<ClickEvent>(Convert);
            button.SetEnabled(convertButtonActive);

            var initButton = rootVisualElement.Q<Button>("initializeButton");
            initButton.RegisterCallback<ClickEvent>(InitializeAllActiveConverters);
        }

        void GetAndSetData(int i, Action onAllConvertersCompleted = null)
        {
            // This need to be in Init method
            // Need to get the assets that this converter is converting.
            // Need to return Name, Path, Initial info, Help link.
            // New empty list of ConverterItemInfos
            List<ConverterItemDescriptor> converterItemInfos = new List<ConverterItemDescriptor>();
            var initCtx = new InitializeConverterContext { items = converterItemInfos };

            var conv = m_CoreConvertersList[i];

            m_ConverterStates[i].isLoading = true;

            // This should also go to the init method
            // This will fill out the converter item infos list
            int id = i;
            conv.OnInitialize(initCtx, OnConverterCompleteDataCollection);

            void OnConverterCompleteDataCollection()
            {
                // Set the item infos list to to the right index
                m_ItemsToConvert[id] = new ConverterItems { itemDescriptors = converterItemInfos };
                m_ConverterStates[id].items = new List<ConverterItemState>(converterItemInfos.Count);

                // Default all the entries to true
                for (var j = 0; j < converterItemInfos.Count; j++)
                {
                    string message = string.Empty;
                    Status status;
                    bool active = true;
                    // If this data hasn't been filled in from the init phase then we can assume that there are no issues / warnings
                    if (string.IsNullOrEmpty(converterItemInfos[j].warningMessage))
                    {
                        status = Status.Pending;
                    }
                    else
                    {
                        status = Status.Warning;
                        message = converterItemInfos[j].warningMessage;
                        active = false;
                        m_ConverterStates[id].warnings++;
                    }

                    m_ConverterStates[id].items.Add(new ConverterItemState
                    {
                        isActive = active,
                        message = message,
                        status = status,
                        hasConverted = false,
                    });
                }

                m_ConverterStates[id].isLoading = false;
                m_ConverterStates[id].isInitialized = true;

                // Making sure that the pending amount is set to the amount of items needs converting
                m_ConverterStates[id].pending = m_ConverterStates[id].items.Count;

                EditorUtility.SetDirty(this);
                m_SerializedObject.ApplyModifiedProperties();

                CheckAllConvertersCompleted();
                convertButtonActive = true;
                // Make sure that the Convert Button is turned back on
                var button = rootVisualElement.Q<Button>("convertButton");
                button.SetEnabled(convertButtonActive);
            }

            void CheckAllConvertersCompleted()
            {
                int convertersToInitialize = 0;
                int convertersInitialized = 0;

                for (var j = 0; j < m_ConverterStates.Count; j++)
                {
                    var converter = m_ConverterStates[j];

                    // Skip inactive converters
                    if (!converter.isActiveAndEnabled)
                        continue;

                    if (converter.isInitialized)
                        convertersInitialized++;
                    else
                        convertersToInitialize++;
                }

                var sum = convertersToInitialize + convertersInitialized;

                Assert.IsFalse(sum == 0);

                // Show our progress so far
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayProgressBar($"Initializing converters", $"Initializing converters ({convertersInitialized}/{sum})...", (float)convertersInitialized / sum);

                // If all converters are initialized call the complete callback
                if (convertersToInitialize == 0)
                {
                    onAllConvertersCompleted?.Invoke();
                }
            }
        }

        void InitializeAllActiveConverters(ClickEvent evt)
        {
            // If we use search index, go async
            if (ShouldCreateSearchIndex())
            {
                CreateSearchIndex(m_ConverterIndex);
            }
            // Otherwise do everything directly
            else
            {
                ConverterCollectData(() => { EditorUtility.ClearProgressBar(); });
            }

            void CreateSearchIndex(string name)
            {
                // Create <guid>.index in the project
                var title = $"Building {name} search index";
                EditorUtility.DisplayProgressBar(title, "Creating search index...", -1f);

                // Private implementation of a file naming function which puts the file at the selected path.
                Type assetdatabase = typeof(AssetDatabase);
                var indexPath = (string)assetdatabase.GetMethod("GetUniquePathNameAtSelectedPath", BindingFlags.NonPublic | BindingFlags.Static).Invoke(assetdatabase, new object[] { $"Assets/{name}.index" });

                // Write search index manifest
                System.IO.File.WriteAllText(indexPath,
@"{
                ""roots"": [""Assets""],
                ""includes"": [],
                ""excludes"": [],
                ""options"": {
                    ""types"": true,
                    ""properties"": true,
                    ""extended"": true,
                    ""dependencies"": true
                    },
                ""baseScore"": 9999
                }");

                
                
                // Import the search index
                AssetDatabase.ImportAsset(indexPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.DontDownloadFromCacheServer);

                EditorApplication.delayCall += () =>
                {
                    // Create dummy request to ensure indexing has finished
                    var context = SearchService.CreateContext("asset", $"p: a=\"{name}\"");
                    SearchService.Request(context, (_, items) =>
                    {
                        OnSearchIndexCreated(name, indexPath, () =>
                        {
                            DeleteSearchIndex(context, indexPath);
                        });
                    });
                };
            }

            void OnSearchIndexCreated(string name, string path, Action onComplete)
            {
                EditorUtility.ClearProgressBar();

                ConverterCollectData(onComplete);
            }

            void ConverterCollectData(Action onConverterDataCollectionComplete)
            {
                EditorUtility.DisplayProgressBar($"Initializing converters", $"Initializing converters...", -1f);

                int convertersToConvert = 0;
                for (int i = 0; i < m_ConverterStates.Count; ++i)
                {
                    if (m_ConverterStates[i].requiresInitialization)
                    {
                        convertersToConvert++;
                        GetAndSetData(i, onConverterDataCollectionComplete);
                    }
                }

                // If we did not kick off any converter intialization
                // We can complete everything immediately
                if (convertersToConvert == 0)
                {
                    onConverterDataCollectionComplete?.Invoke();
                }
            }

            void DeleteSearchIndex(SearchContext context, string indexPath)
            {
                context?.Dispose();
                // Client code has finished with the created index. We can delete it.
                AssetDatabase.DeleteAsset(indexPath);
                EditorUtility.ClearProgressBar();
            }
        }

        bool ShouldCreateSearchIndex()
        {
            for (int i = 0; i < m_ConverterStates.Count; ++i)
            {
                if (m_ConverterStates[i].requiresInitialization)
                {
                    var converter = m_CoreConvertersList[i];
                    if (converter.needsIndexing)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void AddToContextMenu(ContextualMenuPopulateEvent evt, int coreConverterIndex)
        {
            var ve = (VisualElement)evt.target;
            // Checking if this context menu should be enabled or not
            var isActive = m_ConverterStates[coreConverterIndex].items[(int)ve.userData].isActive &&
                !m_ConverterStates[coreConverterIndex].items[(int)ve.userData].hasConverted;

            evt.menu.AppendAction("Run converter for this asset",
                e => { ConvertIndex(coreConverterIndex, (int)ve.userData); },
                isActive ? DropdownMenuAction.AlwaysEnabled : DropdownMenuAction.AlwaysDisabled);
        }

        void UpdateInfo(int stateIndex, RunItemContext ctx)
        {
            if (ctx.didFail)
            {
                m_ConverterStates[stateIndex].items[ctx.item.index].message = ctx.info;
                m_ConverterStates[stateIndex].items[ctx.item.index].status = Status.Error;
                m_ConverterStates[stateIndex].errors++;
            }
            else
            {
                m_ConverterStates[stateIndex].items[ctx.item.index].status = Status.Success;
                m_ConverterStates[stateIndex].success++;
            }

            m_ConverterStates[stateIndex].pending--;

            // Making sure that this is set here so that if user is clicking Convert again it will not run again.
            ctx.hasConverted = true;

            VisualElement child = m_ScrollView[stateIndex];
            child.Q<ListView>("converterItems").Rebuild();
        }

        void Convert(ClickEvent evt)
        {
            List<ConverterState> activeConverterStates = new List<ConverterState>();
            // Get the names of the converters
            // Get the amount of them
            // Make the string "name x/y"

            // Getting all the active converters to use in the cancelable progressbar
            foreach (ConverterState state in m_ConverterStates)
            {
                if (state.isActive && state.isInitialized)
                {
                    activeConverterStates.Add(state);
                }
            }

            int currentCount = 0;
            int activeConvertersCount = activeConverterStates.Count;
            foreach (ConverterState activeConverterState in activeConverterStates)
            {
                currentCount++;
                var index = activeConverterState.index;
                m_CoreConvertersList[index].OnPreRun();
                var converterName = m_CoreConvertersList[index].name;
                var itemCount = m_ItemsToConvert[index].itemDescriptors.Count;
                string progressTitle = $"{converterName}           Converter : {currentCount}/{activeConvertersCount}";
                for (var j = 0; j < itemCount; j++)
                {
                    if (activeConverterState.items[j].isActive)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(progressTitle,
                            string.Format("({0} of {1}) {2}", j, itemCount, m_ItemsToConvert[index].itemDescriptors[j].info),
                            (float)j / (float)itemCount))
                            break;
                        ConvertIndex(index, j);
                    }
                }
                m_CoreConvertersList[index].OnPostRun();
                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();
            }
        }

        void ConvertIndex(int coreConverterIndex, int index)
        {
            if (!m_ConverterStates[coreConverterIndex].items[index].hasConverted)
            {
                m_ConverterStates[coreConverterIndex].items[index].hasConverted = true;
                var item = new ConverterItemInfo()
                {
                    index = index,
                    descriptor = m_ItemsToConvert[coreConverterIndex].itemDescriptors[index],
                };
                var ctx = new RunItemContext(item);
                m_CoreConvertersList[coreConverterIndex].OnRun(ref ctx);
                UpdateInfo(coreConverterIndex, ctx);
            }
        }
    }
}
#endif
