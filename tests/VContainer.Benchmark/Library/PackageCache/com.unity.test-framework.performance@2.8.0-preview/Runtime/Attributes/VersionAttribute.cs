using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Unity.PerformanceTesting
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class VersionAttribute : NUnitAttribute, IApplyToTest
    {
        public string Version;

        public VersionAttribute(string version)
        {
            Version = version;
        }

        public void ApplyToTest(Test test)
        {
            test.Properties.Add("Version", this);
        }
    }
}
