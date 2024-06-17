using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VContainer.SourceGenerator;

static class Analyzer
{
    public static TypeMeta? AnalyzeTypeSymbol(
        ITypeSymbol symbol,
        ReferenceSymbols referenceSymbols,
        TypeDeclarationSyntax? syntax = null,
        CancellationToken cancellation = default)
    {
        if (symbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }
        if (typeSymbol.TypeKind is TypeKind.Interface or TypeKind.Struct or TypeKind.Enum)
        {
            return null;
        }
        if (typeSymbol.IsAbstract || typeSymbol.IsStatic || typeSymbol.SpecialType != SpecialType.None)
        {
            return null;
        }

        var moduleName = typeSymbol.ContainingModule.Name;
        if (moduleName is "VContainer" or "VContainer.Standalone" ||
            moduleName.StartsWith("Unity.") ||
            moduleName.StartsWith("UnityEngine.") ||
            moduleName.StartsWith("System."))
        {
            return null;
        }

        foreach (var baseTypeSymbol in typeSymbol.GetAllBaseTypes())
        {
            if (SymbolEqualityComparer.Default.Equals(baseTypeSymbol, referenceSymbols.AttributeBase))
            {
                return null;
            }
        }

        foreach (var attributeData in typeSymbol.GetAttributes())
        {
            if (attributeData.AttributeClass != null)
            {
                // Ignore
                if (SymbolEqualityComparer.Default.Equals(
                        attributeData.AttributeClass,
                        referenceSymbols.VContainerInjectIgnoreAttribute))
                {
                    return null;
                }
            }

        }
        return new TypeMeta(typeSymbol, referenceSymbols, syntax);
    }
}

record struct TypeDeclarationCandidate(TypeDeclarationSyntax Syntax, SemanticModel SemanticModel)
{
    public TypeMeta? Analyze(ReferenceSymbols referenceSymbols, CancellationToken cancellation = default)
    {
        var symbol = SemanticModel.GetDeclaredSymbol(Syntax);
        if (symbol is ITypeSymbol typeSymbol)
            return Analyzer.AnalyzeTypeSymbol(typeSymbol, referenceSymbols, Syntax);
        return null;
    }
}

record struct RegisterInvocationCandidate(InvocationExpressionSyntax Syntax, SemanticModel SemanticModel)
{
    public IEnumerable<TypeMeta> Analyze(ReferenceSymbols referenceSymbols, CancellationToken cancellation = default)
    {
        var symbol = SemanticModel.GetSymbolInfo(Syntax).Symbol;
        if (symbol is IMethodSymbol methodSymbol)
        {
            var typeSymbol = methodSymbol.ReceiverType;
            if (SymbolEqualityComparer.Default.Equals(typeSymbol, referenceSymbols.ContainerBuilderInterface))
            {
                if (methodSymbol.Arity > 0)
                {
                    foreach (var typeArgument in methodSymbol.TypeArguments)
                    {
                        var typeMeta = Analyzer.AnalyzeTypeSymbol(typeArgument, referenceSymbols, cancellation: cancellation);
                        if (typeMeta != null)
                        {
                            yield return typeMeta;
                        }
                    }
                }
                else
                {
                    foreach (var p in methodSymbol.Parameters)
                    {
                        var typeMeta = Analyzer.AnalyzeTypeSymbol(p.Type, referenceSymbols, cancellation: cancellation);
                        if (typeMeta != null)
                        {
                            yield return typeMeta;
                        }
                    }
                }
            }
        }
    }
}