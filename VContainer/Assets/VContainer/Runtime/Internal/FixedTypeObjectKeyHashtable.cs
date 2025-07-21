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
            public readonly object Key;
            public readonly TValue Value;

            public HashEntry(Type type, object key, TValue value)
            {
                Type = type;
                Key = key;
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
                    array[array.Length - 1] = new HashEntry(item.Key.Item1, item.Key.Item2, item.Value);
                }

                table[hash & indexFor] = array;
            }
        }

        private int GetHashCode(Type type, object key = null)
        {
            var typeHash = RuntimeHelpers.GetHashCode(type);
            
            if(key == null)
                return typeHash;
            
            // Combine the hash codes of Type and Key
            var keyHash = key.GetHashCode();
            return (typeHash * 397) ^ keyHash; // FNV-style combination
        }

        public bool TryGet(Type type, object key, out TValue value)
        {
            var hashCode = GetHashCode(type, key);
            var buckets = table[hashCode & indexFor];

            if (buckets == null) goto END;

            if (buckets[0].Type == type)
            {
                if (key == null || Equals(buckets[0].Key, key))
                {
                    value = buckets[0].Value;
                    return true;
                }
            }

            for (var i = 1; i < buckets.Length; i++)
            {
                if (buckets[i].Type != type)
                {
                    continue;
                }
                
                if (key == null || Equals(buckets[i].Key, key))
                {
                    value = buckets[i].Value;
                    return true;
                }
            }

            END:
            value = default;
            return false;
        }
    }
}

