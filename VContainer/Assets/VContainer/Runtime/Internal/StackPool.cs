using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    public sealed class StackPool<T>
    {
        public static readonly StackPool<T> Default = new StackPool<T>();

        readonly object syncRoot = new object();

        int tail;
        Stack<T>[] buckets;

        public StackPool(int initialCapacity = 16)
        {
            buckets = new Stack<T>[initialCapacity];
        }

        public Stack<T> Rent()
        {
            lock (syncRoot)
            {
                if (tail >= buckets.Length)
                {
                    Array.Resize(ref buckets, buckets.Length * 2);
                }

                return buckets[tail++] ?? new Stack<T>();
            }
        }

        public void Return(Stack<T> stack)
        {
            stack.Clear();
            lock (syncRoot)
            {
                if (tail > 0)
                {
                    buckets[--tail] = stack;
                }
            }
        }
    }
}