using System.IO;
using System.Linq;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using Mono.Cecil;
using Mono.Cecil.Cil;
using VContainer.Unity;

namespace VContainer.Editor.CodeGen
{
    public sealed class VContainerILPostProcessor : ILPostProcessor
    {
        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            var settings = VContainerSettings.Instance;
            return settings != null &&
                   settings.CodeGen.Enabled &&
                   settings.CodeGen.AssemblyNames.Contains(compiledAssembly.Name) &&
                   compiledAssembly.References.Any(x => x.EndsWith("VContainer.dll"));
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return null;

            var assemblyDefinition = Utils.LoadAssemblyDefinition(compiledAssembly);

            var generator = new InjectionILGenerator(
                assemblyDefinition.MainModule,
                VContainerSettings.Instance.CodeGen.Namespaces);

            if (generator.TryGenerate(out var diagnosticMessages))
            {
                if (diagnosticMessages.Any(d => d.DiagnosticType == DiagnosticType.Error))
                {
                    return new ILPostProcessResult(null, diagnosticMessages);
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
