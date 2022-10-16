using System;
using Microsoft.CodeAnalysis;

namespace VContainer.SourceGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxCollector());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var moduleName = context.Compilation.SourceModule.Name;
                if (moduleName.StartsWith("UnityEngine.")) return;
                if (moduleName.StartsWith("UnityEditor.")) return;
                if (moduleName.StartsWith("Unity.")) return;
                if (moduleName.StartsWith("VContainer.")) return;

                var referenceSymbols = new ReferenceSymbols(context.Compilation);
                if (referenceSymbols.VContainerInjectAttribute is null) return;

                var syntaxCollector = (SyntaxCollector)context.SyntaxReceiver!;
                foreach (var workItem in syntaxCollector.WorkItems)
                {
                    GenerateResolver(workItem, referenceSymbols, in context);
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.UnexpectedErrorDescriptor,
                    Location.None,
                    ex.ToString().Replace(Environment.NewLine, " ")));
            }
        }

        static void GenerateResolver(
            WorkItem workItem,
            ReferenceSymbols referenceSymbols,
            in GeneratorExecutionContext context)
        {
            try
            {
                var typeMeta = workItem.Analyze(in context);
                if (typeMeta is null) return;

                var codeWriter = new CodeWriter();

                var fullType = typeMeta.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Replace("global::", "")
                    .Replace("<", "_")
                    .Replace(">", "_");

                //var typeName = typeMeta.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var generateTypeName = fullType.Replace(".", "_") + "_VContainerGeneratedInjector";

                codeWriter.AppendLine("using System;");
                codeWriter.AppendLine("using System.Collections.Generic;");
                codeWriter.AppendLine("using VContainer;");
                codeWriter.AppendLine();

                var ns = typeMeta.Symbol.ContainingNamespace;
                if (!ns.IsGlobalNamespace)
                {
                    codeWriter.AppendLine($"namespace {ns}");
                    codeWriter.BeginBlock();
                }

                using (codeWriter.BeginBlockScope($"class {generateTypeName} : IInjector"))
                {
                    using (codeWriter.BeginBlockScope("public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)"))
                    {
                        codeWriter.AppendLine("throw new System.NotImplementedException();");
                    }
                    codeWriter.AppendLine();

                    using (codeWriter.BeginBlockScope("public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)"))
                    {
                        codeWriter.AppendLine("throw new System.NotImplementedException();");
                    }
                }

                if (!ns.IsGlobalNamespace)
                {
                    codeWriter.EndBlock();
                }

                context.AddSource($"{fullType}.VContainerGeneratedInjector.cs", codeWriter.ToString());
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.UnexpectedErrorDescriptor,
                    Location.None,
                    ex.ToString().Replace(Environment.NewLine, " ")));
            }
        }
    }
}
