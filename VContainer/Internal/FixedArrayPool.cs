using System.Collections.Concurrent;

namespace VContainer.Internal
{
    public sealed class FixedArrayPool<T>
    {
        public static readonly T[] EmptyArray = new T[0];
        public static readonly FixedArrayPool<T> Shared8 = new FixedArrayPool<T>(8);

        readonly ConcurrentQueue<T[]>[] buckets;

        public FixedArrayPool(int maxLength)
        {
            buckets = new ConcurrentQueue<T[]>[maxLength];
            for (var i = 0; i < maxLength; i++)
            {
                buckets[i] = new ConcurrentQueue<T[]>();
            }
        }

        public T[] Rent(int length)
        {
            if (length <= 0)
                return EmptyArray;

            if (length > buckets.Length)
                return new T[length]; // Not supported

            var q = buckets[length - 1];
            if (q.TryDequeue(out var array))
            {
                return array;
            }
            return new T[length];
        }

        public void Return(T[] array)
        {
            if (array.Length <= 0 || array.Length > buckets.Length)
                return;

            var q = buckets[array.Length - 1];
            q.Enqueue(array);
        }
    }
}
