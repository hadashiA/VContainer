using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VContainer.SourceGenerator
{
    class TypeMeta
    {
        public TypeDeclarationSyntax Syntax { get; }
        public INamedTypeSymbol Symbol { get; }
        public string TypeName { get; }
        public string FullTypeName { get; }

        ReferenceSymbols references;

        public TypeMeta(TypeDeclarationSyntax syntax, INamedTypeSymbol symbol, ReferenceSymbols references)
        {
            Syntax = syntax;
            Symbol = symbol;
            this.references = references;

            TypeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            FullTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
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

        public IMethodSymbol? GetInjectConstructor(out DiagnosticDescriptor? invalid)
        {
            var ctors = Symbol.InstanceConstructors
                .Where(x => !x.IsImplicitlyDeclared) // remove empty ctor(struct always generate it), record's clone ctor
                .ToArray();

            if (ctors.Length == 0)
            {
                invalid = null;
                return null;
            }

            if (ctors.Length <= 1)
            {
                invalid = null;
                return ctors[0];
            }

            var ctorWithAttrs = ctors.Where(ctor =>
            {
                return ctor.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, references.VContainerInjectAttribute));
            }).ToArray();

            if (ctorWithAttrs.Length == 0)
            {
                invalid = DiagnosticDescriptors.MultipleCtorWithoutAttribute;
                return null;
            }

            if (ctorWithAttrs.Length == 1)
            {
                invalid = null;
                return ctorWithAttrs[0]; // ok
            }
            invalid = DiagnosticDescriptors.MultipleCtorAttribute;
            return null;
        }


        public IReadOnlyList<IFieldSymbol> GetInjectFields()
        {
            return Symbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(x => x.ContainsAttribute(references.VContainerInjectAttribute))
                .ToArray();
        }

        public IReadOnlyList<IPropertySymbol> GetInjectProperties()
        {
            return Symbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => x.ContainsAttribute(references.VContainerInjectAttribute))
                .ToArray();

        }

        public IReadOnlyList<IMethodSymbol> GetInjectMethods()
        {
            return Symbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x.ContainsAttribute(references.VContainerInjectAttribute))
                .ToArray();
        }
    }
}
