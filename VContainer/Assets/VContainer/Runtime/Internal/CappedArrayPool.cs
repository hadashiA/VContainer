using System;

namespace VContainer.Internal
{
    sealed class CappedArrayPool<T>
    {
        internal const int InitialBucketSize = 4;

        public static readonly CappedArrayPool<T> Shared8Limit = new CappedArrayPool<T>(8);

        readonly T[][][] buckets;
        readonly object syncRoot = new object();
        readonly int[] tails;

        internal CappedArrayPool(int maxLength)
        {
            buckets = new T[maxLength][][];
            tails = new int[maxLength];
            for (var i = 0; i < maxLength; i++)
            {
                var arrayLength = i + 1;
                buckets[i] = new T[InitialBucketSize][];
                for (var j = 0; j < InitialBucketSize; j++)
                {
                    buckets[i][j] = new T[arrayLength];
                }
                tails[i] = 0;
            }
        }

        public T[] Rent(int length)
        {
            if (length <= 0)
                return Array.Empty<T>();

            if (length > buckets.Length)
                return new T[length]; // Not supported

            var i = length - 1;

            lock (syncRoot)
            {
                var bucket = buckets[i];
                var tail = tails[i];
                if (tail >= bucket.Length)
                {
                    Array.Resize(ref bucket, bucket.Length * 2);
                    buckets[i] = bucket;
                }

                if (bucket[tail] == null)
                {
                    bucket[tail] = new T[length];
                }

                var result = bucket[tail];
                tails[i] += 1;
                return result;
            }
        }

        public void Return(T[] array)
        {
            if (array.Length <= 0 || array.Length > buckets.Length)
                return;

            var i = array.Length - 1;
            lock (syncRoot)
            {
                Array.Clear(array, 0, array.Length);
                if (tails[i] > 0)
                    tails[i] -= 1;
            }
        }
    }
}
