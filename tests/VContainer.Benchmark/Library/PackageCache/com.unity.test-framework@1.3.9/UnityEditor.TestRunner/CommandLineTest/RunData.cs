using System;

namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal class RunData : ScriptableSingleton<RunData>, IRunData
    {
        private bool isRunning;
        private ExecutionSettings executionSettings;
        private string runId;
        private TestRunState runState;
        private string runErrorMessage;

        public bool IsRunning
        {
            get { return isRunning; }
            set { isRunning = value; }
        }

        public ExecutionSettings ExecutionSettings
        {
            get { return executionSettings; }
            set { executionSettings = value; }
        }

        public string RunId
        {
            get { return runId; }
            set { runId = value; }
        }

        public TestRunState RunState
        {
            get { return runState; }
            set { runState = value; }
        }

        public string RunErrorMessage
        {
            get { return runErrorMessage; }
            set { runErrorMessage = value; }
        }
    }
}
