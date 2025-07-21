using System;
using System.Collections.Generic;
using UnityEditor.DeploymentTargets;
using UnityEditor.TestTools.TestRunner.CommandLineTest;
using UnityEngine;

namespace UnityEditor.TestRunner.TestLaunchers
{
    [Serializable]
    internal class RemotePlayerLogController : ScriptableSingleton<RemotePlayerLogController>
    {
        private List<LogWriter> m_LogWriters;

        private Dictionary<string, DeploymentTargetLogger> m_Loggers;

        private string m_DeviceLogsDirectory;

        public void SetBuildTarget(BuildTarget buildTarget)
        {
            m_Loggers = GetDeploymentTargetLoggers(buildTarget);

            if (m_Loggers == null)
                Debug.Log("Deployment target logger could not be created");
        }

        public void SetLogsDirectory(string dir)
        {
            m_DeviceLogsDirectory = dir;
        }

        public void StartLogWriters()
        {
            if (m_DeviceLogsDirectory == null || m_Loggers == null)
                return;

            m_LogWriters = new List<LogWriter>();

            foreach (var logger in m_Loggers)
            {
                m_LogWriters.Add(new LogWriter(m_DeviceLogsDirectory, logger.Key, logger.Value));
                logger.Value.Start();
            }
        }

        public void StopLogWriters()
        {
            if (m_LogWriters == null)
                return;

            foreach (var logWriter in m_LogWriters)
            {
                logWriter.Stop();
            }
        }

        private Dictionary<string, DeploymentTargetLogger> GetDeploymentTargetLoggers(BuildTarget buildTarget)
        {
            DeploymentTargetManager deploymentTargetManager;

            try
            {
                deploymentTargetManager = DeploymentTargetManager.CreateInstance(EditorUserBuildSettings.activeBuildTargetGroup, buildTarget);

                if (deploymentTargetManager == null)
                    return null;
            }
            catch (NotSupportedException ex)
            {
                Debug.Log(ex.Message);
                Debug.Log("Deployment target logger not initialised");
                return null;
            }

            var targets = deploymentTargetManager.GetKnownTargets();
            var loggers = new Dictionary<string, DeploymentTargetLogger>();

            foreach (var target in targets)
            {
                if (target.status != DeploymentTargetStatus.Ready) continue;

                var logger = deploymentTargetManager.GetTargetLogger(target.id);
                logger.Clear();
                loggers.Add(target.id, logger);
            }

            return loggers;
        }
    }
}
