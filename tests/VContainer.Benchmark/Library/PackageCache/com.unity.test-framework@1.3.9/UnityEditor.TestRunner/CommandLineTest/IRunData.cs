namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal interface IRunData
    {
        bool IsRunning { get; set; }
        ExecutionSettings ExecutionSettings { get; set; }
        string RunId { get; set; }
        TestRunState RunState { get; set; }
        string RunErrorMessage { get; set; }
    }
}
