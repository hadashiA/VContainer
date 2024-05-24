using System;
using NUnit.Framework.Internal;

namespace UnityEngine.TestRunner.NUnitExtensions
{
    internal interface ITestSuiteModifier
    {
        TestSuite ModifySuite(TestSuite suite);
    }
}
