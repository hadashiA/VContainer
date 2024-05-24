using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.DeploymentTargets;
using UnityEditor.Utils;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.CommandLineTest
{
    internal class LogWriter : IDisposable
    {
        private string m_LogsDirectory;
        private string m_DeviceID;
        private Dictionary<string, StreamWriter> m_LogStreams;
        private DeploymentTargetLogger m_Logger;

        internal LogWriter(string logsDirectory, string deviceID, DeploymentTargetLogger logger)
        {
            m_LogStreams = new Dictionary<string, StreamWriter>();
            m_Logger = logger;
            m_LogsDirectory = logsDirectory;
            m_DeviceID = deviceID;

            logger.logMessage += WriteLogToFile;
        }

        private void WriteLogToFile(string id, string logLine)
        {
            StreamWriter logStream;
            var streamExists = m_LogStreams.TryGetValue(id, out logStream);
            if (!streamExists)
            {
                var filePath = GetLogFilePath(m_LogsDirectory, m_DeviceID, id);
                logStream = CreateLogFile(filePath);

                m_LogStreams.Add(id, logStream);
            }

            try
            {
                if (logLine != null)
                    logStream.WriteLine(logLine);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Writing {id} log failed.");
                Debug.LogException(ex);
            }
        }

        public void Stop()
        {
            m_Logger.Stop();
            foreach (var logStream in m_LogStreams)
            {
                logStream.Value.Close();
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private StreamWriter CreateLogFile(string path)
        {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Creating {0} device log: {1}", m_DeviceID, path);
            StreamWriter streamWriter = null;
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                streamWriter = File.CreateText(path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Creating device log {path} file failed.");
                Debug.LogException(ex);
            }

            return streamWriter;
        }

        private string GetLogFilePath(string lgosDirectory, string deviceID, string logID)
        {
            var fileName = "Device-" + deviceID + "-" + logID + ".txt";
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            return Paths.Combine(lgosDirectory, fileName);
        }
    }
}
