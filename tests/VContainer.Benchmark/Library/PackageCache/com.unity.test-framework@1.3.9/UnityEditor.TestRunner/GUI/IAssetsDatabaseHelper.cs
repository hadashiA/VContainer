using System;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal interface IAssetsDatabaseHelper
    {
        void OpenAssetInItsDefaultExternalEditor(string assetPath, int line);
    }
}
