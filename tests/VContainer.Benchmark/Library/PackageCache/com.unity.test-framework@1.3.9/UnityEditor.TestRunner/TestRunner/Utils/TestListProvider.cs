using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;

namespace UnityEditor.TestTools.TestRunner
{
    internal class TestListProvider : ITestListProvider
    {
        private readonly EditorLoadedTestAssemblyProvider m_AssemblyProvider;
        private readonly UnityTestAssemblyBuilder m_AssemblyBuilder;

        public TestListProvider(EditorLoadedTestAssemblyProvider assemblyProvider, UnityTestAssemblyBuilder assemblyBuilder)
        {
            m_AssemblyProvider = assemblyProvider;
            m_AssemblyBuilder = assemblyBuilder;
        }

        public IEnumerator<ITest> GetTestListAsync(TestPlatform platform)
        {
            var assembliesTask = m_AssemblyProvider.GetAssembliesGroupedByTypeAsync(platform);
            while (assembliesTask.MoveNext())
            {
                yield return null;
            }

            var assemblies = assembliesTask.Current.Where(pair => platform.IsFlagIncluded(pair.Key))
                .SelectMany(pair => pair.Value.Select(assemblyInfo => Tuple.Create(assemblyInfo.Assembly, pair.Key))).ToArray();

            var settings = UnityTestAssemblyBuilder.GetNUnitTestBuilderSettings(platform);
            var test =  m_AssemblyBuilder.BuildAsync(assemblies.Select(a => a.Item1).ToArray(), assemblies.Select(a => a.Item2).ToArray(), settings);
            while (test.MoveNext())
            {
                yield return null;
            }

            yield return test.Current;
        }
    }
}
