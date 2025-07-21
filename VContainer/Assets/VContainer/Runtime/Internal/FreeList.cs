using System;
using System.Collections.Generic;
#if UNITY_2021_3_OR_NEWER
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace VContainer.Internal
{
    class FreeList<T> where T : class
    {
        public bool IsDisposed => lastIndex == -2;
        public int Length => lastIndex + 1;

        readonly object gate = new object();
        T[] values;
        int lastIndex = -1;

        public FreeList(int initialCapacity)
        {
            values = new T[initialCapacity];
        }

#if NETSTANDARD2_1
        public ReadOnlySpan<T> AsSpan()
        {
            if (lastIndex < 0)
            {
                return ReadOnlySpan<T>.Empty;
            }
            return values.AsSpan(0, lastIndex + 1);
        }
#endif

        public T this[int index] => values[index];

        public void Add(T item)
        {
            lock (gate)
            {
                CheckDispose();

                // try find blank
                var index = FindNullIndex(values);
                if (index == -1)
                {
                    // full, 1, 4, 6,...resize(x1.5)
                    var len = values.Length;
                    var newValues = new T[len + len / 2];
                    Array.Copy(values, newValues, len);
                    values = newValues;
                    index = len;
                }

                values[index] = item;
                if (lastIndex < index)
                {
                    lastIndex = index;
                }
            }
        }

        public void RemoveAt(int index)
        {
            lock (gate)
            {
                if (index < values.Length)
                {
                    ref var v = ref values[index];
                    if (v == null) throw new KeyNotFoundException($"key index {index} is not found.");

                    v = null;
                    if (index == lastIndex)
                    {
                        lastIndex = FindLastNonNullIndex(values, index);
                    }
                }
            }
        }

        public bool Remove(T value)
        {
            lock (gate)
            {
                if (lastIndex < 0) return false;

                var index = -1;
                for (var i = 0; i < values.Length; i++)
                {
                    if (values[i] == value)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            lock (gate)
            {
                if (lastIndex > 0)
                {
                    Array.Clear(values, 0, lastIndex + 1);
                    lastIndex = -1;
                }
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                lastIndex = -2; // -2 is disposed.
            }
        }

        void CheckDispose()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

#if UNITY_2021_3_OR_NEWER
        static unsafe int FindNullIndex(T[] target)
        {
            ref var head = ref UnsafeUtility.As<T, IntPtr>(ref MemoryMarshal.GetReference(target.AsSpan()));
            fixed (void* p = &head)
            {
                var span = new ReadOnlySpan<IntPtr>(p, target.Length);

#if NETSTANDARD2_1
                return span.IndexOf(IntPtr.Zero);
#else
                for (int i = 0; i < span.Length; i++)
                {
                    if (span[i] == IntPtr.Zero) return i;
                }
                return -1;
#endif
            }
        }

        static unsafe int FindLastNonNullIndex(T[] target, int lastIndex)
        {
            ref var head = ref UnsafeUtility.As<T, IntPtr>(ref MemoryMarshal.GetReference(target.AsSpan()));
            fixed (void* p = &head)
            {
                var span = new ReadOnlySpan<IntPtr>(p, lastIndex); // without lastIndexed value.

                for (var i = span.Length - 1; i >= 0; i--)
                {
                    if (span[i] != IntPtr.Zero) return i;
                }

                return -1;
            }
        }
#else
        static int FindNullIndex(T[] target)
        {
            for (var i = 0; i < target.Length; i++)
            {
                if (target[i] == null) return i;
            }
            return -1;
        }

        static int FindLastNonNullIndex(T[] target, int lastIndex)
        {
            for (var i = lastIndex; i >= 0; i--)
            {
                if (target[i] != null) return i;
            }
            return -1;
        }
#endif
    }
}
