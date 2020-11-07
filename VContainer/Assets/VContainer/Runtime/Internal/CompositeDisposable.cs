using System;
using System.Collections.Generic;

namespace VContainer.Internal
{
    sealed class CompositeDisposable : IDisposable
    {
        readonly Stack<IDisposable> disposables = new Stack<IDisposable>();

        bool disposed;

        public void Dispose()
        {
            lock (disposables)
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
            lock (disposables)
            {
                disposables.Push(disposable);
            }
        }
    }
}