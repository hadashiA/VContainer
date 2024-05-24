using System;

namespace UnityEditor.TestTools.TestRunner.Api.Analytics
{
    internal class TestTreeData
    {
        public int totalNumberOfTests;
        public int numTestAttributes;
        public int numUnityTestAttributes;
        public int numCategoryAttributes;
        public int numTestFixtureAttributes;
        public int numConditionalIgnoreAttributes;
        public int numRequiresPlayModeAttributesTrue;
        public int numRequiresPlayModeAttributesFalse;
        public int numUnityPlatformAttributes;
    }
}
