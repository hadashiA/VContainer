using System.Collections.Generic;
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
            if (symbol is INamedTypeSymbol typeSymbol)
            {
                return new TypeMeta(typeSymbol, references);
            }
            return null;
        }
    }

    class TypeMeta
    {
        public INamedTypeSymbol Symbol { get; }
        public string TypeName { get; }
        public IMethodSymbol? Constructor { get; private set; }
        public DiagnosticDescriptor? CtorInvalid { get; private set; }

        public bool IsUseEmptyConstructor => Constructor == null ||
                                             Constructor.Parameters.IsEmpty;

        ReferenceSymbols references;

        public TypeMeta(INamedTypeSymbol symbol, ReferenceSymbols references)
        {
            Symbol = symbol;
            this.references = references;

            TypeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            DetectConstructor();
        }

        public bool InheritsFrom(INamedTypeSymbol baseSymbol)
        {
            var baseName = baseSymbol.ToString();
            var symbol = Symbol;
            while (true)
            {
                if (symbol.ToString() == baseName)
                {
                    return true;
                }
                if (symbol.BaseType != null)
                {
                    symbol = symbol.BaseType;
                    continue;
                }
                break;
            }
            return false;
        }

        public IReadOnlyList<IMethodSymbol> GetInjectFields()
        {
        }

        public IReadOnlyList<IMethodSymbol> GetInjectProperties()
        {
        }

        public IReadOnlyList<IMethodSymbol> GetInjectMethod()
        {

        }

        void DetectConstructor()
        {
            var ctors = Symbol.InstanceConstructors
                .Where(x => !x.IsImplicitlyDeclared) // remove empty ctor(struct always generate it), record's clone ctor
                .ToArray();

            if (ctors.Length == 0)
            {
                return;
            }

            if (ctors.Length > 0)
            {
                var ctorWithAttrs = ctors.Where(ctor =>
                {
                    return ctor.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, references.VContainerInjectAttribute));
                }).ToArray();

                if (ctorWithAttrs.Length == 0)
                {
                    CtorInvalid = DiagnosticDescriptors.MultipleCtorWithoutAttribute;
                    return;
                }

                if (ctorWithAttrs.Length == 1)
                {
                    Constructor = ctorWithAttrs[0]; // ok
                }
            }
            else
            {
                Constructor = ctors[0];
                return;
            }

            CtorInvalid = DiagnosticDescriptors.MultipleCtorAttribute;
        }
    }
}
