using System;
using System.Text.RegularExpressions;
using UnityEngine.TestTools.Logging;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// A test fails if Unity logs a message other than a regular log or warning message. Use `LogAssert` to check for an expected message in the log so that the test does not fail when Unity logs the message.
    /// Use `LogAssert.Expect` before running the code under test, as the check for expected logs runs at the end of each frame.
    /// A test also reports a failure, if an expected message does not appear, or if Unity does not log any regular log or warning messages.
    /// 
    /// `LogAssert` lets you expect Unity log messages that would otherwise cause the test to fail.
    /// </summary>
    public static class LogAssert
    {
        /// <summary>
        /// Verifies that a log message of a specified type appears in the log. A test won't fail from an expected error, assertion, or exception log message. It does fail if an expected message does not appear in the log.
        /// If multiple LogAssert.Expect are used to expect multiple messages, they are expected to be logged in that order.
        /// </summary>
        /// <param name="type">A type of log to expect. It can take one of the [LogType enum](https://docs.unity3d.com/ScriptReference/LogType.html) values.</param>
        /// <param name="message">A string value that should equate to the expected message.</param>
        /// <example>
        /// <code>
        /// [Test]
        /// public void LogAssertExample()
        /// {
        ///     // Expect a regular log message
        ///     LogAssert.Expect(LogType.Log, "Log message");
        ///
        ///     // The test fails without the following expected log message
        ///     Debug.Log("Log message");
        ///
        ///     // An error log
        ///     Debug.LogError("Error message");
        ///
        ///     // Without expecting an error log, the test would fail
        ///     LogAssert.Expect(LogType.Error, "Error message");
        /// }
        ///
        /// </code>
        /// </example>
        public static void Expect(LogType type, string message)
        {
            LogScope.Current.ExpectedLogs.Enqueue(new LogMatch { LogType = type, Message = message });
        }

        /// <summary>
        /// Verifies that a log message of a specified type appears in the log. A test won't fail from an expected error, assertion, or exception log message. It does fail if an expected message does not appear in the log.
        /// </summary>
        /// <param name="type">A type of log to expect. It can take one of the [LogType enum](https://docs.unity3d.com/ScriptReference/LogType.html) values.</param>
        /// <param name="message">A regular expression pattern to match the expected message.</param>
        public static void Expect(LogType type, Regex message)
        {
            LogScope.Current.ExpectedLogs.Enqueue(new LogMatch { LogType = type, MessageRegex = message });
        }

        /// <summary>
        /// Triggers an assertion when receiving any log messages and fails the test if some are unexpected messages. If multiple tests need to check for no received unexpected logs, consider using the <see cref="TestMustExpectAllLogsAttribute"/> attribute instead.
        /// </summary>
        public static void NoUnexpectedReceived()
        {
            LogScope.Current.NoUnexpectedReceived();
        }

        /// <summary>Set this property to `true` to prevent unexpected error log messages from triggering an assertion. By default, it is `false`.</summary>
        /// <returns>The value of the ignoreFailingMessages boolean property.</returns>
        public static bool ignoreFailingMessages
        {
            get
            {
                return LogScope.Current.IgnoreFailingMessages;
            }
            set
            {
                if (value != LogScope.Current.IgnoreFailingMessages)
                {
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "\nIgnoreFailingMessages:" + (value ? "true" : "false"));
                }
                LogScope.Current.IgnoreFailingMessages = value;
            }
        }
    }
}
