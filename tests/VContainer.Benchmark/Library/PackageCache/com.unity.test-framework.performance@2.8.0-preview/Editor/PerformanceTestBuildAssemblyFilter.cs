using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace Unity.PerformanceTesting.Editor
{
    internal class PerformanceTestBuildAssemblyFilter : IFilterBuildAssemblies
    {
        private const string unityTestRunnerAssemblyName = "Unity.PerformanceTesting";

        public int callbackOrder { get; }

        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            if ((buildOptions & BuildOptions.IncludeTestAssemblies) == BuildOptions.IncludeTestAssemblies)
            {
                return assemblies;
            }

            return assemblies.Where(x => !x.Contains(unityTestRunnerAssemblyName))
                .ToArray();
        }
    }
}
