using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace VContainer.Editor.CodeGen
{
    static class Utils
    {
        public static AssemblyDefinition LoadAssemblyDefinition(ICompiledAssembly compiledAssembly)
        {
            var resolver = new PostProcessorAssemblyResolver(compiledAssembly);
            var readerParameters = new ReaderParameters
            {
                SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData.ToArray()),
                SymbolReaderProvider = new PortablePdbReaderProvider(),
                AssemblyResolver = resolver,
                ReflectionImporterProvider = new PostProcessorReflectionImporterProvider(),
                ReadingMode = ReadingMode.Immediate
            };

            var peStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData.ToArray());
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(peStream, readerParameters);

            //apparently, it will happen that when we ask to resolve a type that lives inside Unity.Entities, and we
            //are also postprocessing Unity.Entities, type resolving will fail, because we do not actually try to resolve
            //inside the assembly we are processing. Let's make sure we do that, so that we can use postprocessor features inside
            //unity.entities itself as well.
            resolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition);

            return assemblyDefinition;
        }

        public static System.Type GetType(this TypeReference type)
        {
            return System.Type.GetType(GetReflectionName(type), true);
        }

        public static string GetReflectionName(TypeReference typeRef)
        {
            if (typeRef.IsGenericInstance)
            {
                var genericInstance = (GenericInstanceType)typeRef;
                var genericArgsName = string.Join(",",
                    genericInstance.GenericArguments
                        .Select(GetReflectionName)
                        .ToArray());
                return $"{genericInstance.Namespace}.{typeRef.Name}[{genericArgsName}]";
            }
            return typeRef.FullName;
        }
   }
}