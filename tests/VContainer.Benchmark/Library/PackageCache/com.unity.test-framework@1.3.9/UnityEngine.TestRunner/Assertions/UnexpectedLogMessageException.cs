using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools.Logging;

namespace UnityEngine.TestTools.TestRunner
{
    internal class UnexpectedLogMessageException : ResultStateException
    {
        public LogMatch LogEvent;

        public UnexpectedLogMessageException(LogMatch log)
            : base(BuildMessage(log))
        {
            LogEvent = log;
        }

        private static string BuildMessage(LogMatch log)
        {
            return string.Format("Expected log did not appear: {0}", log);
        }

        public override ResultState ResultState
        {
            get { return ResultState.Failure; }
        }

        public override string StackTrace { get { return null; } }
    }
}
