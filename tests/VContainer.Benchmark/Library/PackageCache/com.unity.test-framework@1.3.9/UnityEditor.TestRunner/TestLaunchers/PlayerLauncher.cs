using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.TestRunner.TestLaunchers;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.Utils;
using UnityEngine.TestTools.TestRunner;
using UnityEngine.TestTools.TestRunner.Callbacks;
using Object = UnityEngine.Object;

namespace UnityEditor.TestTools.TestRunner
{
    internal class TestLaunchFailedException : Exception
    {
        public TestLaunchFailedException() {}
        public TestLaunchFailedException(string message) : base(message) {}
    }

    [Serializable]
    internal class PlayerLauncher : RuntimeTestLauncherBase
    {
        private readonly BuildTarget m_TargetPlatform;
        private ITestRunSettings m_OverloadTestRunSettings;
        private string m_SceneName;
        private int m_HeartbeatTimeout;
        private string m_PlayerWithTestsPath;

        public PlayerLauncher(PlaymodeTestsControllerSettings settings, BuildTarget? targetPlatform, ITestRunSettings overloadTestRunSettings, int heartbeatTimeout, string playerWithTestsPath) : base(settings)
        {
            m_TargetPlatform = targetPlatform ?? EditorUserBuildSettings.activeBuildTarget;
            m_OverloadTestRunSettings = overloadTestRunSettings;
            m_HeartbeatTimeout = heartbeatTimeout;
            m_PlayerWithTestsPath = playerWithTestsPath;
        }

        protected override RuntimePlatform? TestTargetPlatform
        {
            get { return BuildTargetConverter.TryConvertToRuntimePlatform(m_TargetPlatform); }
        }

