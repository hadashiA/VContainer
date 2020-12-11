using System;
using System.Collections.Generic;
using System.Threading;

namespace VContainer.Unity
{
    interface IPlayerLoopItem
    {
        bool MoveNext();
    }

    sealed class PlayerLoopRunner
    {
        readonly Queue<IPlayerLoopItem> runningQueue = new Queue<IPlayerLoopItem>();
        readonly Queue<IPlayerLoopItem> waitingQueue = new Queue<IPlayerLoopItem>();

        readonly object runningGate = new object();
        readonly object waitingGate = new object();

        int running;

        public void Dispatch(IPlayerLoopItem item)
        {
            if (Interlocked.CompareExchange(ref running, 1, 1) == 1)
            {
                lock (waitingGate)
                {
                    waitingQueue.Enqueue(item);
                    return;
                }
            }

            lock (runningGate)
            {
                runningQueue.Enqueue(item);
            }
        }

        public void Run()
        {
            Interlocked.Exchange(ref running, 1);

            lock (runningGate)
            lock (waitingGate)
            {
                while (waitingQueue.Count > 0)
                {
                    var waitingItem = waitingQueue.Dequeue();
                    runningQueue.Enqueue(waitingItem);
                }
            }

            IPlayerLoopItem item;
            lock (runningGate)
            {
                item = runningQueue.Count > 0 ? runningQueue.Dequeue() : null;
            }

            while (item != null)
            {
                var continuous = false;
                try
                {
                    continuous = item.MoveNext();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }

                if (continuous)
                {
                    lock (waitingGate)
                    {
                        waitingQueue.Enqueue(item);
                    }
                }

                lock (runningGate)
                {
                    item = runningQueue.Count > 0 ? runningQueue.Dequeue() : null;
                }
            }

            Interlocked.Exchange(ref running, 0);
        }
    }
}
