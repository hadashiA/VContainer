using System;
using System.Linq;
using System.Reflection;

namespace Reflex.Scripts.Utilities
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Returns the type name. If this is a generic type, appends
        /// the list of generic type arguments between angle brackets.
        /// (Does not account for embedded / inner generic arguments.)
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        internal static string GetFormattedName(this Type type)
        {
            if (type.IsGenericType)
            {
                string genericArguments = type.GetGenericArguments()
                    .Select(x => x.Name)
                    .Aggregate((x1, x2) => $"{x1}, {x2}");
                return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
                       + $"<{genericArguments}>";
            }

            return type.Name;
        }

        internal static bool TryGetConstructors(this Type type, out ConstructorInfo[] constructors)
        {
            constructors = type.GetConstructors();
            return constructors.Length > 0;
        }
    }
}