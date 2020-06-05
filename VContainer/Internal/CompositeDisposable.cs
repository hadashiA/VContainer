using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    sealed class CompositeDisposable : IDisposable
    {
        readonly object syncRoot = new object();
        readonly Stack<IDisposable> disposables = new Stack<IDisposable>();

        bool disposed;

        public void Dispose()
        {
            lock (syncRoot)
            {
                if (!disposed)
                {
                    while (disposables.Count > 0)
                    {
                        disposables.Pop().Dispose();
                    }
                    disposed = true;
                }
            }
        }

        public void Add(IDisposable disposable)
        {
            lock (syncRoot)
            {
                disposables.Push(disposable);
            }
        }
    }
}