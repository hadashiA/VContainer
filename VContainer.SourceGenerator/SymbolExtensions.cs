using System.Linq;
using Microsoft.CodeAnalysis;

namespace VContainer.SourceGenerator
{
    public static class SymbolExtensions
    {
        public static bool ContainsAttribute(this ISymbol symbol, INamedTypeSymbol attribtue)
        {
            return symbol.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribtue));
        }

        public static bool CanBeCallFromInternal(this ISymbol symbol)
        {
            return symbol.DeclaredAccessibility >= Accessibility.Internal;
        }
    }
}
