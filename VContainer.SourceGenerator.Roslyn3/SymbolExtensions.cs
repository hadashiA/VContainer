using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static IEnumerable<INamedTypeSymbol> GetAllBaseTypes(this INamedTypeSymbol symbol)
        {
            var t = symbol.BaseType;
            while (t != null)
            {
                yield return t;
                t = t.BaseType;
            }
        }

        public static bool CanBeCallFromInternal(this ISymbol symbol)
        {
            return symbol.DeclaredAccessibility >= Accessibility.Internal;
        }

        public static string GetClassDeclarationName(this INamedTypeSymbol symbol)
        {
            if (symbol.TypeArguments.Length == 0)
            {
                return symbol.Name;
            }

            var sb = new StringBuilder();

            sb.Append(symbol.Name);
            sb.Append('<');

            var first = true;
            foreach (var typeArg in symbol.TypeArguments)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                else
                {
                    first = false;
                }
                sb.Append(typeArg.Name);
            }

            sb.Append('>');

            return sb.ToString();
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => DistinctBy(source, keySelector, null);

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            return DistinctByIterator(source, keySelector, comparer);
        }

        static IEnumerable<TSource> DistinctByIterator<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            using IEnumerator<TSource> enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
            {
                var set = new HashSet<TKey>(comparer);
                do
                {
                    TSource element = enumerator.Current;
                    if (set.Add(keySelector(element)))
                    {
                        yield return element;
                    }
                }
                while (enumerator.MoveNext());
            }
        }
    }
}
