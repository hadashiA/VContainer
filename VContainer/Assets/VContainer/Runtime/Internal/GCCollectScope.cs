using System;

namespace VContainer.Internal
{
    public sealed class GCCollectScope : IDisposable
    {
        public void Dispose()
        {
            GC.Collect();
        }
    }
}
