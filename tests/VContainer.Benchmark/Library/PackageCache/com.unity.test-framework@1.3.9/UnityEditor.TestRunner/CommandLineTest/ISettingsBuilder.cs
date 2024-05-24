using System;

namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal interface ISettingsBuilder
    {
        Api.ExecutionSettings BuildApiExecutionSettings(string[] commandLineArgs);
        ExecutionSettings BuildExecutionSettings(string[] commandLineArgs);
    }
}
