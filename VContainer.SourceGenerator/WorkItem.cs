using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VContainer.SourceGenerator
{
    class WorkItem
    {
        public TypeDeclarationSyntax Syntax { get; }

        public WorkItem(TypeDeclarationSyntax syntax)
        {
            Syntax = syntax;
        }

        public TypeMeta? Analyze(in GeneratorExecutionContext context, ReferenceSymbols references)
        {
            var semanticModel = context.Compilation.GetSemanticModel(Syntax.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(Syntax, context.CancellationToken);
            if (symbol is INamedTypeSymbol typeSymbol)
            {
                return new TypeMeta(Syntax, typeSymbol, references);
            }
            return null;
        }
    }
}

