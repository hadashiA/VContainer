using System;
using UnityEditor.TestRunner.CommandLineParser;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal class TestStarter
    {
        [InitializeOnLoadMethod]
        internal static void Initialize()
        {
            new TestStarter().Init();
        }

        internal Action<EditorApplication.CallbackFunction> registerEditorUpdateCallback = (action) =>
        {
            EditorApplication.update += action;
        };
        internal Action<EditorApplication.CallbackFunction> unregisterEditorUpdateCallback = (action) =>
        {
            EditorApplication.update -= action;
        };
        internal Func<bool> isCompiling = () => EditorApplication.isCompiling;
        internal IRunData runData = RunData.instance;
        internal Func<string[]> GetCommandLineArgs = Environment.GetCommandLineArgs;

        internal void Init()
        {
            if (!ShouldRunTests())
            {
                return;
            }

            if (isCompiling())
            {
                return;
            }

            executer.ExitOnCompileErrors();

            if (runData.IsRunning)
            {
                executer.SetUpCallbacks(runData.ExecutionSettings);
                registerEditorUpdateCallback(executer.ExitIfRunIsCompleted);
                return;
            }

            // Execute the test run on the next editor update to allow other framework components
            // (the TestJobDataHolder.ResumeRunningJobs method in particular) to register themselves
            // or modify the test run environment using InitializeOnLoad and InitializeOnLoadMethod calls
            registerEditorUpdateCallback(InitializeAndExecuteRun);
        }

        internal void InitializeAndExecuteRun()
        {
            unregisterEditorUpdateCallback(InitializeAndExecuteRun);

            runData.IsRunning = true;
            var commandLineArgs = GetCommandLineArgs();
            runData.ExecutionSettings = executer.BuildExecutionSettings(commandLineArgs);
            executer.SetUpCallbacks(runData.ExecutionSettings);
            runData.RunState = default;
            runData.RunId = executer.InitializeAndExecuteRun(commandLineArgs);
            registerEditorUpdateCallback(executer.ExitIfRunIsCompleted);
        }

        private bool ShouldRunTests()
        {
            var shouldRunTests = false;
            var optionSet = new CommandLineOptionSet(
                new CommandLineOption("runTests", () => { shouldRunTests = true; }),
                new CommandLineOption("runEditorTests", () => { shouldRunTests = true; })
            );
            optionSet.Parse(GetCommandLineArgs());
            return shouldRunTests;
        }

        internal IExecuter m_Executer;
        private IExecuter executer
        {
            get
            {
                if (m_Executer == null)
                {
                    Func<bool> compilationCheck = () => EditorUtility.scriptCompilationFailed;
                    Action<string> actionLogger = msg => { Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, msg); };
                    var apiSettingsBuilder = new SettingsBuilder(new TestSettingsDeserializer(() => new TestSettings()), actionLogger, Debug.LogWarning, compilationCheck);
                    m_Executer = new Executer(ScriptableObject.CreateInstance<TestRunnerApi>(), apiSettingsBuilder, Debug.LogErrorFormat, Debug.LogException, Debug.Log, EditorApplication.Exit, compilationCheck, TestRunnerApi.IsRunActive);
                }

                return m_Executer;
            }
        }
    }
}
