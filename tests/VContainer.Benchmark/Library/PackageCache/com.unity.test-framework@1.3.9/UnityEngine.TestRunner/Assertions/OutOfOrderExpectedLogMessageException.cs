using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools.Logging;

namespace UnityEngine.TestTools.TestRunner
{
    internal class OutOfOrderExpectedLogMessageException : ResultStateException
    {
        public OutOfOrderExpectedLogMessageException(LogEvent log, LogMatch nextExpected)
            : base(BuildMessage(log, nextExpected))
        {
        }

        private static string BuildMessage(LogEvent log, LogMatch nextExpected)
        {
            return $"Expected {log.LogType} with '{log.Message}' arrived out of order. Expected '{nextExpected}' next.";
        }

        public override ResultState ResultState
        {
            get { return ResultState.Failure; }
        }

        public override string StackTrace { get { return null; } }
    }
}
