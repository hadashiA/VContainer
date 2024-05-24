using System;
using System.Collections;
using UnityEngine;
using System.Linq;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal abstract class TestTaskBase
    {
        public ErrorRunMode RunOnError = ErrorRunMode.DoNotRunOnError;

        public bool RerunAfterResume;
        public bool RerunAfterEnteredEditMode;
        public bool SupportsResumingEnumerator;
        public bool RunOnCancel;
        public bool CanRunInstantly = true;

        public abstract IEnumerator Execute(TestJobData testJobData);

        public enum ErrorRunMode
        {
            DoNotRunOnError,
            RunOnlyOnError,
            RunAlways
        }

        public virtual string GetName()
        {
            return GetType().Name;
        }

        public string GetTitle()
        {
            var name = GetName();
            if (name.EndsWith("Task")) // Trim away the Task part of the title in the end.
            {
                name = name.Substring(0, name.Length - 4);
            }

            var title = string.Empty;
            for (var i = 0; i < name.Length; i++)
            {
                var nameChar = name[i];
                if (i == 0 || (nameChar >= 'a' && nameChar <= 'z'))
                {
                    title += nameChar;
                }
                else
                {
                    title += " " + nameChar;
                }
            }

            return title;
        }

        public bool ShouldExecute(TaskInfo taskInfo)
        {
            switch (taskInfo.taskMode)
            {
                case TaskMode.Error:
                    return RunOnError == ErrorRunMode.RunAlways || RunOnError == ErrorRunMode.RunOnlyOnError;
                case TaskMode.Resume:
                    return RerunAfterResume;
                case TaskMode.EnteredEditMode:
                    return RerunAfterEnteredEditMode;
                case TaskMode.Canceled:
                    return RunOnCancel;
                default:
                    return RunOnError != ErrorRunMode.RunOnlyOnError;
            }
        }
        
        protected static bool IsAutomated()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            return commandLineArgs.Contains("-automated");
        }
    }
}
