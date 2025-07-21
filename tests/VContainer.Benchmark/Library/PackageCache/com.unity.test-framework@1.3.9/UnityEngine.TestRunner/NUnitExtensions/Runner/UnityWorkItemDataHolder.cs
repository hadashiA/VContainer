using System;
using System.Collections.Generic;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal class UnityWorkItemDataHolder
    {
        public static List<string> alreadyStartedTests = new List<string>();
        public static List<string> alreadyExecutedTests;
    }
}
