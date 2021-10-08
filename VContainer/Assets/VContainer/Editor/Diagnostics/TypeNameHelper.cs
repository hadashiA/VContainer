using System;
using System.Collections.Generic;
using System.Linq;

namespace VContainer.Editor.Diagnostics
{
    static class TypeNameHelper
    {
        static readonly IReadOnlyDictionary<Type, string> TypeAlias = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(void), "void" }
        };

        public static string GetTypeAlias(Type type)
        {
            if (TypeAlias.TryGetValue(type, out var alias))
            {
                return alias;
            }
            if (type.IsGenericType)
            {
                var typeParameters = type.GetGenericArguments().Select(GetTypeAlias);
                var typeNameBase = type.Name.Split('`')[0];
                return $"{typeNameBase}<{string.Join(",", typeParameters)}>";
            }
            return type.Name;
        }
    }
}