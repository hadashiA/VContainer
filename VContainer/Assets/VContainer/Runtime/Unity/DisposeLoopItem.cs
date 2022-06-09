using System;

namespace VContainer.Unity
{
    sealed class DisposeLoopItem : IPlayerLoopItem
    {
        readonly IDisposable disposable;

        public DisposeLoopItem(IDisposable disposable)
        {
            this.disposable = disposable;
        }

        public bool MoveNext()
        {
            disposable.Dispose();
            return false;
        }
    }
}
