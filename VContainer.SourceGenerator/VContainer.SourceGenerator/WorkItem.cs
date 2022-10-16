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

        public TypeMeta? Analyze(in GeneratorExecutionContext context)
        {
            var semanticModel = context.Compilation.GetSemanticModel(Syntax.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(Syntax, context.CancellationToken);
            if (symbol is INamedTypeSymbol typeSymbol)
            {
                return new TypeMeta(typeSymbol);
            }
            return null;
        }
    }

    class TypeMeta
    {
        public INamedTypeSymbol Symbol { get; }
        public IMethodSymbol? Constructor { get; }

        public bool IsUseEmptyConstructor => Constructor == null || Constructor.Parameters.IsEmpty;

        public TypeMeta(INamedTypeSymbol symbol)
        {
            Symbol = symbol;
        }

        bool InheritsFrom<T>(INamedTypeSymbol symbol)
        {
            var baseName = typeof(T).FullName;
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

        public bool TryGetConstructor(
            INamedTypeSymbol symbol,
            INamedTypeSymbol injectAttributeSymbol,
            out IMethodSymbol? result,
            out DiagnosticDescriptor? diagnosticDescriptor)
        {
            var ctors = symbol.InstanceConstructors
                .Where(x => !x.IsImplicitlyDeclared) // remove empty ctor(struct always generate it), record's clone ctor
                .ToArray();

            if (ctors.Length == 0)
            {
                result = null; // allows null as ok(not exists explicitly declared constructor == has implictly empty ctor)
                diagnosticDescriptor = null;
                return true;
            }

            if (ctors.Length == 1)
            {
                result = ctors[0];
                diagnosticDescriptor = null;
                return true;
            }

            var ctorWithAttrs = ctors.Where(ctor =>
            {
                return ctor.GetAttributes()
                    .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, injectAttributeSymbol));
            }).ToArray();

            if (ctorWithAttrs.Length == 0)
            {
                result = null;
                diagnosticDescriptor = DiagnosticDescriptors.MultipleCtorWithoutAttribute;
                return false;
            }
            if (ctorWithAttrs.Length == 1)
            {
                result = ctorWithAttrs[0]; // ok
                diagnosticDescriptor = null;
                return true;
            }

            result = null;
            diagnosticDescriptor = DiagnosticDescriptors.MultipleCtorAttribute;
            return false;
        }
    }
}