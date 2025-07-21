using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal interface ITestListCacheData
    {
        List<TestPlatform> platforms { get; }
        List<ITest> cachedData { get; }
    }
}
