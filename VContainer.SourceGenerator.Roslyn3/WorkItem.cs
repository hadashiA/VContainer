using System.Linq;
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
            if (symbol is not INamedTypeSymbol typeSymbol)
            {
                return null;
            }
            var isAttribute = typeSymbol.GetAllBaseTypes().Any(x => SymbolEqualityComparer.Default.Equals(x, references.AttributeBase));
            if (isAttribute)
            {
                return null;
            }

            var injectIgnore = symbol.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, references.VContainerInjectIgnoreAttribute));
            if (injectIgnore)
            {
                return null;
            }

            return new TypeMeta(Syntax, typeSymbol, references);
        }
    }
}
