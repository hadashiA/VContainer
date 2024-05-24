using System;
using System.Linq;
using System.Collections.Generic;

namespace Reflex.Scripts.Utilities
{
    internal static class IEnumerableExtensions
    {
        internal static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.OrderByDescending(selector).First();
        }
        
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source) action(element);
        }
    }
}