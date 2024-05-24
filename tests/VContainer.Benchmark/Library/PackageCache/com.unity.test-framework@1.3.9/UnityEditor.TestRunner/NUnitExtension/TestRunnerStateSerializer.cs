using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.Logging;
using UnityEngine.TestTools.NUnitExtensions;

namespace UnityEditor.TestTools.TestRunner
{
    [Serializable]
    internal class TestRunnerStateSerializer : IStateSerializer
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        [SerializeField]
        private HideFlags m_OriginalHideFlags;

        [SerializeField]
        private bool m_ShouldRestore;

        [SerializeField]
        private string m_TestObjectTypeName;

        [SerializeField]
        private ScriptableObject m_TestObject;

        [SerializeField]
        private string m_TestObjectTxt;

        [SerializeField]
        private long StartTicks;

        [SerializeField]
        private double StartTimeOA;

        [SerializeField]
        private string output;

        [SerializeField]
        private LogMatch[] m_ExpectedLogs;

        public bool ShouldRestore()
        {
            return m_ShouldRestore;
        }

        public void SaveContext()
        {
            var currentContext = UnityTestExecutionContext.CurrentContext;

            if (currentContext.TestObject != null)
            {
                m_TestObjectTypeName = currentContext.TestObject.GetType().AssemblyQualifiedName;
                m_TestObject = null;
                m_TestObjectTxt = null;
                if (currentContext.TestObject is ScriptableObject)
                {
                    m_TestObject = currentContext.TestObject as ScriptableObject;
                    m_OriginalHideFlags = m_TestObject.hideFlags;
                    m_TestObject.hideFlags |= HideFlags.DontSave;
                }
                else
                {
                    m_TestObjectTxt = JsonUtility.ToJson(currentContext.TestObject);
                }
            }

            output = currentContext.CurrentResult.Output;
            StartTicks = currentContext.StartTicks;
            StartTimeOA = currentContext.StartTime.ToOADate();
            if (LogScope.HasCurrentLogScope())
            {
                m_ExpectedLogs = LogScope.Current.ExpectedLogs.ToArray();
            }

            m_ShouldRestore = true;
        }

        public void RestoreContext()
        {
            var currentContext = UnityTestExecutionContext.CurrentContext;

            var outputProp = currentContext.CurrentResult.GetType().BaseType.GetField("_output", Flags);
            (outputProp.GetValue(currentContext.CurrentResult) as StringBuilder).Append(output);

            currentContext.StartTicks = StartTicks;
            currentContext.StartTime = DateTime.FromOADate(StartTimeOA);
            if (LogScope.HasCurrentLogScope())
            {
                LogScope.Current.ExpectedLogs = new Queue<LogMatch>(m_ExpectedLogs);
            }

            m_ShouldRestore = false;
        }

        public bool CanRestoreFromScriptableObject(Type requestedType)
        {
            if (m_TestObject == null)
            {
                return false;
            }
            return m_TestObjectTypeName == requestedType.AssemblyQualifiedName;
        }

        public ScriptableObject RestoreScriptableObjectInstance()
        {
            if (m_TestObject == null)
            {
                Debug.LogError("No object to restore");
                return null;
            }
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            var temp = m_TestObject;
            m_TestObject = null;
            m_TestObjectTypeName = null;
            return temp;
        }

        public bool CanRestoreFromJson(Type requestedType)
        {
            if (string.IsNullOrEmpty(m_TestObjectTxt))
            {
                return false;
            }
            return m_TestObjectTypeName == requestedType.AssemblyQualifiedName;
        }

        public void RestoreClassFromJson(ref object instance)
        {
            if (string.IsNullOrEmpty(m_TestObjectTxt))
            {
                Debug.LogWarning("No JSON representation to restore");
                return;
            }
            JsonUtility.FromJsonOverwrite(m_TestObjectTxt, instance);
            m_TestObjectTxt = null;
            m_TestObjectTypeName = null;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (m_TestObject == null)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                return;
            }

            //We set the DontSave flag here because the ScriptableObject would be nulled right before entering EditMode
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                m_TestObject.hideFlags |= HideFlags.DontSave;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                m_TestObject.hideFlags = m_OriginalHideFlags;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
        }
    }
}
