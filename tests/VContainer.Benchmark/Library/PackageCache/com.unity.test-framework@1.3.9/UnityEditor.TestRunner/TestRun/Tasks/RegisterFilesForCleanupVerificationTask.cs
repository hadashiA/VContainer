using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class RegisterFilesForCleanupVerificationTask : FileCleanupVerifierTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.existingFiles = GetAllFilesInAssetsDirectory();
            yield return null;
        }
    }
}
