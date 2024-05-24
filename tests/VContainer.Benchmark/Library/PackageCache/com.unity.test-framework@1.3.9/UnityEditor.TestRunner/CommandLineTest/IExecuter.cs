namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal interface IExecuter
    {
        string InitializeAndExecuteRun(string[] commandLineArgs);
        void ExitIfRunIsCompleted();
        ExecutionSettings BuildExecutionSettings(string[] commandLineArgs);
        void SetUpCallbacks(ExecutionSettings executionSettings);
        void ExitOnCompileErrors();
    }
}
