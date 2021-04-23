using System.IO;
using System.Linq;
using System.Reflection;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace VContainer.Editor.CodeGen
{
    public sealed class VContainerILPostProcessor : ILPostProcessor
    {
        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            var referenceDlls = compiledAssembly.References
                .Select(Path.GetFileNameWithoutExtension);

            return referenceDlls.Any(x => x == "VContainer") &&
                   referenceDlls.Any(x => x == "VContainer.EnableCodeGen");
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return null;

            var assemblyDefinition = Utils.LoadAssemblyDefinition(compiledAssembly);
            var generator = new InjectionILGenerator(assemblyDefinition.MainModule, compiledAssembly, null);

            if (generator.TryGenerate(out var diagnosticMessages))
            {
                if (diagnosticMessages.Any(d => d.DiagnosticType == DiagnosticType.Error))
                {
                    return new ILPostProcessResult(null, diagnosticMessages);
                }

                // TODO:
                var (selfReference, selfReferenceIndex) = assemblyDefinition.MainModule.AssemblyReferences
                    .Select((x, i) => (x, i))
                    .FirstOrDefault(e => e.x.Name == assemblyDefinition.Name.Name);

                if (selfReference != null)
                {
                    assemblyDefinition.MainModule.AssemblyReferences.RemoveAt(selfReferenceIndex);
                }

                var pe = new MemoryStream();
                var pdb = new MemoryStream();
                var writerParameters = new WriterParameters
                {
                    SymbolWriterProvider = new PortablePdbWriterProvider(),
                    SymbolStream = pdb,
                    WriteSymbols = true
                };

                assemblyDefinition.Write(pe, writerParameters);

                return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), diagnosticMessages);
            }
            return new ILPostProcessResult(null, diagnosticMessages);
        }
    }
}
