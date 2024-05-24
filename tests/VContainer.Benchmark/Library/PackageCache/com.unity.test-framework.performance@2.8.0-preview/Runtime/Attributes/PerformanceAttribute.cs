using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools;

namespace Unity.PerformanceTesting
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PerformanceAttribute : CategoryAttribute, IOuterUnityTestAction
    {
        public PerformanceAttribute()
            : base("Performance") { }

        public IEnumerator BeforeTest(ITest test)
        {
            // domain reload will cause this method to be hit multiple times
            // active performance test is serialized and survives reloads
            if (PerformanceTest.Active == null)
            {
                PerformanceTest.StartTest(test);
                yield return null;
            }
        }

        public IEnumerator AfterTest(ITest test)
        {
            PerformanceTest.EndTest(test);
            yield return null;
        }
    }
}