        public override void Run()
        {
            var editorConnectionTestCollector = RemoteTestRunController.instance;
            editorConnectionTestCollector.hideFlags = HideFlags.HideAndDontSave;
            editorConnectionTestCollector.Init(m_TargetPlatform, m_HeartbeatTimeout);

            var remotePlayerLogController = RemotePlayerLogController.instance;
            remotePlayerLogController.hideFlags = HideFlags.HideAndDontSave;

            using (var settings = new PlayerLauncherContextSettings(m_OverloadTestRunSettings))
            {
                m_SceneName = CreateSceneName();
                var scene = PrepareScene(m_SceneName);
                string scenePath = scene.path;

                var filter = m_Settings.BuildNUnitFilter();
                var runner = LoadTests(filter);
                var exceptionThrown = ExecutePreBuildSetupMethods(runner.LoadedTest, filter);
                if (exceptionThrown)
                {
                    ReopenOriginalScene(m_Settings.originalScene);
                    AssetDatabase.DeleteAsset(m_SceneName);
                    CallbacksDelegator.instance.RunFailed("Run Failed: One or more errors in a prebuild setup. See the editor log for details.");
                    return;
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                var playerBuildOptions = GetBuildOptions(scenePath);

                var success = BuildAndRunPlayer(playerBuildOptions);

                editorConnectionTestCollector.PostBuildAction();
                FilePathMetaInfo.TryCreateFile(runner.LoadedTest, playerBuildOptions.BuildPlayerOptions);
                ExecutePostBuildCleanupMethods(runner.LoadedTest, filter);

                ReopenOriginalScene(m_Settings.originalScene);
                AssetDatabase.DeleteAsset(m_SceneName);

                if (!success)
                {
                    editorConnectionTestCollector.CleanUp();
                    Object.DestroyImmediate(editorConnectionTestCollector);
                    Debug.LogError("Player build failed");
                    throw new TestLaunchFailedException("Player build failed");
                }

                if ((playerBuildOptions.BuildPlayerOptions.options & BuildOptions.AutoRunPlayer) != 0)
                {
                    editorConnectionTestCollector.PostSuccessfulBuildAction();
                    editorConnectionTestCollector.PostSuccessfulLaunchAction();
                }

                var runSettings = m_OverloadTestRunSettings as PlayerLauncherTestRunSettings;
                if (success && runSettings != null && runSettings.buildOnly)
                {
                    EditorUtility.RevealInFinder(playerBuildOptions.BuildPlayerOptions.locationPathName);
                }
            }
        }

        public Scene PrepareScene(string sceneName)
        {
            var scene = CreateBootstrapScene(sceneName, runner =>
            {
                runner.AddEventHandlerMonoBehaviour<PlayModeRunnerCallback>();
                runner.settings = m_Settings;
                var commandLineArgs = Environment.GetCommandLineArgs();
                if (!commandLineArgs.Contains("-doNotReportTestResultsBackToEditor"))
                {
                    runner.AddEventHandlerMonoBehaviour<RemoteTestResultSender>();
                }
                runner.AddEventHandlerMonoBehaviour<PlayerQuitHandler>();
                runner.AddEventHandlerScriptableObject<TestRunCallbackListener>();
            });
            return scene;
        }

        private static bool BuildAndRunPlayer(PlayerLauncherBuildOptions buildOptions)
        {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Building player with following options:\n{0}", buildOptions);

#if !UNITY_2021_2_OR_NEWER
            // Android has to be in listen mode to establish player connection
		    // Only flip connect to host if we are older than Unity 2021.2
            if (buildOptions.BuildPlayerOptions.target == BuildTarget.Android)
            {
                buildOptions.BuildPlayerOptions.options &= ~BuildOptions.ConnectToHost;
            }
#endif
            // For now, so does Lumin
#if !UNITY_2022_2_OR_NEWER            
            if (buildOptions.BuildPlayerOptions.target == BuildTarget.Lumin)
            {
                buildOptions.BuildPlayerOptions.options &= ~BuildOptions.ConnectToHost;
            }
#endif

#if UNITY_2023_2_OR_NEWER
            // WebGL has to be in close on quit mode to ensure that the browser tab is closed when the player finishes running tests
            if (buildOptions.BuildPlayerOptions.target == BuildTarget.WebGL)
            {
                PlayerSettings.WebGL.closeOnQuit = true;
            }
#endif

            var result = BuildPipeline.BuildPlayer(buildOptions.BuildPlayerOptions);
            if (result.summary.result != BuildResult.Succeeded)
                Debug.LogError(result.SummarizeErrors());

#if UNITY_2023_2_OR_NEWER
            // Clean up WebGL close on quit mode
            if (buildOptions.BuildPlayerOptions.target == BuildTarget.WebGL)
            {
                PlayerSettings.WebGL.closeOnQuit = false;
            }
#endif

            return result.summary.result == BuildResult.Succeeded;
        }

        internal PlayerLauncherBuildOptions GetBuildOptions(string scenePath)
        {
            var buildOnly = false;
            var runSettings = m_OverloadTestRunSettings as PlayerLauncherTestRunSettings;
            if (runSettings != null)
            {
                buildOnly = runSettings.buildOnly;
            }

            var buildOptions = new BuildPlayerOptions();

            var scenes = new List<string> { scenePath };
            scenes.AddRange(EditorBuildSettings.scenes.Select(x => x.path));
            buildOptions.scenes = scenes.ToArray();

            buildOptions.options |= BuildOptions.Development | BuildOptions.ConnectToHost | BuildOptions.IncludeTestAssemblies | BuildOptions.StrictMode;
            buildOptions.target = m_TargetPlatform;

#if UNITY_2021_2_OR_NEWER
            buildOptions.subtarget = EditorUserBuildSettings.GetActiveSubtargetFor(m_TargetPlatform);
#endif

            if (EditorUserBuildSettings.waitForPlayerConnection)
                buildOptions.options |= BuildOptions.WaitForPlayerConnection;

            if (EditorUserBuildSettings.allowDebugging)
                buildOptions.options |= BuildOptions.AllowDebugging;

            if (EditorUserBuildSettings.installInBuildFolder)
                buildOptions.options |= BuildOptions.InstallInBuildFolder;
            else if (!buildOnly)
                buildOptions.options |= BuildOptions.AutoRunPlayer;

            var buildTargetGroup = EditorUserBuildSettings.activeBuildTargetGroup;
            buildOptions.targetGroup = buildTargetGroup;

            //Check if Lz4 is supported for the current buildtargetgroup and enable it if need be
            if (PostprocessBuildPlayer.SupportsLz4Compression(buildTargetGroup, m_TargetPlatform))
            {
                if (EditorUserBuildSettings.GetCompressionType(buildTargetGroup) == Compression.Lz4)
                    buildOptions.options |= BuildOptions.CompressWithLz4;
                else if (EditorUserBuildSettings.GetCompressionType(buildTargetGroup) == Compression.Lz4HC)
                    buildOptions.options |= BuildOptions.CompressWithLz4HC;
            }

            string buildLocation;
            if (buildOnly)
            {
                buildLocation = buildOptions.locationPathName = runSettings.buildOnlyLocationPath;
            }
            else
            {
                var reduceBuildLocationPathLength = false;

                //Some platforms hit MAX_PATH limits during the build process, in these cases minimize the path length
                if ((m_TargetPlatform == BuildTarget.WSAPlayer)
#if !UNITY_2021_1_OR_NEWER
                || (m_TargetPlatform == BuildTarget.XboxOne)
#endif
                )
                {
                    reduceBuildLocationPathLength = true;
                }

                var uniqueTempPathInProject = FileUtil.GetUniqueTempPathInProject();
                var playerDirectoryName = "PlayerWithTests";

                //Some platforms hit MAX_PATH limits during the build process, in these cases minimize the path length
                if (reduceBuildLocationPathLength)
                {
                    playerDirectoryName = "PwT";
                    uniqueTempPathInProject = Path.GetTempFileName();
                    File.Delete(uniqueTempPathInProject);
                    Directory.CreateDirectory(uniqueTempPathInProject);
                }

                buildLocation = Path.Combine(string.IsNullOrEmpty(m_PlayerWithTestsPath) ? Path.GetFullPath(uniqueTempPathInProject) : m_PlayerWithTestsPath, playerDirectoryName);

                // iOS builds create a folder with Xcode project instead of an executable, therefore no executable name is added
                if (m_TargetPlatform == BuildTarget.iOS)
                {
                    buildOptions.locationPathName = buildLocation;
                }
                else
                {
                    string extensionForBuildTarget =
                        PostprocessBuildPlayer.GetExtensionForBuildTarget(buildTargetGroup, buildOptions.target,
                            buildOptions.options);
                    var playerExecutableName = "PlayerWithTests";
                    if (!string.IsNullOrEmpty(extensionForBuildTarget))
                        playerExecutableName += $".{extensionForBuildTarget}";

                    buildOptions.locationPathName = Path.Combine(buildLocation, playerExecutableName);
                }
            }

            return new PlayerLauncherBuildOptions
            {
                BuildPlayerOptions = ModifyBuildOptions(buildOptions),
                PlayerDirectory = buildLocation,
            };
        }

        private BuildPlayerOptions ModifyBuildOptions(BuildPlayerOptions buildOptions)
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.GetReferencedAssemblies().Any(z => z.Name == "UnityEditor.TestRunner")).ToArray();
            var attributes = allAssemblies.SelectMany(assembly => assembly.GetCustomAttributes(typeof(TestPlayerBuildModifierAttribute), true).OfType<TestPlayerBuildModifierAttribute>()).ToArray();
            var modifiers = attributes.Select(attribute => attribute.ConstructModifier()).ToArray();

            foreach (var modifier in modifiers)
            {
                buildOptions = modifier.ModifyOptions(buildOptions);
            }

            return buildOptions;
        }

        private static bool ShouldReduceBuildLocationPathLength(BuildTarget target)
        {
            switch (target)
            {
#if UNITY_2020_2_OR_NEWER
                case BuildTarget.GameCoreXboxOne:
                case BuildTarget.GameCoreXboxSeries:
#else
                case BuildTarget.XboxOne:
#endif
                case BuildTarget.WSAPlayer:
                    return true;
                default:
                    return false;
            }
        }
    }
}
