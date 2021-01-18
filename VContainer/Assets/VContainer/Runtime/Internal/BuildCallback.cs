using System;

namespace VContainer.Internal
{
    interface IBuildCallback
    {
        void Call(IObjectResolver resolver);
    }

    class BuildCallback : IBuildCallback
    {
        readonly Action<IObjectResolver> callback;

        public BuildCallback(Action<IObjectResolver> callback)
        {
            this.callback = callback;
        }

        public void Call(IObjectResolver resolver)
        {
            callback.Invoke(resolver);
        }
    }

    class BuildCallbackWithState<T> : IBuildCallback
    {
        readonly T state;
        readonly Action<T, IObjectResolver> callback;

        public BuildCallbackWithState(T state, Action<T, IObjectResolver> callback)
        {
            this.state = state;
            this.callback = callback;
        }

        public void Call(IObjectResolver resolver)
        {
            callback.Invoke(state, resolver);
        }
    }
}
