using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestRunner.TestLaunchers;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal class TestAdaptor : ITestAdaptor
    {
        internal TestAdaptor(ITest test, ITestAdaptor[] children = null)
        {
            Id = test.Id;
            Name = test.Name;
            var childIndex = -1;
            if (test.Properties["childIndex"].Count > 0)
            {
                childIndex = (int)test.Properties["childIndex"][0];
            }
            FullName = TestExtensions.GetFullName(test.FullName, childIndex);
            TestCaseCount = test.TestCaseCount;
            HasChildren = test.HasChildren;
            IsSuite = test.IsSuite;
            if (UnityTestExecutionContext.CurrentContext != null)
            {
                TestCaseTimeout = UnityTestExecutionContext.CurrentContext.TestCaseTimeout;
            }
            else
            {
                TestCaseTimeout = TimeoutCommand.k_DefaultTimeout;
            }

            TypeInfo = test.TypeInfo;
            Method = test.Method;
            Arguments = test is TestMethod testMethod ? testMethod.parms?.Arguments : (test as TestSuite)?.Arguments;
            Categories = test.GetAllCategoriesFromTest().Distinct().ToArray();
            IsTestAssembly = test is TestAssembly;
            RunState = (RunState)Enum.Parse(typeof(RunState), test.RunState.ToString());
            Description = (string)test.Properties.Get(PropertyNames.Description);
            SkipReason = test.GetSkipReason();
            ParentId = test.GetParentId();
            ParentFullName = test.GetParentFullName();
            UniqueName = test.GetUniqueName();
            ParentUniqueName = test.GetParentUniqueName();
            ChildIndex = childIndex;
            
            var testPlatform = test.Properties.Get("platform");
            if (testPlatform is TestPlatform platform)
            {
                TestMode = PlatformToTestMode(platform);
            }

            Children = children;
        }

        public void SetParent(ITestAdaptor parent)
        {
            Parent = parent;
            if (parent != null)
            {
                TestMode = parent.TestMode;
            }
        }

        internal TestAdaptor(RemoteTestData test)
        {
            Id = test.id;
            Name = test.name;
            FullName = TestExtensions.GetFullName(test.fullName, test.ChildIndex);
            TestCaseCount = test.testCaseCount;
            HasChildren = test.hasChildren;
            IsSuite = test.isSuite;
            m_ChildrenIds = test.childrenIds;
            TestCaseTimeout = test.testCaseTimeout;
            Categories = test.Categories;
            IsTestAssembly = test.IsTestAssembly;
            RunState = (RunState)Enum.Parse(typeof(RunState), test.RunState.ToString());
            Description = test.Description;
            SkipReason = test.SkipReason;
            ParentId = test.ParentId;
            UniqueName = test.UniqueName;
            ParentUniqueName = test.ParentUniqueName;
            ParentFullName = test.ParentFullName;
            ChildIndex = test.ChildIndex;
            TestMode = TestMode.PlayMode;
        }

        internal void ApplyChildren(IEnumerable<TestAdaptor> allTests)
        {
            Children = m_ChildrenIds.Select(id => allTests.First(t => t.Id == id)).ToArray();
            if (!string.IsNullOrEmpty(ParentId))
            {
                Parent = allTests.FirstOrDefault(t => t.Id == ParentId);
            }
        }

        private static TestMode PlatformToTestMode(TestPlatform testPlatform)
        {
            switch (testPlatform)
            {
                case TestPlatform.All:
                    return TestMode.EditMode | TestMode.PlayMode;
                case TestPlatform.EditMode:
                    return TestMode.EditMode;
                case TestPlatform.PlayMode:
                    return TestMode.PlayMode;
                default:
                    return default;
            }
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public int TestCaseCount { get; private set; }
        public bool HasChildren { get; private set; }
        public bool IsSuite { get; private set; }
        public IEnumerable<ITestAdaptor> Children { get; private set; }
        public ITestAdaptor Parent { get; private set; }
        public int TestCaseTimeout { get; private set; }
        public ITypeInfo TypeInfo { get; private set; }
        public IMethodInfo Method { get; private set; }
        public object[] Arguments { get; }
        private string[] m_ChildrenIds;
        public string[] Categories { get; private set; }
        public bool IsTestAssembly { get; private set; }
        public RunState RunState { get; }
        public string Description { get; }
        public string SkipReason { get; }
        public string ParentId { get; }
        public string ParentFullName { get; }
        public string UniqueName { get; }
        public string ParentUniqueName { get; }
        public int ChildIndex { get; }
        public TestMode TestMode { get; private set; }
    }
}
