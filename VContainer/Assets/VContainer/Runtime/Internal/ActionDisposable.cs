using System;

namespace VContainer.Internal
{
    struct ActionDisposable : IDisposable
    {
        readonly Action<IObjectResolver> callback;
        readonly IObjectResolver container;

        public ActionDisposable(Action<IObjectResolver> callback, IObjectResolver container)
        {
            this.callback = callback;
            this.container = container;
        }

        public void Dispose()
        {
            callback.Invoke(container);
        }
    }
}