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
            lock (waitingAndStateGate)
            {
                running = true;
            }

            var hasNext = false;
            lock (runningGate)
            {
                hasNext = runningQueue.Count > 0;
            }

            while (hasNext)
            {
                IPlayerLoopItem item;
                lock (runningGate)
                {
                    item = runningQueue.Dequeue();
                }

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
                    hasNext = runningQueue.Count > 0;
                }
            }

            lock (runningGate)
            lock (waitingAndStateGate)
            {
                running = false;
                while (waitingQueue.Count > 0)
                {
                    var item = waitingQueue.Dequeue();
                    runningQueue.Enqueue(item);
                }
            }
        }
    }
}
