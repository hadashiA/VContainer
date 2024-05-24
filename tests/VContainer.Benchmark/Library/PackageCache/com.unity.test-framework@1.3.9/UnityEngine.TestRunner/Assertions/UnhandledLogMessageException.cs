using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools.Logging;
using UnityEngine.TestTools.Utils;

namespace UnityEngine.TestTools.TestRunner
{
    internal class UnhandledLogMessageException : ResultStateException
    {
        public LogEvent LogEvent;
        private readonly string m_CustomStackTrace;

        public UnhandledLogMessageException(LogEvent log)
            : base(BuildMessage(log))
        {
            LogEvent = log;
            m_CustomStackTrace = StackTraceFilter.Filter(log.StackTrace);
        }

        private static string BuildMessage(LogEvent log)
        {
            return string.Format("Unhandled log message: '{0}'. Use UnityEngine.TestTools.LogAssert.Expect", log);
        }

        public override ResultState ResultState
        {
            get { return ResultState.Failure; }
        }

        public override string StackTrace
        {
            get { return m_CustomStackTrace; }
        }
    }
}
