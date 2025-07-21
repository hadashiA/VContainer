using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal interface ITestListCache
    {
        void CacheTest(TestPlatform platform, ITest test);
        IEnumerator<ITestAdaptor> GetTestFromCacheAsync(TestPlatform platform);
    }
}
