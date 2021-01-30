using System;

namespace VContainer.Runtime.Unity
{
    sealed class StartupExceptionHandler
    {
        readonly Action<Exception> handler;

        public StartupExceptionHandler(Action<Exception> handler)
        {
            this.handler = handler;
        }

        public void Publish(Exception ex)
        {
            handler.Invoke(ex);
        }
    }
}
