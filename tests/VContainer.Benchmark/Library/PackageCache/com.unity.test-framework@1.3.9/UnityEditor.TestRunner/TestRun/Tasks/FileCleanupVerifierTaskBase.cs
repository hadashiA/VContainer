using System;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal abstract class FileCleanupVerifierTaskBase : TestTaskBase
    {
        internal Func<string[]> GetAllAssetPathsAction = AssetDatabase.GetAllAssetPaths;

        protected string[] GetAllFilesInAssetsDirectory()
        {
            return GetAllAssetPathsAction();
        }
    }
}
