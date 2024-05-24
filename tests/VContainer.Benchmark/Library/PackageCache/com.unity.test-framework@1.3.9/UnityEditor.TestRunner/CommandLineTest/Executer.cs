using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.TestRunner.TestLaunchers;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal class Executer : IExecuter
    {
        internal IRunData runData = RunData.instance;

        private ITestRunnerApi m_TestRunnerApi;
        private ISettingsBuilder m_SettingsBuilder;
        private Action<string, object[]> m_LogErrorFormat;
        private Action<Exception> m_LogException;
        private Action<string> m_LogMessage;
        private Action<int> m_ExitEditorApplication;
        private Func<bool> m_ScriptCompilationFailedCheck;
        private Func<bool> m_IsRunActive;

        public Executer(ITestRunnerApi testRunnerApi, ISettingsBuilder settingsBuilder, Action<string, object[]> logErrorFormat, Action<Exception> logException, Action<string> logMessage, Action<int> exitEditorApplication, Func<bool> scriptCompilationFailedCheck, Func<bool> isRunActive)
        {
            m_TestRunnerApi = testRunnerApi;
            m_SettingsBuilder = settingsBuilder;
            m_LogErrorFormat = logErrorFormat;
            m_LogException = logException;
            m_LogMessage = logMessage;
            m_ExitEditorApplication = exitEditorApplication;
            m_ScriptCompilationFailedCheck = scriptCompilationFailedCheck;
            m_IsRunActive = isRunActive;
        }

        public string InitializeAndExecuteRun(string[] commandLineArgs)
        {
            Api.ExecutionSettings executionSettings;
            try
            {
                executionSettings = m_SettingsBuilder.BuildApiExecutionSettings(commandLineArgs);
                if (executionSettings.targetPlatform.HasValue)
                    RemotePlayerLogController.instance.SetBuildTarget(executionSettings.targetPlatform.Value);
            }
            catch (SetupException exception)
            {
                HandleSetupException(exception);
                return string.Empty;
            }

            try
            {
                // It is important that the message starts with "Running tests for ", otherwise TestCleanConsole will fail.
                m_LogMessage($"Running tests for {executionSettings}");
                return m_TestRunnerApi.Execute(executionSettings);
            }
            catch (Exception exception)
            {
                m_LogException(exception);
                ExitApplication(ReturnCodes.RunError, "Exception when starting test run.");
                return string.Empty;
            }
        }

        public void ExitIfRunIsCompleted()
        {
            if (m_IsRunActive())
            {
                return;
            }

            var runState = runData.RunState;
            var returnCode = s_StateReturnCodes[runState];
            var reason = s_StateMessages[runState] ?? runData.RunErrorMessage;
            ExitApplication(returnCode, reason);
        }

        private void ExitApplication(ReturnCodes returnCode, string reason)
        {
            var returnCodeInt = (int)returnCode;

            m_LogMessage($"Test run completed. Exiting with code {returnCodeInt} ({returnCode}). {reason}");

            m_ExitEditorApplication(returnCodeInt);
        }

        public ExecutionSettings BuildExecutionSettings(string[] commandLineArgs)
        {
            return m_SettingsBuilder.BuildExecutionSettings(commandLineArgs);
        }

        internal enum ReturnCodes
        {
            Ok = 0,
            Failed = 2,
            RunError = 3,
            PlatformNotFoundReturnCode = 4
        }

        public void SetUpCallbacks(ExecutionSettings executionSettings)
        {
            RemotePlayerLogController.instance.SetLogsDirectory(executionSettings.DeviceLogsDirectory);

            var resultSavingCallback = ScriptableObject.CreateInstance<ResultsSavingCallbacks>();
            resultSavingCallback.m_ResultFilePath = executionSettings.TestResultsFile;

            var logSavingCallback = ScriptableObject.CreateInstance<LogSavingCallbacks>();

            TestRunnerApi.RegisterTestCallback(resultSavingCallback);
            TestRunnerApi.RegisterTestCallback(logSavingCallback);
            TestRunnerApi.RegisterTestCallback(new RunStateCallbacks());
        }

        public void ExitOnCompileErrors()
        {
            if (m_ScriptCompilationFailedCheck())
            {
                var handling = s_ExceptionHandlingMapping.First(h => h.m_ExceptionType == SetupException.ExceptionType.ScriptCompilationFailed);
                m_LogErrorFormat(handling.m_Message, new object[0]);
                ExitApplication(handling.m_ReturnCode, handling.m_Message);
            }
        }

        private void HandleSetupException(SetupException exception)
        {
            ExceptionHandling handling = s_ExceptionHandlingMapping.FirstOrDefault(h => h.m_ExceptionType == exception.Type) ?? new ExceptionHandling(exception.Type, "Unknown command line test run error. " + exception.Type, ReturnCodes.RunError);
            m_LogErrorFormat(handling.m_Message, exception.Details);
            ExitApplication(handling.m_ReturnCode, handling.m_Message);
        }

        private class ExceptionHandling
        {
            internal SetupException.ExceptionType m_ExceptionType;
            internal string m_Message;
            internal ReturnCodes m_ReturnCode;
            public ExceptionHandling(SetupException.ExceptionType exceptionType, string message, ReturnCodes returnCode)
            {
                m_ExceptionType = exceptionType;
                m_Message = message;
                m_ReturnCode = returnCode;
            }
        }

        private static ExceptionHandling[] s_ExceptionHandlingMapping = {
            new ExceptionHandling(SetupException.ExceptionType.ScriptCompilationFailed, "Scripts had compilation errors.", ReturnCodes.RunError),
            new ExceptionHandling(SetupException.ExceptionType.PlatformNotFound, "Test platform not found ({0}).", ReturnCodes.PlatformNotFoundReturnCode),
            new ExceptionHandling(SetupException.ExceptionType.TestSettingsFileNotFound, "Test settings file not found at {0}.", ReturnCodes.RunError),
            new ExceptionHandling(SetupException.ExceptionType.OrderedTestListFileNotFound, "Ordered test list file not found at {0}.", ReturnCodes.RunError)
        };

        private static IDictionary<TestRunState, string> s_StateMessages = new Dictionary<TestRunState, string>()
        {
            {TestRunState.NoCallbacksReceived, "No callbacks received."},
            {TestRunState.OneOrMoreTestsExecutedWithNoFailures, "Run completed."},
            {TestRunState.OneOrMoreTestsExecutedWithOneOrMoreFailed, "One or more tests failed."},
            {TestRunState.CompletedJobWithoutAnyTestsExecuted, "No tests were executed."},
            {TestRunState.RunError, null}
        };

        private static IDictionary<TestRunState, ReturnCodes> s_StateReturnCodes = new Dictionary<TestRunState, ReturnCodes>()
        {
            {TestRunState.NoCallbacksReceived, ReturnCodes.RunError},
            {TestRunState.OneOrMoreTestsExecutedWithNoFailures, ReturnCodes.Ok},
            {TestRunState.OneOrMoreTestsExecutedWithOneOrMoreFailed, ReturnCodes.Failed},
            {TestRunState.CompletedJobWithoutAnyTestsExecuted, ReturnCodes.Ok},
            {TestRunState.RunError, ReturnCodes.RunError}
        };
    }
}
