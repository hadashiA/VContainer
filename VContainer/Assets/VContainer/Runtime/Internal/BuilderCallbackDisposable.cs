using System;

namespace VContainer.Internal
{
    class BuilderCallbackDisposable : IDisposable
    {
        public event Action<IObjectResolver> Disposing;
        readonly IObjectResolver container;

        [Inject]
        public BuilderCallbackDisposable(IObjectResolver container)
        {
            this.container = container;
        }

        public void Dispose()
        {
            if (Disposing != null) Disposing.Invoke(container);
        }
    }
}