using System;

namespace UnityEngine.TestRunner.TestLaunchers
{
    [Serializable]
    internal class RemoteTestResultDataWithTestData
    {
        public RemoteTestResultData[] results;
        public RemoteTestData[] tests;
    }
}
