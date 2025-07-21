using System;
using System.IO;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    [Serializable]
    internal class PlayModeTestListGUI : TestListGUI
    {
        private struct PlayerMenuItem
        {
            public GUIContent name;
            public bool filterSelectedTestsOnly;
            public bool buildOnly;
        }

        [SerializeField]
        private int m_SelectedOption;

        public override TestMode TestMode
        {
            get { return TestMode.PlayMode; }
        }

        private string GetBuildText()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
                        return "Export";
                    break;
                case BuildTarget.iOS:
                    return "Export";
            }
            return "Build";
        }

        private string PickBuildLocation()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var lastLocation = EditorUserBuildSettings.GetBuildLocation(target);
            var extension = PostprocessBuildPlayer.GetExtensionForBuildTarget(targetGroup, target, BuildOptions.None);
            var defaultName = FileUtil.GetLastPathNameComponent(lastLocation);
            lastLocation = string.IsNullOrEmpty(lastLocation) ? string.Empty : Path.GetDirectoryName(lastLocation);
            bool updateExistingBuild;
            var location = EditorUtility.SaveBuildPanel(target, $"{GetBuildText()} {target}", lastLocation, defaultName, extension,
                out updateExistingBuild);
            if (!string.IsNullOrEmpty(location))
                EditorUserBuildSettings.SetBuildLocation(target, location);
            return location;
        }

        private void ExecuteAction(PlayerMenuItem item)
        {
            var runSettings = new PlayerLauncherTestRunSettings();
            runSettings.buildOnly = item.buildOnly;
            if (runSettings.buildOnly)
            {
                runSettings.buildOnlyLocationPath = PickBuildLocation();
                if (string.IsNullOrEmpty(runSettings.buildOnlyLocationPath))
                {
                    Debug.LogWarning("Aborting, build selection was canceled.");
                    return;
                }
            }

            if (item.filterSelectedTestsOnly)
                RunTestsInPlayer(runSettings, SelectedTestsFilter);
            else
            {
                var filter = new UITestRunnerFilter { categoryNames = m_TestRunnerUIFilter.CategoryFilter };
                RunTestsInPlayer(runSettings, filter);
            }
        }

        public override void PrintHeadPanel()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
            base.PrintHeadPanel();

            PlayerMenuItem[] menuItems;

            if (EditorUserBuildSettings.installInBuildFolder)
            {
                menuItems = new []
                {
                    // Note: We select here buildOnly = false, so build location dialog won't show up
                    //       The player won't actually be ran when using together with EditorUserBuildSettings.installInBuildFolder
                    new PlayerMenuItem()
                    {
                        name = new GUIContent("Install All Tests In Build Folder"), buildOnly = false, filterSelectedTestsOnly = false
                    },
                    new PlayerMenuItem()
                    {
                        name = new GUIContent("Install Selected Tests In Build Folder"), buildOnly = false, filterSelectedTestsOnly = true
                    }
                };
            }
            else
            {
                menuItems = new []
                {
                    new PlayerMenuItem()
                    {
                        name = new GUIContent("Run All Tests"), buildOnly = false, filterSelectedTestsOnly = false
                    },
                    new PlayerMenuItem()
                    {
                        name = new GUIContent("Run Selected Tests"), buildOnly = false, filterSelectedTestsOnly = true
                    },
                    new PlayerMenuItem()
                    {
                        name = new GUIContent($"{GetBuildText()} All Tests"), buildOnly = true, filterSelectedTestsOnly = false
                    },
                    new PlayerMenuItem()
                    {
                        name = new GUIContent($"{GetBuildText()} Selected Tests"), buildOnly = true, filterSelectedTestsOnly = true
                    },
                };
            }

            m_SelectedOption = Math.Min(m_SelectedOption, menuItems.Length - 1);
            var selectedMenuItem = menuItems[m_SelectedOption];
            if (GUILayout.Button(
                new GUIContent($"{selectedMenuItem.name.text} ({EditorUserBuildSettings.activeBuildTarget})"),
                EditorStyles.toolbarButton))
            {
                ExecuteAction(selectedMenuItem);
            }

            if (GUILayout.Button(GUIContent.none, EditorStyles.toolbarDropDown))
            {
                Vector2 mousePos = Event.current.mousePosition;
                EditorUtility.DisplayCustomMenu(new Rect(mousePos.x, mousePos.y, 0, 0),
                    menuItems.Select(m => m.name).ToArray(),
                    -1,
                    (object userData, string[] options, int selected) => m_SelectedOption = selected,
                    menuItems);
            }

            EditorGUILayout.EndHorizontal();
            DrawFilters();
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
            EditorGUILayout.EndHorizontal();
        }

        public override void RenderNoTestsInfo()
        {
            if (!TestListGUIHelper.SelectedFolderContainsTestAssembly())
            {
                var noTestText = "No tests to show";
                if (!PlayerSettings.playModeTestRunnerEnabled)
                {
                    const string testsArePulledFromCustomAssemblues = "Test Assemblies are defined by Assembly Definitions that references the \"nunit.framework.dll\" Assembly Reference or the Assembly Definition Reference \"UnityEngine.TestRunner\".";
                    const string infoTextAboutTestsInAllAssemblies =
                        "To have tests in all assemblies enable it in the Test Runner window context menu";
                    noTestText += Environment.NewLine + testsArePulledFromCustomAssemblues + Environment.NewLine +
                        infoTextAboutTestsInAllAssemblies;
                }

                EditorGUILayout.HelpBox(noTestText, MessageType.Info);
                if (GUILayout.Button("Create PlayMode Test Assembly Folder"))
                {
                    TestListGUIHelper.AddFolderAndAsmDefForTesting();
                }
            }

            if (!TestListGUIHelper.CanAddPlayModeTestScriptAndItWillCompile())
            {
                UnityEngine.GUI.enabled = false;
                EditorGUILayout.HelpBox("PlayMode test scripts can only be created in non editor test assemblies.", MessageType.Warning);
            }
            if (GUILayout.Button("Create Test Script in current folder"))
            {
                TestListGUIHelper.AddTest();
            }
            UnityEngine.GUI.enabled = true;
        }

        protected override void RunTests(UITestRunnerFilter[] filters)
        {
            foreach (var filter in filters)
            {
                filter.ClearResults(newResultList.OfType<UITestRunnerFilter.IClearableResult>().ToList());
            }

            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            testRunnerApi.Execute(new ExecutionSettings()
            {
                filters = filters.Select(filter => new Filter()
                {
                    assemblyNames = filter.assemblyNames,
                    categoryNames = filter.categoryNames,
                    groupNames =  filter.groupNames,
                    testMode = TestMode,
                    testNames = filter.testNames
                }).ToArray()
            });
        }


        protected void RunTestsInPlayer(PlayerLauncherTestRunSettings runSettings, params UITestRunnerFilter[] filters)
        {
            foreach (var filter in filters)
            {
                filter.ClearResults(newResultList.OfType<UITestRunnerFilter.IClearableResult>().ToList());
            }

            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            testRunnerApi.Execute(new ExecutionSettings()
            {
                overloadTestRunSettings = runSettings,
                filters = filters.Select(filter => new Filter()
                {
                    assemblyNames = filter.assemblyNames,
                    categoryNames = filter.categoryNames,
                    groupNames = filter.groupNames,
                    testMode = TestMode,
                    testNames = filter.testNames
                }).ToArray(),
                targetPlatform = EditorUserBuildSettings.activeBuildTarget
            });
        }

        public override TestPlatform TestPlatform { get { return TestPlatform.PlayMode; } }

        protected override bool IsBusy()
        {
            return TestRunnerApi.IsRunActive() || EditorApplication.isCompiling || EditorApplication.isPlaying;
        }
    }
}
