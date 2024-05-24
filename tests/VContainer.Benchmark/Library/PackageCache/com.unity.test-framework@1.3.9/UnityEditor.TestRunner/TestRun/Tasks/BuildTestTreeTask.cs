using System;
using System.Collections;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class BuildTestTreeTask : TestTaskBase
    {
        private TestPlatform m_TestPlatform;

        public BuildTestTreeTask(TestPlatform testPlatform)
        {
            m_TestPlatform = testPlatform;
            RerunAfterResume = true;
        }

        internal IEditorLoadedTestAssemblyProvider m_testAssemblyProvider = new EditorLoadedTestAssemblyProvider(new EditorCompilationInterfaceProxy(), new EditorAssembliesProxy());
        internal Func<string[], int, IAsyncTestAssemblyBuilder> m_testAssemblyBuilderFactory = (orderedTestNames, seed) => new UnityTestAssemblyBuilder(orderedTestNames, seed);
        internal ICallbacksDelegator m_CallbacksDelegator = CallbacksDelegator.instance;

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.testTree != null)
            {
                yield break;
            }

            var assembliesEnumerator = m_testAssemblyProvider.GetAssembliesGroupedByTypeAsync(m_TestPlatform);
            while (assembliesEnumerator.MoveNext())
            {
                yield return null;
            }

            if (assembliesEnumerator.Current == null)
            {
                throw new Exception("Assemblies not retrieved.");
            }

            var assemblies = assembliesEnumerator.Current.Where(pair => m_TestPlatform.IsFlagIncluded(pair.Key)).SelectMany(pair => pair.Value).Select(x => x.Assembly).ToArray();
            var buildSettings = UnityTestAssemblyBuilder.GetNUnitTestBuilderSettings(m_TestPlatform);
            var testAssemblyBuilder = m_testAssemblyBuilderFactory(testJobData.executionSettings.orderedTestNames, testJobData.executionSettings.randomOrderSeed);
            var enumerator = testAssemblyBuilder.BuildAsync(assemblies, Enumerable.Repeat(m_TestPlatform, assemblies.Length).ToArray(), buildSettings);
            while (enumerator.MoveNext())
            {
                yield return null;
            }

            var testList = enumerator.Current;
            if (testList== null)
            {
                throw new Exception("Test list not retrieved.");
            }

            testJobData.testTree = testList;
            m_CallbacksDelegator.TestTreeRebuild(testList);
        }
    }
}
