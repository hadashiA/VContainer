using System;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal interface ITestTreeRebuildCallbacks : ICallbacks
    {
        void TestTreeRebuild(ITestAdaptor test);
    }
}
