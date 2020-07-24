using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace VContainer.Editor.CodeGen
{
    class PostProcessorReflectionImporterProvider : IReflectionImporterProvider
    {
        public IReflectionImporter GetReflectionImporter(ModuleDefinition module)
        {
            return new PostProcessorReflectionImporter(module);
        }
    }

    class PostProcessorReflectionImporter : DefaultReflectionImporter
    {
        const string SystemPrivateCoreLib = "System.Private.CoreLib";
        readonly AssemblyNameReference correctCorlib;

        public PostProcessorReflectionImporter(ModuleDefinition module) : base(module)
        {
            correctCorlib = module.AssemblyReferences.FirstOrDefault(a =>
            {
                return a.Name == "mscorlib" || a.Name == "netstandard" || a.Name == SystemPrivateCoreLib;
            });
        }

        public override AssemblyNameReference ImportReference(AssemblyName reference)
        {
            if (correctCorlib != null && reference.Name == SystemPrivateCoreLib)
                return correctCorlib;

            return base.ImportReference(reference);
        }
    }
}