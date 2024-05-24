using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner
{
    [Serializable]
    internal class ApplePlatformSetup : IPlatformSetup
    {
        [SerializeField]
        private bool m_Stripping;

        private bool m_RunOnSimulator;
        private List<int> m_XcodesOpenBeforeTests;
        private List<int> m_XcodesToCloseAfterTests;
        private List<int> m_SimulatorsOpenBeforeTests;

        public ApplePlatformSetup(BuildTarget buildTarget)
        {
        }

        public void Setup()
        {
            // Camera and fonts are stripped out and app crashes on iOS when test runner is trying to add a scene with... camera and text
            m_Stripping = PlayerSettings.stripEngineCode;
            PlayerSettings.stripEngineCode = false;

            m_RunOnSimulator = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS
                ? PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK
                : PlayerSettings.tvOS.sdkVersion == tvOSSdkVersion.Simulator;

            // Gather IDs of Xcodes that were already open before building and running the tests
            if (Application.isEditor && Application.isBatchMode)
            {
                m_XcodesOpenBeforeTests = new List<int>();
                foreach (var xcode in Process.GetProcessesByName("Xcode"))
                {
                    m_XcodesOpenBeforeTests.Add(xcode.Id);
                }
            }
        }

        public void PostBuildAction()
        {
            // Restoring player setting as early as possible
            PlayerSettings.stripEngineCode = m_Stripping;

            // Gather IDs of Xcodes that were opened while building the tests
            if (Application.isEditor && Application.isBatchMode)
            {
                m_XcodesToCloseAfterTests = new List<int>();
                foreach (var xcode in Process.GetProcessesByName("Xcode"))
                {
                    if (!m_XcodesOpenBeforeTests?.Contains(xcode.Id) ?? false)
                        m_XcodesToCloseAfterTests.Add(xcode.Id);
                }
            }
        }

        public void PostSuccessfulBuildAction()
        {
            // Gather IDs of Simulator processes that were already open before successfully building the tests
            if (Application.isEditor && Application.isBatchMode && m_RunOnSimulator)
            {
                m_SimulatorsOpenBeforeTests = new List<int>();
                foreach (var simulator in Process.GetProcessesByName("Simulator"))
                {
                    m_SimulatorsOpenBeforeTests.Add(simulator.Id);
                }
            }
        }

        public void PostSuccessfulLaunchAction()
        {
        }

        public void CleanUp()
        {
            // Close Xcode that was opened while building the tests (batchmode only)
            // should be only one such Xcode, otherwise skip to avoid closing unrelated Xcodes
            if (Application.isEditor && Application.isBatchMode &&
                m_XcodesToCloseAfterTests != null && m_XcodesToCloseAfterTests.Count == 1)
            {
                var xcodeToClose = Process.GetProcessById(m_XcodesToCloseAfterTests[0]);

                if (xcodeToClose.ProcessName == "Xcode")
                {
                    xcodeToClose.CloseMainWindow();
                    xcodeToClose.Close();
                }
            }

            // Close all Simulator processes that were opened after successfully building the tests (batchmode only)
            if (Application.isEditor && Application.isBatchMode && m_RunOnSimulator)
            {
                foreach (var simulator in Process.GetProcessesByName("Simulator"))
                {
                    if (!m_SimulatorsOpenBeforeTests?.Contains(simulator.Id) ?? false)
                    {
                        simulator.CloseMainWindow();
                        simulator.Close();
                    }
                }
            }
        }
    }
}
