using System;
using System.Collections;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal abstract class WorkItemFactory
    {
        public UnityWorkItem Create(ITest loadedTest, ITestFilter filter)
        {
            TestSuite suite = loadedTest as TestSuite;
            if (suite != null)
            {
                return new CompositeWorkItem(suite, filter, this);
            }

            var testMethod = (TestMethod)loadedTest;
            if (testMethod.Method.ReturnType.Type != typeof(IEnumerator))
            {
                return new DefaultTestWorkItem(testMethod, filter);
            }

            return Create(testMethod, filter, loadedTest);
        }

        protected abstract UnityWorkItem Create(TestMethod method, ITestFilter filter, ITest loadedTest);
    }
}
