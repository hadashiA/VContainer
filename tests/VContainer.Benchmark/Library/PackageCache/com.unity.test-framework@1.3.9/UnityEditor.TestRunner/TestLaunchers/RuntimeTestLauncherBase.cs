using System;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;
using UnityEngine.TestTools.Utils;

namespace UnityEditor.TestTools.TestRunner
{
    internal abstract class RuntimeTestLauncherBase : TestLauncherBase
    {
        internal readonly PlaymodeTestsControllerSettings m_Settings;

        protected RuntimeTestLauncherBase(PlaymodeTestsControllerSettings mSettings)
        {
            m_Settings = mSettings;
        }
        protected Scene CreateBootstrapScene(string sceneName, Action<PlaymodeTestsController> runnerSetup)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject(PlaymodeTestsController.kPlaymodeTestControllerName);

            var editorLoadedTestAssemblyProvider = new EditorLoadedTestAssemblyProvider(new EditorCompilationInterfaceProxy(), new EditorAssembliesProxy());

            var runner = go.AddComponent<PlaymodeTestsController>();
            runnerSetup(runner);
            runner.settings.bootstrapScene = sceneName;
            runner.settings.orderedTestNames = m_Settings.orderedTestNames;
            runner.settings.randomOrderSeed = m_Settings.randomOrderSeed;
            runner.AssembliesWithTests = editorLoadedTestAssemblyProvider.GetAssembliesGroupedByType(TestPlatform.PlayMode).Select(x => x.Assembly.GetName().Name).ToList();

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, sceneName, false);

            return scene;
        }

        public string CreateSceneName()
        {
            return "Assets/InitTestScene" + DateTime.Now.Ticks + ".unity";
        }

        protected UnityTestAssemblyRunner LoadTests(ITestFilter filter)
        {
            var editorLoadedTestAssemblyProvider = new EditorLoadedTestAssemblyProvider(new EditorCompilationInterfaceProxy(), new EditorAssembliesProxy());
            var assembliesWithTests = editorLoadedTestAssemblyProvider.GetAssembliesGroupedByType(TestPlatform.PlayMode).Select(x => x.Assembly.GetName().Name).ToList();

            var nUnitTestAssemblyRunner = new UnityTestAssemblyRunner(new UnityTestAssemblyBuilder(m_Settings.orderedTestNames, m_Settings.randomOrderSeed), null, UnityTestExecutionContext.CurrentContext);
            var assemblyProvider = new PlayerTestAssemblyProvider(new AssemblyLoadProxy(), assembliesWithTests);
            nUnitTestAssemblyRunner.Load(assemblyProvider.GetUserAssemblies().Select(a => a.Assembly).ToArray(), TestPlatform.PlayMode, UnityTestAssemblyBuilder.GetNUnitTestBuilderSettings(TestPlatform.PlayMode));
            return nUnitTestAssemblyRunner;
        }

        protected static void ReopenOriginalScene(string originalSceneName)
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            if (!string.IsNullOrEmpty(originalSceneName))
            {
                EditorSceneManager.OpenScene(originalSceneName);
            }
        }
    }

    internal static class PlaymodeTestsControllerExtensions
    {
        internal static T AddEventHandlerMonoBehaviour<T>(this PlaymodeTestsController controller) where T : MonoBehaviour, ITestRunnerListener
        {
            var eventHandler = controller.gameObject.AddComponent<T>();
            SetListeners(controller, eventHandler);
            return eventHandler;
        }

        internal static T AddEventHandlerScriptableObject<T>(this PlaymodeTestsController controller) where T : ScriptableObject, ITestRunnerListener
        {
            var eventListener = ScriptableObject.CreateInstance<T>();
            AddEventHandlerScriptableObject(controller, eventListener);
            return eventListener;
        }

        internal static void AddEventHandlerScriptableObject(this PlaymodeTestsController controller, ITestRunnerListener obj)
        {
            SetListeners(controller, obj);
        }

        private static void SetListeners(PlaymodeTestsController controller, ITestRunnerListener eventHandler)
        {
            UnityEventTools.AddPersistentListener(controller.testStartedEvent, eventHandler.TestStarted);
            UnityEventTools.AddPersistentListener(controller.testFinishedEvent, eventHandler.TestFinished);
            UnityEventTools.AddPersistentListener(controller.runStartedEvent, eventHandler.RunStarted);
            UnityEventTools.AddPersistentListener(controller.runFinishedEvent, eventHandler.RunFinished);
        }
    }
}
