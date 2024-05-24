using System;
using System.Collections.Generic;

namespace UnityEngine.TestTools.Logging
{
    internal interface ILogScope : IDisposable
    {
        Queue<LogMatch> ExpectedLogs { get; set; }
        List<LogEvent> AllLogs { get; }
        List<LogEvent> FailingLogs { get; }
        void EvaluateLogScope(bool endOfScopeCheck);
        bool IgnoreFailingMessages { get; set; }
        bool IsNUnitException { get; }
        bool IsNUnitSuccessException { get; }
        bool IsNUnitInconclusiveException { get; }
        bool IsNUnitIgnoreException { get; }
        string NUnitExceptionMessage { get; }
        void AddLog(string message, string stacktrace, LogType type);
        bool AnyFailingLogs();
        void ProcessExpectedLogs();
        void NoUnexpectedReceived();
    }
}
