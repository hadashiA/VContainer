using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class SaveUndoIndexTask : TestTaskBase
    {
        internal Func<int> GetUndoGroup = Undo.GetCurrentGroup;
        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.undoGroup = GetUndoGroup();
            yield break;
        }
    }
}
