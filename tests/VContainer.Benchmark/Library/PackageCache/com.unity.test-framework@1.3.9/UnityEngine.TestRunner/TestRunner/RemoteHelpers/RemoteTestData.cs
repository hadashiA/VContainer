using System;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine.TestRunner.NUnitExtensions;

namespace UnityEngine.TestRunner.TestLaunchers
{
    [Serializable]
    internal class RemoteTestData
    {
        public string id;
        public string name;
        public string fullName;
        public int testCaseCount;
        public int ChildIndex;
        public bool hasChildren;
        public bool isSuite;
        public string[] childrenIds;
        public int testCaseTimeout;
        public string[] Categories;
        public bool IsTestAssembly;
        public RunState RunState;
        public string Description;
        public string SkipReason;
        public string ParentId;
        public string UniqueName;
        public string ParentUniqueName;
        public string ParentFullName;

        internal RemoteTestData(ITest test)
        {
            id = test.Id;
            name = test.Name;
            fullName = test.FullName;
            testCaseCount = test.TestCaseCount;
            ChildIndex = -1;
            if (test.Properties["childIndex"].Count > 0)
            {
                ChildIndex = (int)test.Properties["childIndex"][0];
            }
            hasChildren = test.HasChildren;
            isSuite = test.IsSuite;
            childrenIds = test.Tests.Select(t => t.Id).ToArray();
            Categories = test.GetAllCategoriesFromTest().ToArray();
            IsTestAssembly = test is TestAssembly;
            RunState = (RunState)Enum.Parse(typeof(RunState), test.RunState.ToString());
            Description = (string)test.Properties.Get(PropertyNames.Description);
            SkipReason = test.GetSkipReason();
            ParentId = test.GetParentId();
            UniqueName = test.GetUniqueName();
            ParentUniqueName = test.GetParentUniqueName();
            ParentFullName = test.GetParentFullName();
        }
    }
}
