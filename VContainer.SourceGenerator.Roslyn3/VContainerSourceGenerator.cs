using Microsoft.CodeAnalysis;

namespace VContainer.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class VContainerSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxCollector());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var moduleName = context.Compilation.SourceModule.Name;
        if (moduleName.StartsWith("UnityEngine.")) return;
        if (moduleName.StartsWith("UnityEditor.")) return;
        if (moduleName.StartsWith("Unity.")) return;
        if (moduleName.StartsWith("VContainer.") && !moduleName.Contains("Test")) return;

        var references = ReferenceSymbols.Create(context.Compilation);
        if (references is null) return;

        var codeWriter = new CodeWriter();
        var syntaxCollector = (SyntaxCollector)context.SyntaxReceiver!;
        foreach (var workItem in syntaxCollector.WorkItems)
        {
            if (workItem.TypeDeclarationSyntax is { } typeDeclarationSyntax)
            {
                var semanticModel = context.Compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
                var typeDeclarationCandidate = new TypeDeclarationCandidate(typeDeclarationSyntax, semanticModel);
                if (typeDeclarationCandidate.Analyze(references) is { } typeMeta)
                {
                    Execute(typeMeta, codeWriter, references, in context);
                    codeWriter.Clear();
                }
            }
            else if (workItem.RegisterInvocationSyntax is { } registerInvocationSyntax)
            {
                var semanticModel = context.Compilation.GetSemanticModel(registerInvocationSyntax.SyntaxTree);
                var registerInvocationCandidate = new RegisterInvocationCandidate(registerInvocationSyntax, semanticModel);
                var typeMetas = registerInvocationCandidate.Analyze(references);
                foreach (var typeMeta in typeMetas)
                {
                    Execute(typeMeta, codeWriter, references, in context);
                    codeWriter.Clear();
                }
            }
        }
    }

    static void Execute(TypeMeta typeMeta, CodeWriter codeWriter, ReferenceSymbols referenceSymbols, in GeneratorExecutionContext context)
    {
        if (Emitter.TryEmitGeneratedInjector(typeMeta, codeWriter, referenceSymbols, in context))
        {
            var fullType = typeMeta.FullTypeName
                .Replace("global::", "")
                .Replace("<", "_")
                .Replace(">", "_");
            context.AddSource($"{fullType}GeneratedInjector.g.cs", codeWriter.ToString());
        }
    }
}