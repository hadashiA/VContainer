using System;

namespace VContainer.Unity
{
    interface IPlayerLoopItem
    {
        bool MoveNext();
    }

    sealed class PlayerLoopRunner
    {
        const int InitialBufferSize = 16;

        readonly object syncRoot = new object();

        IPlayerLoopItem[] items = new IPlayerLoopItem[InitialBufferSize];
        IPlayerLoopItem[] runningBuffer = new IPlayerLoopItem[InitialBufferSize];

        public void Dispatch(IPlayerLoopItem item)
        {
            lock (syncRoot)
            {
                var length = items.Length;
                for (var i = 0; i < length; i++)
                {
                    if (items[i] is null)
                    {
                        items[i] = item;
                        return;
                    }
                }
                Array.Resize(ref items, length * 2);
                Array.Resize(ref runningBuffer, length * 2);
                items[length] = item;
            }
        }

        public void Run()
        {
            var length = 0;
            lock (syncRoot)
            {
                length = items.Length;
            }
            if (length == 0) return;

            lock (syncRoot)
            {
                // TODO:
                Array.Copy(items, 0, runningBuffer, 0, length);
            }

            for (var i = 0; i < length; i++)
            {
                var item = runningBuffer[i];
                var continuous = false;
                try
                {
                    continuous = item?.MoveNext() ?? false;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
                if (!continuous)
                {
                    // TODO:
                    lock (syncRoot)
                    {
                        items[i] = null;
                    }
                }
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                for (var i = 0; i < items.Length; i++)
                {
                    items[i] = null;
                }
            }
        }
    }
}
