using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    sealed class FixedTypeObjectKeyHashtable<TValue>
    {
        readonly struct HashEntry
        {
            public readonly Type Type;
            public readonly object Identifier;
            public readonly TValue Value;

            public HashEntry(Type type, object identifier, TValue value)
            {
                Type = type;
                Identifier = identifier;
                Value = value;
            }
        }

        readonly HashEntry[][] table;
        readonly int indexFor;

        public FixedTypeObjectKeyHashtable(KeyValuePair<(Type, object), TValue>[] values, float loadFactor = 0.75f)
        {
            var initialCapacity = (int)(values.Length / loadFactor);

            // make power of 2
            var capacity = 1;
            while (capacity < initialCapacity)
            {
                capacity <<= 1;
            }

            table = new HashEntry[capacity][];
            indexFor = table.Length - 1;

            foreach (var item in values)
            {
                var hash = GetHashCode(item.Key.Item1, item.Key.Item2);
                var array = table[hash & indexFor];
                if (array == null)
                {
                    array = new HashEntry[1];
                    array[0] = new HashEntry(item.Key.Item1, item.Key.Item2, item.Value);
                }
                else
                {
                    var newArray = new HashEntry[array.Length + 1];
                    Array.Copy(array, newArray, array.Length);
                    array = newArray;
                    array[^1] = new HashEntry(item.Key.Item1, item.Key.Item2, item.Value);
                }

                table[hash & indexFor] = array;
            }
        }

        private int GetHashCode(Type type, object identifier)
        {
            // Combine the hash codes of Type and identifier
            var typeHash = RuntimeHelpers.GetHashCode(type);
            var identifierHash = identifier?.GetHashCode() ?? 0;
            return (typeHash * 397) ^ identifierHash; // FNV-style combination
        }

        public bool TryGet(Type type, object identifier, out TValue value)
        {
            var hashCode = GetHashCode(type, identifier);
            var buckets = table[hashCode & indexFor];

            if (buckets == null) goto END;

            if (buckets[0].Type == type && Equals(buckets[0].Identifier, identifier))
            {
                value = buckets[0].Value;
                return true;
            }

            for (var i = 1; i < buckets.Length; i++)
            {
                if (buckets[i].Type != type || !Equals(buckets[i].Identifier, identifier))
                {
                    continue;
                }
                
                value = buckets[i].Value;
                return true;
            }

            END:
            value = default;
            return false;
        }
    }
}