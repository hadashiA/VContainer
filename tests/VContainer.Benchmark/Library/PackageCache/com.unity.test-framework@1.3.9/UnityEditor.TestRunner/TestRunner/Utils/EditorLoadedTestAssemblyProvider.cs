using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditorLoadedTestAssemblyProvider : IEditorLoadedTestAssemblyProvider
    {
        private const string k_NunitAssemblyName = "nunit.framework";
        private const string k_TestRunnerAssemblyName = "UnityEngine.TestRunner";
        internal const string k_PerformanceTestingAssemblyName = "Unity.PerformanceTesting";

        private readonly IEditorAssembliesProxy m_EditorAssembliesProxy;
        private readonly ScriptAssembly[] m_AllEditorScriptAssemblies;
        private readonly PrecompiledAssembly[] m_AllPrecompiledAssemblies;

        public EditorLoadedTestAssemblyProvider(IEditorCompilationInterfaceProxy compilationInterfaceProxy, IEditorAssembliesProxy editorAssembliesProxy)
        {
            m_EditorAssembliesProxy = editorAssembliesProxy;
            m_AllEditorScriptAssemblies = compilationInterfaceProxy.GetAllEditorScriptAssemblies();
            m_AllPrecompiledAssemblies = compilationInterfaceProxy.GetAllPrecompiledAssemblies();
        }

        public List<IAssemblyWrapper> GetAssembliesGroupedByType(TestPlatform mode)
        {
            var assemblies = GetAssembliesGroupedByTypeAsync(mode);
            while (assemblies.MoveNext())
            {
            }

            return assemblies.Current.Where(pair => mode.IsFlagIncluded(pair.Key)).SelectMany(pair => pair.Value).ToList();
        }

        public IEnumerator<IDictionary<TestPlatform, List<IAssemblyWrapper>>> GetAssembliesGroupedByTypeAsync(TestPlatform mode)
        {
            IAssemblyWrapper[] loadedAssemblies = m_EditorAssembliesProxy.loadedAssemblies;

            IDictionary<TestPlatform, List<IAssemblyWrapper>> result = new Dictionary<TestPlatform, List<IAssemblyWrapper>>
            {
                {TestPlatform.EditMode, new List<IAssemblyWrapper>() },
                {TestPlatform.PlayMode, new List<IAssemblyWrapper>() }
            };
            var filteredAssemblies = FilterAssembliesWithTestReference(loadedAssemblies);

            foreach (var loadedAssembly in filteredAssemblies)
            {
                var assemblyName = new FileInfo(loadedAssembly.Location).Name;
                var scriptAssemblies = m_AllEditorScriptAssemblies.Where(x => x.Filename == assemblyName).ToList();
                var precompiledAssemblies = m_AllPrecompiledAssemblies.Where(x => new FileInfo(x.Path).Name == assemblyName).ToList();
                if (scriptAssemblies.Count < 1 && precompiledAssemblies.Count < 1)
                {
                    continue;
                }

                var assemblyFlags = scriptAssemblies.Any() ? scriptAssemblies.Single().Flags : precompiledAssemblies.Single().Flags;
                var assemblyType = (assemblyFlags & AssemblyFlags.EditorOnly) == AssemblyFlags.EditorOnly ? TestPlatform.EditMode : TestPlatform.PlayMode;
                result[assemblyType].Add(loadedAssembly);
                yield return null;
            }

            yield return result;
        }
        
        private IAssemblyWrapper[] FilterAssembliesWithTestReference(IAssemblyWrapper[] loadedAssemblies)
        {
            var filteredResults = new Dictionary<IAssemblyWrapper, bool>();
            foreach (var assembly in loadedAssemblies)
            {
                FilterAssemblyForTestReference(assembly, loadedAssemblies, filteredResults, new Dictionary<IAssemblyWrapper, bool>());
            }

            return filteredResults.Where(pair => pair.Value).Select(pair => pair.Key).ToArray();
        }
        
        private void FilterAssemblyForTestReference(IAssemblyWrapper assemblyToFilter, IAssemblyWrapper[] loadedAssemblies, 
            IDictionary<IAssemblyWrapper, bool> filterResults, IDictionary<IAssemblyWrapper, bool> resultsAlreadyAnalyzed)
        {
            if(resultsAlreadyAnalyzed.ContainsKey(assemblyToFilter))
            {
                return;
            }

            resultsAlreadyAnalyzed[assemblyToFilter] = true;

            var references = assemblyToFilter.GetReferencedAssemblies();
            if (references.Any(IsTestReference))
            {
                filterResults[assemblyToFilter] = true;
                return;
            }

            foreach (var reference in references)
            {
                var referencedAssembly = loadedAssemblies.FirstOrDefault(a => a.Name.Name == reference.Name);
                if (referencedAssembly == null)
                {
                    continue;
                }

                FilterAssemblyForTestReference(referencedAssembly, loadedAssemblies, filterResults, resultsAlreadyAnalyzed);
                
                if (filterResults.ContainsKey(referencedAssembly) && filterResults[referencedAssembly])
                {
                    filterResults[assemblyToFilter] = true;
                    return;
                }
            }
            
            filterResults[assemblyToFilter] = false;
        }

        private static bool IsTestReference(AssemblyName assemblyName)
        {
            return assemblyName.Name == k_NunitAssemblyName || 
                   assemblyName.Name == k_TestRunnerAssemblyName ||
                   assemblyName.Name == k_PerformanceTestingAssemblyName;
        }
    }
}
