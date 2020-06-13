using System;
using System.Collections.Generic;

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
        readonly object waitingAndStateGate = new object();

        bool running;

        public void Dispatch(IPlayerLoopItem item)
        {
            lock (waitingAndStateGate)
            {
                if (running)
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
            lock (runningGate)
            lock (waitingAndStateGate)
            {
                running = true;
                while (waitingQueue.Count > 0)
                {
                    var waitingItem = waitingQueue.Dequeue();
                    runningQueue.Enqueue(waitingItem);
                }
            }

            var item = default(IPlayerLoopItem);
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
                    lock (waitingAndStateGate)
                    {
                        waitingQueue.Enqueue(item);
                    }
                }

                lock (runningGate)
                {
                    item = runningQueue.Count > 0 ? runningQueue.Dequeue() : null;
                }
            }

            lock (waitingAndStateGate)
            {
                running = false;
            }
        }
    }
}
