using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor.TestTools.TestRunner.UnityTestProtocol;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class EnableTestOutLoggerTask : TestTaskBase, IDisposable
    {
        internal Action<Action<PlayModeStateChange>> SubscribePlayModeStateChanged = callback => 
            EditorApplication.playModeStateChanged += callback;
        internal Action<Action<PlayModeStateChange>> UnsubscribePlayModeStateChanged = callback => 
            EditorApplication.playModeStateChanged -= callback;
        internal Action<Application.LogCallback> SubscribeLogMessageReceivedThreaded =
            callback => Application.logMessageReceived += callback;
        internal Action<Application.LogCallback> UnsubscribeLogMessageReceivedThreaded =
            callback => Application.logMessageReceived -= callback;

        internal Func<TextWriter> GetCurrentContextWriter = () => TestContext.Out;

        public EnableTestOutLoggerTask()
        {
            RerunAfterResume = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            SubscribePlayModeStateChanged(WaitForExitPlaymode);
            SubscribeLogMessageReceivedThreaded(LogReceived);
            yield break;
        }
        
        private void WaitForExitPlaymode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                UnsubscribePlayModeStateChanged(WaitForExitPlaymode);
                UnsubscribeLogMessageReceivedThreaded(LogReceived);
                SubscribeLogMessageReceivedThreaded(LogReceived);
            }
        }

        private void LogReceived(string message, string stacktrace, LogType type)
        {
            if (message.StartsWith(UtpDebugLogger.UtpPrefix))
            {
                return;
            }

            var writer = GetCurrentContextWriter();
            if (writer != null)
            {
                writer.WriteLine(message);
                if (type == LogType.Exception)
                {
                    writer.WriteLine(stacktrace);
                }
            }
        }

        public void Dispose()
        {
            UnsubscribeLogMessageReceivedThreaded(LogReceived);
        }
    }
}
