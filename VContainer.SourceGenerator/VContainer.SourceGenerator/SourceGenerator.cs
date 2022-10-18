using System;
using System.Linq;
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
            ReferenceSymbols references,
            in GeneratorExecutionContext context)
        {
            try
            {
                var typeMeta = workItem.Analyze(in context, references);
                if (typeMeta is null) return;

                var codeWriter = new CodeWriter();

                var fullType = typeMeta.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Replace("global::", "")
                    .Replace("<", "_")
                    .Replace(">", "_");

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
                        if (typeMeta.InheritsFrom(references.UnityEngineComponent))
                        {
                            codeWriter.AppendLine($"throw new NotSupportedException(UnityEngine.Component:{} cannot be `new`);");
                        }
                        else if (typeMeta.IsUseEmptyConstructor)
                        {
                            codeWriter.AppendLine($"return new {typeMeta.TypeName}();");
                        }
                        else
                        {
                            var parameters = typeMeta.Constructor!.Parameters
                                .Select(param =>
                                {
                                    var paramType = param.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                                    var paramName = param.Name;
                                    return (paramType, paramName);
                                })
                                .ToArray();

                            foreach (var (paramType, paramName) in parameters)
                            {
                                codeWriter.AppendLine($"var {paramName} = resolver.ResolveOrParameter(typeof({paramType}), \"{paramName}\", parameters);");
                            }

                            var arguments = parameters.Select(x => $"{x.paramType} {x.paramName}");
                            codeWriter.AppendLine($"new {typeMeta.TypeName}({string.Join(", ", arguments)});");
                        }
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
