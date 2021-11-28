using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    // http://neue.cc/2017/07/11_555.html
    sealed class FixedTypeKeyHashtable<TValue>
    {
        readonly struct HashEntry
        {
            public readonly Type Type;
            public readonly TValue Value;

            public HashEntry(Type key, TValue value)
            {
                Type = key;
                Value = value;
            }
        }

        readonly HashEntry[][] table;
        readonly int indexFor;

        public FixedTypeKeyHashtable(KeyValuePair<Type, TValue>[] values, float loadFactor = 0.75f)
        {
            var initialCapacity = (int)((float)values.Length / loadFactor);

            // make power of 2(and use mask)
            // see: Hashing https://en.wikipedia.org/wiki/Hash_table
            var capacity = 1;
            while (capacity < initialCapacity)
            {
                capacity <<= 1;
            }

            table = new HashEntry[(int)capacity][];
            indexFor = table.Length - 1;

            foreach (var item in values)
            {
                var hash = RuntimeHelpers.GetHashCode(item.Key);
                var array = table[hash & indexFor];
                if (array == null)
                {
                    array = new HashEntry[1];
                    array[0] = new HashEntry(item.Key, item.Value);
                }
                else
                {
                    var newArray = new HashEntry[array.Length + 1];
                    Array.Copy(array, newArray, array.Length);
                    array = newArray;
                    array[array.Length - 1] = new HashEntry(item.Key, item.Value);
                }

                table[hash & indexFor] = array;
            }
        }

        public TValue Get(Type type)
        {
            var hashCode = RuntimeHelpers.GetHashCode(type);
            var buckets = table[hashCode & indexFor];

            if (buckets == null) goto ERROR;

            if (buckets[0].Type == type)
            {
                return buckets[0].Value;
            }

            for (int i = 1; i < buckets.Length; i++)
            {
                if (buckets[i].Type == type)
                {
                    return buckets[i].Value;
                }
            }

            ERROR:
            throw new KeyNotFoundException("Type was not dound, Type: " + type.FullName);
        }

        public bool TryGet(Type type, out TValue value)
        {
            var hashCode = RuntimeHelpers.GetHashCode(type);
            var buckets = table[hashCode & indexFor];

            if (buckets == null) goto END;

            if (buckets[0].Type == type)
            {
                value = buckets[0].Value;
                return true;
            }

            for (int i = 1; i < buckets.Length; i++)
            {
                if (buckets[i].Type == type)
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