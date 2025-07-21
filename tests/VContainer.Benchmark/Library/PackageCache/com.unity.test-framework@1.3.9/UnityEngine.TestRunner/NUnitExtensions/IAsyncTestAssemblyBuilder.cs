using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools.NUnitExtensions
{
    internal interface IAsyncTestAssemblyBuilder : ITestAssemblyBuilder
    {
        IEnumerator<ITest> BuildAsync(Assembly[] assemblies, TestPlatform[] testPlatforms, IDictionary<string, object> options);
    }
}
