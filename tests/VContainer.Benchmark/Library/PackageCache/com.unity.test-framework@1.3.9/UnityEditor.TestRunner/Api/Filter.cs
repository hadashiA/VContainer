using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.TestTools.TestRunner.GUI;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// The filter class provides the <see cref="TestRunnerApi"/> with a specification of what tests to run when [running tests programmatically](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/extension-run-tests.html).
    /// </summary>
    [Serializable]
    public class Filter
    {
        /// <summary>
        /// An enum flag that specifies if Edit Mode or Play Mode tests should run.
        ///</summary>
        [SerializeField]
        public TestMode testMode;
        /// <summary>
        /// The full name of the tests to match the filter. This is usually in the format FixtureName.TestName. If the test has test arguments, then include them in parenthesis. E.g. MyTestClass2.MyTestWithMultipleValues(1).
        /// </summary>
        [SerializeField]
        public string[] testNames;
        /// <summary>
        /// The same as testNames, except that it allows for Regex. This is useful for running specific fixtures or namespaces. E.g. "^MyNamespace\\." Runs any tests where the top namespace is MyNamespace.
        /// </summary>
        [SerializeField]
        public string[] groupNames;
        /// <summary>
        /// The name of a [Category](https://nunit.org/docs/2.2.7/category.html) to include in the run. Any test or fixtures runs that have a Category matching the string.
        /// </summary>
        [SerializeField]
        public string[] categoryNames;
        /// <summary>
        /// The name of assemblies included in the run. That is the assembly name, without the .dll file extension. E.g., MyTestAssembly
        /// </summary>
        [SerializeField]
        public string[] assemblyNames;
        /// <summary>
        /// The <see cref="BuildTarget"/> platform to run the test on. If set to null, then the Editor is the target for the tests.
        /// Obsolete. Use the targetPlatform property on the <see cref="ExecutionSettings"/>.
        /// </summary>
        public BuildTarget? targetPlatform;

        /// <summary>
        /// Implementation of ToString() that builds a string composed of the filter values.
        /// </summary>
        /// <returns>The current filter values as a string.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(Filter)} with settings:");
            stringBuilder.AppendLine($"{nameof(testMode)} = {testMode}");
            stringBuilder.AppendLine($"{nameof(targetPlatform)} = {targetPlatform}");
            stringBuilder.AppendLine($"{nameof(testNames)} = " + (testNames == null ? "null" : string.Join(", ", testNames)));
            stringBuilder.AppendLine($"{nameof(groupNames)} = " + (groupNames == null ? "null" : string.Join(", ", groupNames)));
            stringBuilder.AppendLine($"{nameof(categoryNames)} = " + (categoryNames == null ? "null" : string.Join(", ", categoryNames)));
            stringBuilder.AppendLine($"{nameof(assemblyNames)} = " + (assemblyNames == null ? "null" : string.Join(", ", assemblyNames)));

            return stringBuilder.ToString();
        }
        
        internal RuntimeTestRunnerFilter ToRuntimeTestRunnerFilter(bool synchronousOnly)
        {
            return new RuntimeTestRunnerFilter
            {
                testNames = testNames,
                categoryNames = categoryNames,
                groupNames = groupNames,
                assemblyNames = assemblyNames,
                synchronousOnly = synchronousOnly
            };
        }
        
        
        internal bool HasAny()
        {
            return assemblyNames != null && assemblyNames.Any()
                   || categoryNames != null && categoryNames.Any()
                   || groupNames != null && groupNames.Any()
                   || testNames != null && testNames.Any();
        }
    }
}
