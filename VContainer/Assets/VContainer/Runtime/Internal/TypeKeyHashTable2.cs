using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    sealed class TypeKeyHashTable2<TValue>
    {
        struct Bucket
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint DistAndFingerPrintFromHash(int hash)
            {
                return DistOne | ((uint)hash & FingerPrintMask);
            }

            public const uint DistOne = 0x00000100;
            public const uint FingerPrintMask = 0x000000FF;

            /// <summary>
            /// upper 3 bytes: dist (distance of , also known PSL (probe sequence length))
            /// lower 1 bytes: fingerprint (lower 1 byte of hash code)
            /// </summary>
            public uint DistAndFingerPrint;

            /// <summary>
            /// The index that point to the location where value is actually stored.
            /// </summary>
            public int EntryIndex;
        }

        readonly Bucket[] buckets;
        readonly KeyValuePair<Type, TValue>[] entries;
        readonly int indexFor;

        int insertedEntryLength;

        public TypeKeyHashTable2(KeyValuePair<Type, TValue>[] values, float loadFactor = 0.75f)
        {
            var initialCapacity = (int)(values.Length / loadFactor);

            // make power of 2(and use mask)
            // see: Hashing https://en.wikipedia.org/wiki/Hash_table
            var capacity = 1;
            while (capacity < initialCapacity)
            {
                capacity <<= 1;
            }

            buckets = new Bucket[capacity];
            entries = new KeyValuePair<Type, TValue>[values.Length];
            indexFor = buckets.Length - 1;

            var entryIndex = 0;
            foreach (var x in values)
            {
                Insert(x, entryIndex++);
            }
        }

        public bool TryGet(Type key, out TValue value)
        {
            var hash = RuntimeHelpers.GetHashCode(key);
            var distAndFingerPrint = Bucket.DistAndFingerPrintFromHash(hash);
            var bucketIndex = hash & indexFor;
            var bucket = buckets[bucketIndex];

            while (true)
            {
                if (distAndFingerPrint == bucket.DistAndFingerPrint)
                {
                    // compare key
                    var entry = entries[bucket.EntryIndex];
                    if (key == entry.Key)
                    {
                        value = entry.Value;
                        return true;
                    }
                }
                else if (distAndFingerPrint > bucket.DistAndFingerPrint)
                {
                    // not found
                    value = default;
                    return false;
                }
                distAndFingerPrint += Bucket.DistOne;
                bucketIndex = NextBucketIndex(bucketIndex);
                bucket = buckets[bucketIndex];
            }

        }

        void Insert(KeyValuePair<Type, TValue> entry, int entryIndex)
        {
            var hash = RuntimeHelpers.GetHashCode(entry.Key);
            var distAndFingerPrint = Bucket.DistAndFingerPrintFromHash(hash);
            var bucketIndex = hash & indexFor;

            // robin food hashing
            while (distAndFingerPrint <= buckets[bucketIndex].DistAndFingerPrint)
            {
                // key already exists
                if (distAndFingerPrint == buckets[bucketIndex].DistAndFingerPrint &&
                    entry.Key == entries[buckets[bucketIndex].EntryIndex].Key)
                {
                    throw new InvalidOperationException($"The key already exists: {entry.Key}");
                }

                //
                bucketIndex = NextBucketIndex(bucketIndex);
                distAndFingerPrint += Bucket.DistOne; //
            }

            entries[entryIndex] = entry;
            SetBucketAt(bucketIndex, new Bucket
            {
                DistAndFingerPrint = distAndFingerPrint,
                EntryIndex = entryIndex
            });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="i"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetBucketAt(int i, Bucket bucket)
        {
            while (buckets[i].DistAndFingerPrint != 0)
            {
                // swap
                (buckets[i], bucket) = (bucket, buckets[i]);
                bucket.DistAndFingerPrint += Bucket.DistOne;
                i = NextBucketIndex(i);
            }
            buckets[i] = bucket;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int NextBucketIndex(int i)
        {
            return i + 1 >= buckets.Length ? 0 : i + 1;
        }
    }
}