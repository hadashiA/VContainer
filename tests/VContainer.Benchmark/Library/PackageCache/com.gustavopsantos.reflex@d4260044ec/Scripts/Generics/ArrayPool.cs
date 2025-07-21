using Reflex.Scripts.Utilities;
using System.Collections.Generic;

namespace Reflex
{
    internal class ArrayPool<T>
    {
        internal static readonly ArrayPool<T> Shared = new ArrayPool<T>();

        private readonly Dictionary<int, Queue<T[]>> _registry = new Dictionary<int, Queue<T[]>>();

        internal T[] Rent(int size)
        {
            if (_registry.TryGetValue(size, out var queue) && queue.TryDequeue(out var array))
            {
                return array;
            }

            return new T[size];
        }

        internal void Return(T[] array)
        {
            if (_registry.TryGetValue(array.Length, out var queue))
            {
                queue.Enqueue(array);
            }
            else
            {
                _registry.Add(array.Length, new Queue<T[]>().With(array));
            }
        }
    }
}