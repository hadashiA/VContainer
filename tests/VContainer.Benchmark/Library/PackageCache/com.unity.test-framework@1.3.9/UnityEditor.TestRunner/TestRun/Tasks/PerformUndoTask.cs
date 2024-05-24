using System;
using System.Collections;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class PerformUndoTask : TestTaskBase
    {
        private const double warningThreshold = 1000;

        internal Action<int> RevertAllDownToGroup = Undo.RevertAllDownToGroup;
        internal Action<string> LogWarning = Debug.LogWarning;
        internal Action<string, string, float> DisplayProgressBar = EditorUtility.DisplayProgressBar;
        internal Action ClearProgressBar = EditorUtility.ClearProgressBar;
        internal Func<DateTime> TimeNow = () => DateTime.Now;

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.undoGroup < 0)
            {
                yield break;
            }

            DisplayProgressBar("Undo", "Reverting changes to the scene", 0);

            var undoStartTime = TimeNow();

            RevertAllDownToGroup(testJobData.undoGroup);

            var timeDelta = TimeNow() - undoStartTime;
            if (timeDelta.TotalMilliseconds >= warningThreshold)
            {
                LogWarning($"Undo after editor test run took {timeDelta.Seconds} second{(timeDelta.Seconds == 1 ? "" : "s")}.");
            }

            ClearProgressBar();
        }
    }
}
