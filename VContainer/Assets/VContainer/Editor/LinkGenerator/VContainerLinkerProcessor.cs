using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.UnityLinker;

namespace VContainer.Editor.LinkGenerator
{
    /*
     * TODO
     * Define the way to write tests
     * Cover all parts by tests
     * Finish all todos
     * Don't forget that InjectTypeInfo.Type has changed
     */
    internal sealed class VContainerLinkerProcessor : IUnityLinkerProcessor, IPostprocessBuildWithReport
    {
        public static readonly string LinkXmlDirectoryPath = Path.Combine("Assets", "VContainer.LinkGenerator");
        public static readonly string LinkXmlFilePath = Path.Combine(LinkXmlDirectoryPath, "link.xml");
        public int callbackOrder { get; }

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            UnityEditor.Compilation.Assembly[] unityAssemblies = CompilationPipeline.GetAssemblies(
#if UNITY_2017_4_OR_NEWER
                AssembliesType.PlayerWithoutTestAssemblies
#endif
            );

            var toProcess = new List<System.Reflection.Assembly>();

            foreach (UnityEditor.Compilation.Assembly unityAssembly in unityAssemblies) {
                var dllReferences = unityAssembly.allReferences.Select(Path.GetFileNameWithoutExtension).ToList();

                if (dllReferences.Any(x => x == "VContainer") &&
                    dllReferences.Any(x => x == "VContainer.EnableLinkGenerator"))
                    toProcess.Add(System.Reflection.Assembly.Load(new AssemblyName(unityAssembly.name)));
            }

            if (toProcess.Count == 0)
                return null;

            if (!Directory.Exists(LinkXmlDirectoryPath))
                Directory.CreateDirectory(LinkXmlDirectoryPath);

            // TODO if file empty unity linker will be crash
            using (var stream = File.CreateText(LinkXmlFilePath)) {
                LinkGeneratorTypeHelper.Generate(stream, toProcess);
            }

            return LinkXmlFilePath;
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            // string fullPath = Path.Combine(Application.dataPath, "VContainer.LinkGenerator");
            //
            // if (Directory.Exists(fullPath))
            //     Directory.Delete(fullPath, true);
        }
    }
}