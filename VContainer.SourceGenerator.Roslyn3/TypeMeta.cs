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
        public bool ExplicitInjectable { get; }

        public IReadOnlyList<IMethodSymbol> Constructors { get; }
        public IReadOnlyList<IMethodSymbol> ExplictInjectConstructors { get; }
        public IReadOnlyList<IFieldSymbol> InjectFields { get; }
        public IReadOnlyList<IPropertySymbol> InjectProperties { get; }
        public IReadOnlyList<IMethodSymbol> InjectMethods { get; }

        public bool IsGenerics => Symbol.Arity > 0;

        ReferenceSymbols references;

        public TypeMeta(TypeDeclarationSyntax syntax, INamedTypeSymbol symbol, ReferenceSymbols references)
        {
            Syntax = syntax;
            Symbol = symbol;
            this.references = references;

            TypeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            FullTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            Constructors = GetConstructors();
            ExplictInjectConstructors = GetExplicitInjectConstructors();
            InjectFields = GetInjectFields();
            InjectProperties = GetInjectProperties();
            InjectMethods = GetInjectMethods();

            ExplicitInjectable = ExplictInjectConstructors.Count > 0 ||
                                 InjectFields.Count > 0 ||
                                 InjectProperties.Count > 0 ||
                                 InjectMethods.Count > 0;
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

        IReadOnlyList<IMethodSymbol> GetExplicitInjectConstructors()
        {
            return Constructors.Where(ctor =>
            {
                return ctor.GetAttributes().Any(attr =>
                    SymbolEqualityComparer.Default.Equals(attr.AttributeClass, references.VContainerInjectAttribute));
            }).ToArray();
        }

        IReadOnlyList<IMethodSymbol> GetConstructors()
        {
            return Symbol.InstanceConstructors
                .Where(x => !x.IsImplicitlyDeclared) // remove empty ctor(struct always generate it), record's clone ctor
                .ToArray();
        }

        IReadOnlyList<IFieldSymbol> GetInjectFields()
        {
            return Symbol.GetAllMembers()
                .OfType<IFieldSymbol>()
                .Where(x => x.ContainsAttribute(references.VContainerInjectAttribute))
                .DistinctBy(x => x.Name)
                .ToArray();
        }

        IReadOnlyList<IPropertySymbol> GetInjectProperties()
        {
            return Symbol.GetAllMembers()
                .OfType<IPropertySymbol>()
                .Where(x => x.ContainsAttribute(references.VContainerInjectAttribute))
                .DistinctBy(x => x.Name)
                .ToArray();
        }

        IReadOnlyList<IMethodSymbol> GetInjectMethods()
        {
            return Symbol.GetAllMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x.MethodKind == MethodKind.Ordinary &&
                            x.ContainsAttribute(references.VContainerInjectAttribute))
                .ToArray();
        }

        public bool IsNested()
        {
            return Syntax.Parent is TypeDeclarationSyntax;
        }
    }
}
