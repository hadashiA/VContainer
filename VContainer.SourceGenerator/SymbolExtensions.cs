using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace VContainer.SourceGenerator
{
    public static class SymbolExtensions
    {
        public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol, bool withoutOverride = true)
        {
            // Iterate Parent -> Derived
            if (symbol.BaseType != null)
            {
                foreach (var item in GetAllMembers(symbol.BaseType))
                {
                    // override item already iterated in parent type
                    if (!withoutOverride || !item.IsOverride)
                    {
                        yield return item;
                    }
                }
            }

            foreach (var item in symbol.GetMembers())
            {
                if (!withoutOverride || !item.IsOverride)
                {
                    yield return item;
                }
            }
        }

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
