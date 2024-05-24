using System;
using System.Collections;
using NUnit.Framework.Internal;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal interface IEnumerableTestMethodCommand
    {
        IEnumerable ExecuteEnumerable(ITestExecutionContext context);
    }
}
