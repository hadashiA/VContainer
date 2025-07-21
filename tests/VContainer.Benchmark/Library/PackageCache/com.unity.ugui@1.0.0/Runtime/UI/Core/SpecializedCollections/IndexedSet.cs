using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Collections
{
    internal class IndexedSet<T> : IList<T>
    {
        //This is a container that gives:
        //  - Unique items
        //  - Fast random removal
        //  - Fast unique inclusion to the end
        //  - Sequential access
        //  - Possibility to have disabled items registered
        //Downsides:
        //  - Uses more memory
        //  - Ordering is not persistent
        //  - Not Serialization Friendly.

        //We use a Dictionary to speed up list lookup, this makes it cheaper to guarantee no duplicates (set)
        //When removing we move the last item to the removed item position, this way we only need to update the index cache of a single item. (fast removal)
        //Order of the elements is not guaranteed. A removal will change the order of the items.

        readonly List<T> m_List = new List<T>();
        Dictionary<T, int> m_Dictionary = new Dictionary<T, int>();
        int m_EnabledObjectCount = 0;

        public void Add(T item)
        {
            Add(item, true);
        }

        public void Add(T item, bool isActive)
        {
            m_List.Add(item);
            m_Dictionary.Add(item, m_List.Count - 1);
            if (isActive)
                EnableItem(item);
        }

        public bool AddUnique(T item, bool isActive = true)
        {
            if (m_Dictionary.ContainsKey(item))
            {
                if (isActive)
                    EnableItem(item);
                else
                    DisableItem(item);
                return false;
            }

            Add(item, isActive);

            return true;
        }

        public bool EnableItem(T item)
        {
            if (!m_Dictionary.TryGetValue(item, out int index))
                return false;

            if (index < m_EnabledObjectCount)
                return true;

            if (index > m_EnabledObjectCount)
                Swap(m_EnabledObjectCount, index);

            m_EnabledObjectCount++;
            return true;
        }

        public bool DisableItem(T item)
        {
            if (!m_Dictionary.TryGetValue(item, out int index))
                return false;

            if (index >= m_EnabledObjectCount)
                return true;

            if (index < m_EnabledObjectCount - 1)
                Swap(index, m_EnabledObjectCount - 1);

            m_EnabledObjectCount--;
            return true;
        }

        public bool Remove(T item)
        {
            int index = -1;
            if (!m_Dictionary.TryGetValue(item, out index))
                return false;

            RemoveAt(index);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            m_List.Clear();
            m_Dictionary.Clear();
            m_EnabledObjectCount = 0;
        }

        public bool Contains(T item)
        {
            return m_Dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_List.CopyTo(array, arrayIndex);
        }

        public int Count { get { return m_EnabledObjectCount; } }
        public int Capacity { get { return m_List.Count; } }
        public bool IsReadOnly { get { return false; } }
        public int IndexOf(T item)
        {
            int index = -1;
            if (m_Dictionary.TryGetValue(item, out index))
                return index;
            return -1;
        }

        public void Insert(int index, T item)
        {
            //We could support this, but the semantics would be weird. Order is not guaranteed..
            throw new NotSupportedException("Random Insertion is semantically invalid, since this structure does not guarantee ordering.");
        }

        public void RemoveAt(int index)
        {
            T item = m_List[index];
            if (index == m_List.Count - 1)
            {
                if (m_EnabledObjectCount == m_List.Count)
                    m_EnabledObjectCount--;

                m_List.RemoveAt(index);
            }
            else
            {
                int replaceItemIndex = m_List.Count - 1;
                if (index < m_EnabledObjectCount - 1)
                {
                    Swap(--m_EnabledObjectCount, index);
                    index = m_EnabledObjectCount;
                }
                else if (index == m_EnabledObjectCount - 1)
                {
                    m_EnabledObjectCount--;
                }

                Swap(replaceItemIndex, index);
                m_List.RemoveAt(replaceItemIndex);
            }
            m_Dictionary.Remove(item);
        }

        private void Swap(int index1, int index2)
        {
            if (index1 == index2)
                return;
            T item1 = m_List[index1];
            T item2 = m_List[index2];
            m_List[index1] = item2;
            m_List[index2] = item1;
            m_Dictionary[item2] = index1;
            m_Dictionary[item1] = index2;
        }

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)m_EnabledObjectCount)
                    throw new IndexOutOfRangeException();
                return m_List[index];
            }
            set
            {
                //Order in the list should not be set manually since the order is not guaranteed.
                //The item will be activated or not according to the old item's state.
                T item = m_List[index];
                m_Dictionary.Remove(item);
                m_List[index] = value;
                m_Dictionary.Add(value, index);
            }
        }

        public void RemoveAll(Predicate<T> match)
        {
            //I guess this could be optmized by instead of removing the items from the list immediatly,
            //We move them to the end, and then remove all in one go.
            //But I don't think this is going to be the bottleneck, so leaving as is for now.
            int i = 0;
            while (i < m_List.Count)
            {
                T item = m_List[i];
                if (match(item))
                    Remove(item);
                else
                    i++;
            }
        }

        //Sorts the internal list, this makes the exposed index accessor sorted as well.
        //But note that any insertion or deletion, can unorder the collection again.
        public void Sort(Comparison<T> sortLayoutFunction)
        {
            //There might be better ways to sort and keep the dictionary index up to date.
            m_List.Sort(sortLayoutFunction);
            //Rebuild the dictionary index.
            for (int i = 0; i < m_List.Count; ++i)
            {
                T item = m_List[i];
                m_Dictionary[item] = i;
            }
        }
    }
}
