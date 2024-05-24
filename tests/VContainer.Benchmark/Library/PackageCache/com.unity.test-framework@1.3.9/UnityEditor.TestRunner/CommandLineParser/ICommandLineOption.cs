using System;

namespace UnityEditor.TestRunner.CommandLineParser
{
    internal interface ICommandLineOption
    {
        string ArgName { get; }
        void ApplyValue(string value);
    }
}
