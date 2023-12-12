using System;

namespace VContainer.Unity
{
    sealed class AsyncLoopItem : IPlayerLoopItem
    {
        readonly Action action;

        public AsyncLoopItem(Action action)
        {
            this.action = action;
        }
        
        public bool MoveNext()
        {
            action();
            return false;
        }
    }
}
